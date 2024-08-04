using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Error;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Hsms;
using SecsGem.NetCore.State.Client;

namespace SecsGem.NetCore.Function
{
    public class GemClientFunction
    {
        private readonly SecsGemClient _kernel;

        private readonly SecsGemTcpClient _tcp;

        public GemClientFunction(SecsGemClient kernel)
        {
            _kernel = kernel;
            _tcp = kernel._tcp;
        }

        public async Task<bool> Select(CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Type(HsmsMessageType.SelectReq)
                    .Build(),
                ct
            );

            if (msg.Header.SType != HsmsMessageType.SelectRsp || msg.Header.F != 0)
            {
                await _kernel.Emit(new SecsGemErrorEvent
                {
                    Message = "Server rejected select Contextuest",
                });
                return false;
            }
            else
            {
                await _kernel.State.TriggerAsync(GemClientStateTrigger.Select, true);
                return true;
            }
        }

        public async Task<bool> Deselect(CancellationToken ct = default)
        {
            var res = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Type(HsmsMessageType.DeselectReq)
                    .Build(),
                ct
            );

            return res.Header.SType == HsmsMessageType.DeselectRsp;
        }

        public async Task Separate(CancellationToken ct = default)
        {
            try
            {
                if (_tcp.Online)
                {
                    await _tcp.SendAsync(
                       HsmsMessage.Builder
                           .Type(HsmsMessageType.SeparateReq)
                           .Build(),
                       ct
                    );
                    await _kernel.State.TriggerAsync(GemClientStateTrigger.Disconnect, true);
                }
            }
            catch { }
        }

        public async Task<bool> S1F13EstablishCommunicationsRequest(CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(1)
                    .Func(13)
                    .Build(),
                ct
            );

            var ack = msg.Root[0].GetBin();
            if (ack == 1)
            {
                await _kernel.Emit(new SecsGemErrorEvent
                {
                    Message = "Server rejected communication online Contextuest",
                });
                return false;
            }
            else
            {
                await _kernel.State.TriggerAsync(GemClientStateTrigger.EstablishCommunication, true);
                _kernel.Device.Model = msg.Root[1][0].GetString();
                _kernel.Device.Revision = msg.Root[1][1].GetString();
                return true;
            }
        }

        public async Task<bool> S1F1AreYouOnline(CancellationToken ct = default)
        {
            try
            {
                await _tcp.SendAndWaitForReplyAsync(
                   HsmsMessage.Builder
                       .Stream(1)
                       .Func(1)
                       .Build(),
                   ct
                );
            }
            catch (SecsGemTransactionException)
            {
                return false;
            }

            return true;
        }

        public async Task S1F15RequestOffLine(CancellationToken ct = default)
        {
            await _tcp.SendAndWaitForReplyAsync(
               HsmsMessage.Builder
                   .Stream(1)
                   .Func(15)
                   .Build(),
               ct
            );

            await _kernel.State.TriggerAsync(GemClientStateTrigger.GoOffline, true);
        }

        public async Task<bool> S1F17RequestOnline(CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
               HsmsMessage.Builder
                   .Stream(1)
                   .Func(17)
                   .Build(),
               ct
            );
            var ack = msg.Root.GetBin() != 1;

            if (ack)
            {
                await _kernel.State.TriggerAsync(GemClientStateTrigger.GoOnline, true);
            }

            return ack;
        }

        public async Task<IEnumerable<StatusVariable>> S1F11StatusVariableNamelistRequest(IEnumerable<uint> ids = null, CancellationToken ct = default)
        {
            ids ??= Array.Empty<uint>();

            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(1)
                    .Func(11)
                    .Item(new ListDataItem(ids.Select(x => new U4DataItem(x)).ToArray()))
                    .Build(),
                ct
            );

            List<StatusVariable> svs = new();
            for (var i = 0; i < msg.Root.Count; i++)
            {
                svs.Add(new StatusVariable
                {
                    Id = msg.Root[i][0].GetU4(),
                    Name = msg.Root[i][1].GetString(),
                    Unit = msg.Root[i][2].GetString(),
                });
            }

            return svs;
        }

        public async Task<Dictionary<uint, string>> S1F3SelectedEquipmentStatusRequest(IEnumerable<uint> ids, CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(1)
                    .Func(3)
                    .Item(new ListDataItem(ids.Select(x => new U4DataItem((uint)x)).ToArray()))
                    .Build(),
                ct
            );

            Dictionary<uint, string> values = new();
            for (var i = 0; i < msg.Root.Count; i++)
            {
                values[ids.ElementAt(i)] = msg.Root[i].GetString();
            }

            return values;
        }

        public async Task<IEnumerable<DataVariable>> S1F21DataVariableNamelistRequest(IEnumerable<string> ids = null, CancellationToken ct = default)
        {
            if (ids == null) ids = Array.Empty<string>();

            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(1)
                    .Func(21)
                    .Item(new ListDataItem(ids.Select(x => new ADataItem(x)).ToArray()))
                    .Build(),
                ct
            );

            List<DataVariable> dvs = new();
            for (var i = 0; i < msg.Root.Count; i++)
            {
                dvs.Add(new DataVariable
                {
                    Id = msg.Root[i][0].GetString(),
                    Description = msg.Root[i][1].GetString(),
                    Unit = msg.Root[i][2].GetString(),
                });
            }

            return dvs;
        }

        public async Task<SECS_RESPONSE.EAC> S2F15NewEquipmentConstantSend(IEnumerable<EquipmentConstant> ecs, CancellationToken ct = default)
        {
            if (!ecs.Any()) return 0;

            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(15)
                    .Item(new ListDataItem(
                        ecs.Select(ec => new ListDataItem(
                            new U4DataItem(ec.Id),
                            new ADataItem(ec.Value.ToString())
                        )).ToArray()
                    ))
                    .Build(),
                ct
            );

            return (SECS_RESPONSE.EAC)msg.Root.GetBin();
        }

        public async Task<IEnumerable<EquipmentConstant>> S2F29EquipmentConstantNamelistRequest(IEnumerable<uint> ids = null, CancellationToken ct = default)
        {
            if (ids == null) ids = Array.Empty<uint>();

            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(29)
                    .Item(new ListDataItem(ids.Select(x => new U4DataItem(x)).ToArray()))
                    .Build(),
                ct
            );

            List<EquipmentConstant> svs = new();
            for (var i = 0; i < msg.Root.Count; i++)
            {
                svs.Add(new EquipmentConstant
                {
                    Id = msg.Root[i][0].GetU4(),
                    Name = msg.Root[i][1].GetString(),
                    Min = int.Parse(msg.Root[i][2].GetString()),
                    Max = int.Parse(msg.Root[i][3].GetString()),
                    Default = int.Parse(msg.Root[i][4].GetString()),
                    Unit = msg.Root[i][5].GetString(),
                });
            }

            return svs;
        }

        public async Task<Dictionary<uint, int>> S2F13EquipmentConstantRequest(IEnumerable<uint> ids = null, CancellationToken ct = default)
        {
            if (ids == null) ids = Array.Empty<uint>();

            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(13)
                    .Item(new ListDataItem(ids.Select(x => new U4DataItem(x)).ToArray()))
                    .Build(),
                ct
            );

            Dictionary<uint, int> values = new();
            for (var i = 0; i < msg.Root.Count; i++)
            {
                values[ids.ElementAt(i)] = int.Parse(msg.Root[i].GetString());
            }

            return values;
        }

        public async Task<IEnumerable<CollectionEvent>> S1F23CollectionEventNamelistRequest(IEnumerable<uint> ids = null, CancellationToken ct = default)
        {
            if (ids == null) ids = Array.Empty<uint>();

            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(1)
                    .Func(23)
                    .Item(new ListDataItem(ids.Select(x => new U4DataItem(x)).ToArray()))
                    .Build(),
                ct
            );

            List<CollectionEvent> dvs = new();
            for (var i = 0; i < msg.Root.Count; i++)
            {
                dvs.Add(new CollectionEvent
                {
                    Id = msg.Root[i][0].GetU4(),
                    Name = msg.Root[i][1].GetString(),
                    CollectionReports = new()
                    {
                        new CollectionReport
                        {
                            DataVariables = msg.Root[i][2]
                                .GetListItem()
                                .Select(x => new DataVariable { Id = x.GetString() })
                                .ToList()
                        }
                    }
                });
            }

            return dvs;
        }

        public async Task<SECS_RESPONSE.DRACK> S2F33DefineReport(uint dataId, IEnumerable<CollectionReport> report, CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(33)
                    .Item(
                        new ListDataItem(
                            new U4DataItem(dataId),
                            new ListDataItem(
                                report
                                    .Select(
                                        x => new ListDataItem(
                                            new U4DataItem(x.Id),
                                            new ListDataItem(
                                                x.DataVariables.Select(dv => new ADataItem(dv.Id)).ToArray()
                                            )
                                        )
                                    )
                                    .ToArray()
                            )
                        )
                    )
                    .Build(),
                ct
            );

            var ack = (SECS_RESPONSE.DRACK)msg.Root.GetBin();
            return ack;
        }

        public async Task<SECS_RESPONSE.LRACK> S2F35LinkEventReport(uint dataId, IEnumerable<CollectionEvent> ces, CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(35)
                    .Item(
                        new ListDataItem(
                            new U4DataItem(dataId),
                            new ListDataItem(
                                ces
                                    .Select(
                                        x => new ListDataItem(
                                            new U4DataItem(x.Id),
                                            new ListDataItem(
                                                x.CollectionReports.Select(report => new U4DataItem(report.Id)).ToArray()
                                            )
                                        )
                                    )
                                    .ToArray()
                            )
                        )
                    )
                    .Build(),
                ct
            );

            var ack = (SECS_RESPONSE.LRACK)msg.Root.GetBin();
            return ack;
        }

        public async Task<IEnumerable<CollectionEvent>> S2F55RequestEventReportLinks(IEnumerable<uint> ids = null, CancellationToken ct = default)
        {
            if (ids == null) ids = Array.Empty<uint>();

            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(55)
                    .Item(new ListDataItem(
                        ids.Select(x => new U4DataItem(x)).ToArray()
                    ))
                    .Build(),
                ct
            );

            var res = msg.Root.GetListItem().Select(x => new CollectionEvent
            {
                Id = x[0].GetU4(),
                Name = x[1].GetString(),
                CollectionReports = x[2].GetListItem()
                    .Select(report => new CollectionReport { Id = report.GetU4() })
                    .ToList(),
            }).ToList();

            return res;
        }

        public async Task<bool> S2F37EnableDisableEventReport(bool active, IEnumerable<uint> ids, CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(37)
                    .Item(
                        new ListDataItem(
                            new BoolDataItem(active),
                            new ListDataItem(
                                ids
                                    .Select(id => new U4DataItem(id))
                                    .ToArray()
                            )
                        )
                    )
                    .Build(),
                ct
            );

            var ack = msg.Root[0].GetBin() == 0;
            return ack;
        }

        public async Task<IEnumerable<uint>> S2F57RequestEnabledEvents(CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(57)
                    .Build(),
                ct
            );

            var res = msg.Root.GetListItem().Select(x => x.GetU4()).ToList();
            return res;
        }

        public async Task<IEnumerable<uint>> S2F51RequestReportIdentifiers(CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(51)
                    .Build(),
                ct
            );

            var res = msg.Root.GetListItem().Select(x => x.GetU4()).ToList();
            return res;
        }

        public async Task<IEnumerable<CollectionReport>> S2F53RequestReportDefinitions(IEnumerable<uint> ids = null, CancellationToken ct = default)
        {
            if (ids == null) ids = Array.Empty<uint>();

            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(53)
                    .Item(new ListDataItem(
                        ids.Select(x => new U4DataItem(x)).ToArray()
                    ))
                    .Build(),
                ct
            );

            var res = msg.Root.GetListItem().Select(x => new CollectionReport
            {
                Id = x[0].GetU4(),
                DataVariables = x[1]
                    .GetListItem()
                    .Select(dv => new DataVariable { Id = dv.GetString() })
                    .ToList()
            }).ToList();

            return res;
        }

        public async Task<SECS_RESPONSE.HCACK> S2F41HostCommandSend(string name, Dictionary<string, string> param, CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(41)
                    .Item(
                        new ListDataItem(
                            new ADataItem(name),
                            new ListDataItem(
                                param.Select(x => new ListDataItem(
                                    new ADataItem(x.Key),
                                    new ADataItem(x.Value)
                                )).ToArray()
                            )
                        )
                    )
                    .Build(),
                ct
            );

            var ack = (SECS_RESPONSE.HCACK)msg.Root[0].GetBin();
            return ack;
        }

        public async Task<string> S2F17DateAndTimeRequest(CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(17)
                    .Build(),
                ct
            );

            return msg.Root.GetString();
        }

        public async Task<bool> S2F31DateAndTimeSetRequest(DateTime time, CancellationToken ct = default)
        {
            return await S2F31DateAndTimeSetRequest(time.ToString("o"), ct);
        }

        public async Task<bool> S2F31DateAndTimeSetRequest(string time, CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(2)
                    .Func(31)
                    .Item(new ADataItem(time))
                    .Build(),
                ct
            );

            var ack = msg.Root.GetBin() == 0;
            return ack;
        }

        public async Task S5F3EnableDisableAlarmSend(uint id, bool active, CancellationToken ct = default)
        {
            await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(5)
                    .Func(3)
                    .Item(new ListDataItem(
                        new BinDataItem((byte)(active ? 128 : 0)),
                        new U4DataItem(id)
                    ))
                    .Build(),
                ct
            );
        }

        public async Task<IEnumerable<Alarm>> S5F5ListAlarmsRequest(IEnumerable<uint> ids = null, CancellationToken ct = default)
        {
            if (ids == null) ids = Array.Empty<uint>();

            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(5)
                    .Func(5)
                    .Item(new U4DataItem(ids.ToArray()))
                    .Build(),
                ct
            );

            var alarms = msg.Root.GetListItem().Select(x => new Alarm
            {
                Id = x[1].GetU4(),
                Enabled = x[0].GetBin() >= 128,
                Description = x[2].GetString(),
            });

            return alarms;
        }

        public async Task<IEnumerable<Alarm>> S5F7ListEnabledAlarmRequest(CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(5)
                    .Func(7)
                    .Build(),
                ct
            );

            var alarms = msg.Root.GetListItem().Select(x => new Alarm
            {
                Id = x[1].GetU4(),
                Enabled = x[0].GetBin() >= 128,
                Description = x[2].GetString(),
            });

            return alarms;
        }

        public async Task<SECS_RESPONSE.PPGNT> S7F1ProcessProgramLoadInquire(string id, uint length, CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(7)
                    .Func(1)
                    .Item(
                        new ListDataItem(
                            new ADataItem(id),
                            new U4DataItem(length)
                        )
                    )
                    .Build(),
                ct
            );

            var ack = (SECS_RESPONSE.PPGNT)msg.Root.GetBin();
            return ack;
        }

        public async Task<SECS_RESPONSE.ACKC7> S7F3ProcessProgramSend(string id, byte[] body, CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(7)
                    .Func(3)
                    .Item(
                        new ListDataItem(
                            new ADataItem(id),
                            new BinDataItem(body)
                        )
                    )
                    .Build(),
                ct
            );

            var ack = (SECS_RESPONSE.ACKC7)msg.Root.GetBin();
            return ack;
        }

        public async Task<ProcessProgram> S7F5ProcessProgramRequest(string id, CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(7)
                    .Func(5)
                    .Item(
                        new ADataItem(id)
                    )
                    .Build(),
                ct
            );

            var pp = new ProcessProgram
            {
                Id = msg.Root[0].GetString(),
                Body = msg.Root[1].GetBins(),
            };

            return pp;
        }

        public async Task<IEnumerable<string>> S7F19CurrentProcessProgramDirRequest(CancellationToken ct = default)
        {
            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(7)
                    .Func(19)
                    .Build(),
                ct
            );

            var ppids = msg.Root.GetListItem().Select(x => x.GetString()).ToList();
            return ppids;
        }

        public async Task<SECS_RESPONSE.ACKC7> S7F17DeleteProcessProgramSend(IEnumerable<string> ids = null, CancellationToken ct = default)
        {
            if (ids == null) ids = Array.Empty<string>();

            var msg = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(7)
                    .Func(17)
                    .Item(
                        new ListDataItem(ids.Select(x => new ADataItem(x)).ToArray())
                    )
                    .Build(),
                ct
            );

            var ack = (SECS_RESPONSE.ACKC7)msg.Root.GetBin();
            return ack;
        }

        public async Task<SECS_RESPONSE.ACKC10> S10F3TerminalDisplaySingle(byte id, string text, CancellationToken ct = default)
        {
            var reply = await _tcp.SendAndWaitForReplyAsync(
                  HsmsMessage.Builder
                      .Stream(10)
                      .Func(3)
                      .ReplyExpected()
                      .Item(new ListDataItem(
                          new BinDataItem(id),
                          new ADataItem(text)
                      ))
                      .Build(),
                  ct
               );

            var code = reply.Root.GetBin();
            return (SECS_RESPONSE.ACKC10)code;
        }

        public async Task<SECS_RESPONSE.ACKC10> S10F5TerminalDisplayMultiBlock(byte id, IEnumerable<string> texts, CancellationToken ct = default)
        {
            var reply = await _tcp.SendAndWaitForReplyAsync(
                  HsmsMessage.Builder
                      .Stream(10)
                      .Func(5)
                      .ReplyExpected()
                      .Item(new ListDataItem(
                          new BinDataItem(id),
                          new ListDataItem(texts.Select(text => new ADataItem(text)))
                      ))
                      .Build(),
                  ct
               );

            var code = reply.Root.GetBin();
            return (SECS_RESPONSE.ACKC10)code;
        }

        public async Task<SECS_RESPONSE.ACKC10> S10F9Broadcast(string text, CancellationToken ct = default)
        {
            var reply = await _tcp.SendAndWaitForReplyAsync(
                  HsmsMessage.Builder
                      .Stream(10)
                      .Func(9)
                      .ReplyExpected()
                      .Item(new ListDataItem(
                          new ADataItem(text)
                      ))
                      .Build(),
                  ct
               );

            var code = reply.Root.GetBin();
            return (SECS_RESPONSE.ACKC10)code;
        }

        public Task<HsmsMessage> Send(HsmsMessage message, CancellationToken ct = default)
        {
            return _tcp.SendAndWaitForReplyAsync(message, ct);
        }
    }
}