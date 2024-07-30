using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Hsms;
using System.Globalization;

namespace SecsGem.NetCore.Handler.Server
{
    public class SecsGemStream2Handler : ISecsGemServerStreamHandler
    {
        public async Task S2F13(SecsGemServerRequestContext req)
        {
            IEnumerable<EquipmentConstant> items;
            if (req.Message.Root.Count == 0)
            {
                items = req.Kernel.Feature.EquipmentConstants;
            }
            else
            {
                items = req.Message.Root.GetListItem().Select(x =>
                    req.Kernel.Feature.EquipmentConstants.FirstOrDefault(y => y.Id == x.GetU4())
                ).ToList();
            }

            await req.Kernel.Emit(new SecsGemGetEquipmentConstantEvent
            {
                Params = items
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        items.Select<EquipmentConstant, DataItem>(x => x == null ? new ListDataItem() : new ADataItem(x.Value.ToString())).ToArray()
                    ))
                    .Build()
            );
        }

        public async Task S2F15(SecsGemServerRequestContext req)
        {
            List<EquipmentConstant> ecs = new();
            foreach (var item in req.Message.Root.GetListItem())
            {
                var id = item[0].GetU4();
                if (!int.TryParse(item[1].GetString(), out var intVal))
                {
                    await req.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(req.Message)
                            .Item(new BinDataItem((byte)SECS_RESPONSE.EAC.OneOrMoreValueOutOfRange))
                            .Build()
                    );
                    return;
                }

                var ec = req.Kernel.Feature.EquipmentConstants.FirstOrDefault(x => x.Id == id);
                if (ec == null)
                {
                    await req.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(req.Message)
                            .Item(new BinDataItem((byte)SECS_RESPONSE.EAC.OneOrMoreConstantDoNotExist))
                            .Build()
                    );
                    return;
                }

