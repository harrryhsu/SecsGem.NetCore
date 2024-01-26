using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Client
{
    public class SecsGemClientStreamHandler : ISecsGemClientStreamHandler
    {
        public async Task S1F13(SecsGemClientRequestContext req)
        {
            var success = await req.Kernel.SetCommunicationState(CommunicationStateModel.CommunicationOnline);
            byte res = (byte)(success ? 0 : 1);

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        new BinDataItem(res),
                        new ListDataItem(
                            new ADataItem(req.Kernel.Device.Model),
                            new ADataItem(req.Kernel.Device.Revision)
                        )
                    ))
                    .Build()
            );
        }

        public async Task S5F1(SecsGemClientRequestContext req)
        {
            var id = req.Message.Root[1].GetU4();
            var description = req.Message.Root[2].GetString();

            await req.Kernel.Emit(new SecsGemAlarmEvent
            {
                Alarm = new Alarm
                {
                    Description = description,
                    Id = id,
                    Enabled = true
                }
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem(0))
                    .Build()
            );
        }

        public async Task S5F9(SecsGemClientRequestContext req)
        {
            await req.Kernel.Emit(new SecsGemNotifyExceptionEvent
            {
                Timestamp = req.Message.Root[0].GetString(),
                Id = req.Message.Root[1].GetString(),
                Type = req.Message.Root[2].GetString(),
                Message = req.Message.Root[3].GetString(),
                RecoveryMessage = string.Join("", req.Message.Root[4].GetListItem().Select(x => x.GetString())),
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Build()
            );
        }

        public async Task S6F11(SecsGemClientRequestContext req)
        {
            var dataId = req.Message.Root[0].GetU4();
            var ceid = req.Message.Root[1].GetU4();

            var ce = new CollectionEvent
            {
                Id = ceid,
                CollectionReports = req.Message.Root[2]
                    .GetListItem()
                    .Select(report => new CollectionReport
                    {
                        Id = report[0].GetU4(),
                        DataVariables = report[1]
                            .GetListItem()
                            .Select(dv => new DataVariable
                            {
                                Value = dv.GetString(),
                            })
                            .ToList()
                    })
                    .ToList()
            };

            await req.Kernel.Emit(new SecsGemCollectionEventEvent
            {
                CollectionEvent = ce,
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem(0))
                    .Build()
            );
        }

        public async Task S10F1(SecsGemClientRequestContext req)
        {
            var id = req.Message.Root[0].GetBin();
            var text = req.Message.Root[1].GetString();

            var evt = await req.Kernel.Emit(new SecsGemTerminalDisplayEvent
            {
                Id = id,
                Text = new string[] { text },
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)evt.Return))
                    .Build()
            );
        }
    }
}