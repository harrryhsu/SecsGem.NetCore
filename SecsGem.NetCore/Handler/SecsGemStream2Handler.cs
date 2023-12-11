using SecsGem.NetCore.Event;
using SecsGem.NetCore.Feature;
using SecsGem.NetCore.Hsms;
using System.Globalization;

namespace SecsGem.NetCore.Handler
{
    public class SecsGemStream2Handler : ISecsGemStreamHandler
    {
        public async Task S2F13(SecsGemRequestContext req)
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

            await req.Kernel.Emit(new SecsGemGetEquipmentConstantEvent
            {
                Params = items
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        items.Select(x => new ADataItem(x.Value.ToString())).ToArray()
                    ))
                    .Build()
            );
        }

        public async Task S2F15(SecsGemRequestContext req)
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
                            .Item(new BinDataItem(0x3))
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
                        .Item(new BinDataItem(0x1))
                        .Build()
                    );
                    return;
                }

                if (intVal > ec.Max || intVal < ec.Min)
                {
                    await req.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(req.Message)
                            .Item(new BinDataItem(0x3))
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
                    .Item(new BinDataItem(0x0))
                    .Build()
            );
        }

        public async Task S2F17(SecsGemRequestContext req)
        {
            var now = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ADataItem(now))
                    .Build()
            );
        }

        public async Task S2F29(SecsGemRequestContext req)
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

        public async Task S2F31(SecsGemRequestContext req)
        {
            var time = req.Message.Root.GetString();
            byte res;
            if (DateTime.TryParse(time, out var date))
            {
                // Set Time
                res = 0x0;
            }
            else
            {
                res = 0x1;
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem(res))
                    .Build()
            );
        }

        public async Task S2F33(SecsGemRequestContext req)
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
                        .Item(new BinDataItem(0x0))
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
                               .Item(new BinDataItem(0x2))
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
                               .Item(new BinDataItem(0x3))
                               .Build()
                        );
                        return;
                    }
                    else if (dvIds.Any(x => req.Kernel.Feature.DataVariables.All(y => y.Id != x)))
                    {
                        await req.ReplyAsync(
                          HsmsMessage.Builder
                              .Reply(req.Message)
                              .Item(new BinDataItem(0x4))
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
                    .Item(new BinDataItem(0x0))
                    .Build()
            );
        }

        public async Task S2F35(SecsGemRequestContext req)
        {
            var dataId = req.Message.Root[0].GetU4();
            var events = req.Message.Root[1].GetListItem();

            if (events.Count == 0)
            {
                req.Kernel.Feature.CollectionEvents.ForEach(x => x.CollectionReports.Clear());
                await req.ReplyAsync(
                   HsmsMessage.Builder
                       .Reply(req.Message)
                       .Item(new BinDataItem(0x0))
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
                            .Item(new BinDataItem(0x4))
                            .Build()
                    );
                    return;
                }
                else if (rpids.Any(x => req.Kernel.Feature.CollectionReports.All(y => y.Id != x)))
                {
                    await req.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(req.Message)
                            .Item(new BinDataItem(0x5))
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
                                    .Item(new BinDataItem(0x3))
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
                    .Item(new BinDataItem(0x0))
                    .Build()
            );
        }

        public async Task S2F37(SecsGemRequestContext req)
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

        public async Task S2F41(SecsGemRequestContext req)
        {
            var cmd = req.Message.Root[0].GetString();
            var param = req.Message.Root[1].GetListItem().ToDictionary(x => x[0].GetString(), x => x[1].GetString());
            var existing = req.Kernel.Feature.Commands.FirstOrDefault(x => x.Name == cmd);
            byte ret;

            if (existing == null)
            {
                ret = 0x1;
            }
            else
            {
                var evt = await req.Kernel.Emit(new SecsGemCommandExecuteEvent
                {
                    Cmd = existing,
                    Params = param,
                });
                ret = (byte)evt.Return;
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        new BinDataItem(ret),
                        new ListDataItem()
                    ))
                    .Build()
            );
        }

        public async Task S2F51(SecsGemRequestContext req)
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

        public async Task S2F53(SecsGemRequestContext req)
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

        public async Task S2F55(SecsGemRequestContext req)
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

        public async Task S2F57(SecsGemRequestContext req)
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