                if (intVal > ec.Max || intVal < ec.Min)
                {
                    await req.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(req.Message)
                            .Item(new BinDataItem((byte)SECS_RESPONSE.EAC.OneOrMoreValueOutOfRange))
                            .Build()
                    );
                    return;
                }

                ec.Value = intVal;
                ecs.Add(ec);
            }

            await req.Kernel.Emit(new SecsGemSetEquipmentConstantEvent
            {
                Params = ecs
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)SECS_RESPONSE.EAC.Ok))
                    .Build()
            );
        }

        public async Task S2F17(SecsGemServerRequestContext req)
        {
            var now = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ADataItem(now))
                    .Build()
            );
        }

        public async Task S2F29(SecsGemServerRequestContext req)
        {
            IEnumerable<EquipmentConstant> items;
            if (req.Message.Root.Count == 0)
            {
                items = req.Kernel.Feature.EquipmentConstants;
            }
            else
            {
                items = req.Message.Root.GetListItem().Select(x =>
                    req.Kernel.Feature.EquipmentConstants.FirstOrDefault(y => y.Id == x.GetU4())
                    ?? new EquipmentConstant { Id = x.GetU4() }
                ).ToList();
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        items.Select(x => new ListDataItem(
                            new U4DataItem(x.Id),
                            new ADataItem(x.Name),
                            new ADataItem(x.Min.ToString()),
                            new ADataItem(x.Max.ToString()),
                            new ADataItem(x.Default.ToString()),
                            new ADataItem(x.Unit)
                        )).ToArray()
                    ))
                    .Build()
            );
        }

        public async Task S2F31(SecsGemServerRequestContext req)
        {
            var time = req.Message.Root.GetString();
            var evt = await req.Kernel.Emit(new SecsGemSetTimeEvent
            {
                Time = time,
            });
            var res = evt.Success ? 0 : 1;

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)res))
                    .Build()
            );
        }

        public async Task S2F33(SecsGemServerRequestContext req)
        {
            var dataId = req.Message.Root[0].GetU4();
            var reports = req.Message.Root[1].GetListItem();

            if (reports.Count == 0)
            {
                req.Kernel.Feature.CollectionReports.Clear();
                req.Kernel.Feature.CollectionEvents.ForEach(x => x.CollectionReports.Clear());

                await req.ReplyAsync(
                    HsmsMessage.Builder
                        .Reply(req.Message)
                        .Item(new BinDataItem((byte)SECS_RESPONSE.DRACK.Ok))
                        .Build()
                );
                return;
            }

            List<CollectionReport> remove = new();
            List<CollectionReport> add = new();

            foreach (var report in reports)
            {
                var id = report[0].GetU4();
                var dvIds = report[1].GetListItem().Select(x => x.GetString());
                var existing = req.Kernel.Feature.CollectionReports.FirstOrDefault(x => x.Id == id);

                if (dvIds.Count() == 0)
                {
                    if (existing == null)
                    {
                        await req.ReplyAsync(
                           HsmsMessage.Builder
                               .Reply(req.Message)
                               .Item(new BinDataItem((byte)SECS_RESPONSE.DRACK.InvalidFormat))
                               .Build()
                        );
                        return;
                    }
                    else
                    {
                        remove.Add(existing);
                    }
                }
                else
                {
                    if (existing != null)
                    {
                        await req.ReplyAsync(
                           HsmsMessage.Builder
                               .Reply(req.Message)
                               .Item(new BinDataItem((byte)SECS_RESPONSE.DRACK.AlreadyDefined))
                               .Build()
                        );
                        return;
                    }
                    else if (dvIds.Any(x => req.Kernel.Feature.DataVariables.All(y => y.Id != x)))
                    {
                        await req.ReplyAsync(
                          HsmsMessage.Builder
                              .Reply(req.Message)
                              .Item(new BinDataItem((byte)SECS_RESPONSE.DRACK.InvalidVid))
                              .Build()
                        );
                        return;
                    }
                    else
                    {
                        add.Add(new CollectionReport
                        {
                            Id = id,
                            DataVariables = dvIds.Select(x => req.Kernel.Feature.DataVariables.First(y => y.Id == x)).ToList()
                        });
                    }
                }
            }

            remove.ForEach(x =>
            {
                req.Kernel.Feature.CollectionReports.Remove(x);
            });

            add.ForEach(x =>
            {
                req.Kernel.Feature.CollectionReports.Add(x);
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)SECS_RESPONSE.DRACK.Ok))
                    .Build()
            );
        }

        public async Task S2F35(SecsGemServerRequestContext req)
        {
            var dataId = req.Message.Root[0].GetU4();
            var events = req.Message.Root[1].GetListItem();

            if (events.Count == 0)
            {
                req.Kernel.Feature.CollectionEvents.ForEach(x => x.CollectionReports.Clear());
                await req.ReplyAsync(
                   HsmsMessage.Builder
                       .Reply(req.Message)
                       .Item(new BinDataItem((byte)SECS_RESPONSE.LRACK.Ok))
                       .Build()
                );
                return;
            }

            foreach (var evt in events)
            {
                var id = evt[0].GetU4();
                var rpids = evt[1].GetListItem().Select(x => x.GetU4());
                var existing = req.Kernel.Feature.CollectionEvents.FirstOrDefault(x => x.Id == id);

                if (existing == null)
                {
                    await req.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(req.Message)
                            .Item(new BinDataItem((byte)SECS_RESPONSE.LRACK.OneOrMoreCeidInvalid))
                            .Build()
                    );
                    return;
                }
                else if (rpids.Any(x => req.Kernel.Feature.CollectionReports.All(y => y.Id != x)))
                {
                    await req.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(req.Message)
                            .Item(new BinDataItem((byte)SECS_RESPONSE.LRACK.OneOrMoreRptidInvalid))
                            .Build()
                    );
                    return;
                }
                else
                {
                    foreach (var rpid in rpids)
                    {
                        var existingReport = existing.CollectionReports.FirstOrDefault(x => x.Id == rpid);
                        if (existingReport != null)
                        {
                            await req.ReplyAsync(
                                HsmsMessage.Builder
                                    .Reply(req.Message)
                                    .Item(new BinDataItem((byte)SECS_RESPONSE.LRACK.OneOrMoreCeidAlreadyDefined))
                                    .Build()
                            );
                            return;
                        }
                    }
                }
            }

            foreach (var evt in events)
            {
                var id = evt[0].GetU4();
                var rpids = evt[1].GetListItem().Select(x => x.GetU4());
                var existing = req.Kernel.Feature.CollectionEvents.FirstOrDefault(x => x.Id == id);

                if (rpids.Count() == 0)
                {
                    existing.CollectionReports.Clear();
                }
                else
                {
                    foreach (var rpid in rpids)
                    {
                        var existingReport = req.Kernel.Feature.CollectionReports.First(x => x.Id == rpid);
                        existing.CollectionReports.Add(existingReport);
                    }
                }
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)SECS_RESPONSE.LRACK.Ok))
                    .Build()
            );
        }

        public async Task S2F37(SecsGemServerRequestContext req)
        {
            var enabled = req.Message.Root[0].GetBool();
            var events = req.Message.Root[1].GetListItem().Select(x => req.Kernel.Feature.CollectionEvents.FirstOrDefault(y => y.Id == x.GetU4())).ToList();

            var valid = !events.Any(x => x == null);
            if (valid)
            {
                events.ForEach(x => x.Enabled = enabled);
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)(valid ? 0x1 : 0x0)))
                    .Build()
            );
        }

        public async Task S2F41(SecsGemServerRequestContext req)
        {
            var cmd = req.Message.Root[0].GetString();
            var param = req.Message.Root[1].GetListItem().ToDictionary(x => x[0].GetString(), x => x[1].GetString());
            var existing = req.Kernel.Feature.Commands.FirstOrDefault(x => x.Name == cmd);
            SECS_RESPONSE.HCACK ret;

            if (existing == null)
            {
                ret = SECS_RESPONSE.HCACK.InvalidCommand;
            }
            else
            {
                var evt = await req.Kernel.Emit(new SecsGemCommandExecuteEvent
                {
                    Cmd = existing,
                    Params = param,
                });
                ret = evt.Return;
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        new BinDataItem((byte)ret),
                        new ListDataItem()
                    ))
                    .Build()
            );
        }

        public async Task S2F51(SecsGemServerRequestContext req)
        {
            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        req.Kernel.Feature.CollectionReports.Select(x => new U4DataItem(x.Id)).ToArray()
                    ))
                    .Build()
            );
        }

        public async Task S2F53(SecsGemServerRequestContext req)
        {
            List<CollectionReport> items;
            if (req.Message.Root.Count == 0)
            {
                items = req.Kernel.Feature.CollectionReports;
            }
            else
            {
                items = req.Message.Root.GetListItem().Select(x =>
                    req.Kernel.Feature.CollectionReports.FirstOrDefault(y => y.Id == x.GetU4())
                    ?? new CollectionReport { Id = x.GetU4() }
                ).ToList();
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        items.Select(x => new ListDataItem(
                            new U4DataItem(x.Id),
                            new ListDataItem(
                                x.DataVariables.Select(x => new ADataItem(x.Id)).ToArray())
                            )
                        ).ToArray()
                    ))
                    .Build()
            );
        }

        public async Task S2F55(SecsGemServerRequestContext req)
        {
            List<CollectionEvent> items;
            if (req.Message.Root.Count == 0)
            {
                items = req.Kernel.Feature.CollectionEvents;
            }
            else
            {
                items = req.Message.Root.GetListItem().Select(x =>
                    req.Kernel.Feature.CollectionEvents.FirstOrDefault(y => y.Id == x.GetU4())
                    ?? new CollectionEvent { Id = x.GetU4() }
                ).ToList();
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        items.Select(x => new ListDataItem(
                            new U4DataItem(x.Id),
                            new ADataItem(x.Name),
                            new ListDataItem(
                                x.CollectionReports.Select(x => new U4DataItem(x.Id)).ToArray())
                            )
                        ).ToArray()
                    ))
                    .Build()
            );
        }

        public async Task S2F57(SecsGemServerRequestContext req)
        {
            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        req.Kernel.Feature.CollectionEvents.Where(x => x.Enabled).Select(x => new U4DataItem(x.Id)).ToArray()
                    ))
                    .Build()
            );
        }
    }
}