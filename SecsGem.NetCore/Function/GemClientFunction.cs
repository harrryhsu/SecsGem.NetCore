using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Hsms;
using static SecsGem.NetCore.Handler.Server.SecsGemStream2Handler;
using static SecsGem.NetCore.Handler.Server.SecsGemStream7Handler;

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
                    Message = "Server rejected select request",
                });
                return false;
            }
            else
            {
                _kernel.Device.IsSelected = true;
                return true;
            }
        }

        public async Task Deselect(CancellationToken ct = default)
        {
            await _tcp.SendAsync(
                HsmsMessage.Builder
                    .Type(HsmsMessageType.DeselectReq)
                    .Build(),
                ct
            );
        }

        public async Task Separate(CancellationToken ct = default)
        {
            try
            {
                if (_tcp.Online)
                    await _tcp.SendAsync(
                        HsmsMessage.Builder
                            .Type(HsmsMessageType.SeparateReq)
                            .Build(),
                        ct
                    );
            }
            catch { }
        }

        public async Task<bool> CommunicationEstablish(CancellationToken ct = default)
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
                    Message = "Server rejected communication online request",
                });
                return false;
            }
            else
            {
                await _kernel.SetCommunicationState(CommunicationStateModel.CommunicationOnline, true);
                _kernel.Device.Model = msg.Root[1][0].GetString();
                _kernel.Device.Revision = msg.Root[1][1].GetString();
                return true;
            }
        }

        public async Task<bool> IsEquipmentControlOnline(CancellationToken ct = default)
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

        public async Task ControlOffline(CancellationToken ct = default)
        {
            await _tcp.SendAndWaitForReplyAsync(
               HsmsMessage.Builder
                   .Stream(1)
                   .Func(15)
                   .Build(),
               ct
            );

            _kernel.Device.ControlState = ControlStateModel.ControlHostOffLine;
        }

        public async Task<bool> ControlOnline(CancellationToken ct = default)
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
                _kernel.Device.ControlState = ControlStateModel.ControlOnline;

            return ack;
        }

        public async Task<IEnumerable<StatusVariable>> GetStatusVariableDefinitions(IEnumerable<uint> ids = null, CancellationToken ct = default)
        {
            if (ids == null) ids = Array.Empty<uint>();

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

        public async Task<Dictionary<uint, string>> GetStatusVariableValues(IEnumerable<uint> ids, CancellationToken ct = default)
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

        public async Task<IEnumerable<DataVariable>> GetDataVariableDefinitions(IEnumerable<string> ids = null, CancellationToken ct = default)
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

        public async Task<S2F15_EAC> SetEquipmentConstants(IEnumerable<EquipmentConstant> ecs, CancellationToken ct = default)
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

            return (S2F15_EAC)msg.Root.GetBin();
        }

        public async Task<IEnumerable<EquipmentConstant>> GetEquipmentConstantDefinitions(IEnumerable<uint> ids = null, CancellationToken ct = default)
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

        public async Task<Dictionary<uint, int>> GetEquipmentConstantValues(IEnumerable<uint> ids = null, CancellationToken ct = default)
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

        public async Task<IEnumerable<CollectionEvent>> GetCollectionEventDefinitions(IEnumerable<uint> ids = null, CancellationToken ct = default)
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

        public async Task<S2F34_DRACK> DefineCollectionReport(uint dataId, IEnumerable<CollectionReport> report, CancellationToken ct = default)
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

            var ack = (S2F34_DRACK)msg.Root.GetBin();
            return ack;
        }

        public async Task<S2F36_LRACK> LinkCollectionReport(uint dataId, IEnumerable<CollectionEvent> ces, CancellationToken ct = default)
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

            var ack = (S2F36_LRACK)msg.Root.GetBin();
            return ack;
        }

        public async Task<IEnumerable<CollectionEvent>> GetCollectionEventLinks(IEnumerable<uint> ids = null, CancellationToken ct = default)
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

        public async Task<bool> ToggleCollectionEvent(bool active, IEnumerable<uint> ids, CancellationToken ct = default)
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

        public async Task<IEnumerable<uint>> GetEnabledCollectionEventIds(CancellationToken ct = default)
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

        public async Task<IEnumerable<uint>> GetCollectionReportIds(CancellationToken ct = default)
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

        public async Task<IEnumerable<CollectionReport>> GetCollectionReportDefinitions(IEnumerable<uint> ids = null, CancellationToken ct = default)
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

        public async Task<S2F42_HCACK> CommandSend(string name, Dictionary<string, string> param, CancellationToken ct = default)
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

            var ack = (S2F42_HCACK)msg.Root[0].GetBin();
            return ack;
        }

        public async Task<string> GetServerTime(CancellationToken ct = default)
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

        public async Task<bool> SetServerTime(DateTime time, CancellationToken ct = default)
        {
            return await SetServerTime(time.ToString("o"), ct);
        }

        public async Task<bool> SetServerTime(string time, CancellationToken ct = default)
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

        public async Task ToggleAlarm(uint id, bool active, CancellationToken ct = default)
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

        public async Task<IEnumerable<Alarm>> GetAlarmDefinitions(IEnumerable<uint> ids = null, CancellationToken ct = default)
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

        public async Task<IEnumerable<Alarm>> GetEnabledAlarmDefinitions(CancellationToken ct = default)
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

        public async Task<S7F2_PPGNT> GrantProcessProgramLoad(string id, uint length, CancellationToken ct = default)
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

            var ack = (S7F2_PPGNT)msg.Root.GetBin();
            return ack;
        }

        public async Task<S7F4_ACKC7> LoadProcessProgram(string id, byte[] body, CancellationToken ct = default)
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

            var ack = (S7F4_ACKC7)msg.Root.GetBin();
            return ack;
        }

        public async Task<ProcessProgram> GetProcessProgram(string id, CancellationToken ct = default)
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

        public async Task<IEnumerable<string>> GetProcessPrograms(CancellationToken ct = default)
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

        public async Task<S7F4_ACKC7> DeleteProcessProgram(IEnumerable<string> ids = null, CancellationToken ct = default)
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

            var ack = (S7F4_ACKC7)msg.Root.GetBin();
            return ack;
        }
    }
}