using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    public class SecsGemStream1Handler : ISecsGemServerStreamHandler
    {
        public async Task S1F1(SecsGemServerRequestContext req)
        {
            if (req.Kernel.Device.ControlState.CanGoOnline)
            {
                await req.ReplyAsync(
                    HsmsMessage.Builder
                        .Reply(req.Message)
                        .Item(new ListDataItem(
                            new ADataItem(req.Kernel.Device.Model),
                            new ADataItem(req.Kernel.Device.Revision)
                        ))
                        .Build()
                );
            }
            else
            {
                await req.ReplyAsync(
                    HsmsMessage.Builder
                        .Reply(req.Message)
                        .Stream(1)
                        .Func(0)
                        .Build()
                );
            }
        }

        public async Task S1F3(SecsGemServerRequestContext req)
        {
            IEnumerable<StatusVariable> items;
            if (req.Message.Root.Count == 0)
            {
                items = req.Kernel.Feature.StatusVariables;
            }
            else
            {
                items = req.Message.Root.GetListItem().Select(x =>
                    req.Kernel.Feature.StatusVariables.FirstOrDefault(y => y.Id == x.GetU4())
                    ?? new StatusVariable { Id = x.GetU4() }
                ).ToList();
            }

            await req.Kernel.Emit(new SecsGemGetStatusVariableEvent
            {
                Params = items
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        items.Select(x => new ADataItem(x.Value)).ToArray()
                    ))
                    .Build()
            );
        }

        public async Task S1F11(SecsGemServerRequestContext req)
        {
            IEnumerable<StatusVariable> items;
            if (req.Message.Root.Count == 0)
            {
                items = req.Kernel.Feature.StatusVariables;
            }
            else
            {
                items = req.Message.Root.GetListItem().Select(x =>
                    req.Kernel.Feature.StatusVariables.FirstOrDefault(y => y.Id == x.GetU4())
                    ?? new StatusVariable { Id = x.GetU4() }
                ).ToList();
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        items.Select(x => new ListDataItem(
                            new U4DataItem(x.Id),
                            new ADataItem(x.Name),
                            new ADataItem(x.Unit)
                        )).ToArray()
                    ))
                    .Build()
            );
        }

        public async Task S1F13(SecsGemServerRequestContext req)
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

        public async Task S1F15(SecsGemServerRequestContext req)
        {
            req.Kernel.Device.ControlState.ChangeControlState(ControlStateModel.ControlHostOffLine);
            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem(0x0))
                    .Build()
            );
        }

        public async Task S1F17(SecsGemServerRequestContext req)
        {
            var res = req.Kernel.Device.ControlState.GoOnline();
            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)res))
                    .Build()
            );
        }

        public async Task S1F21(SecsGemServerRequestContext req)
        {
            IEnumerable<DataVariable> items;
            if (req.Message.Root.Count == 0)
            {
                items = req.Kernel.Feature.DataVariables;
            }
            else
            {
                items = req.Message.Root.GetListItem().Select(x =>
                    req.Kernel.Feature.DataVariables.FirstOrDefault(y => y.Id == x.GetString())
                    ?? new DataVariable { Id = x.GetString() }
                ).ToList();
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        items.Select(x => new ListDataItem(
                            new ADataItem(x.Id),
                            new ADataItem(x.Description),
                            new ADataItem(x.Unit)
                        )).ToArray()
                    ))
                    .Build()
            );
        }

        public async Task S1F23(SecsGemServerRequestContext req)
        {
            IEnumerable<CollectionEvent> items;
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
                                x.CollectionReports
                                    .SelectMany(x => x.DataVariables)
                                    .Select(x => new ADataItem(x.Id))
                                    .ToArray()
                            )
                        )).ToArray()
                    ))
                    .Build()
            );
        }
    }
}