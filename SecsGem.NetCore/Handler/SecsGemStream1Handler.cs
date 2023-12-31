﻿using SecsGem.NetCore.Event;
using SecsGem.NetCore.Feature;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler
{
    public class SecsGemStream1Handler : ISecsGemStreamHandler
    {
        public async Task S1F1(SecsGemRequestContext req)
        {
            if (req.Kernel.Device.IsControlOnline)
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
                        .Stream(1)
                        .Func(0)
                        .Build()
                );
            }
        }

        public async Task S1F3(SecsGemRequestContext req)
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

        public async Task S1F11(SecsGemRequestContext req)
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

        public async Task S1F13(SecsGemRequestContext req)
        {
            var success = await req.Kernel.SetCommunicationState(CommunicationStateModel.CommunicationOnline);
            var res = (byte)(success ? 0x0 : 0x1);

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

        public async Task S1F15(SecsGemRequestContext req)
        {
            await req.Kernel.SetCommunicationState(CommunicationStateModel.CommunicationOffline);
            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem(0x0))
                    .Build()
            );
        }

        public async Task S1F17(SecsGemRequestContext req)
        {
            byte res;
            if (req.Kernel.Device.ControlState == ControlStateModel.ControlOffLine)
            {
                if (await req.Kernel.SetControlState(req.Kernel.Device.InitControlState))
                {
                    res = 0x0;
                }
                else
                {
                    res = 0x1;
                }
            }
            else
            {
                res = 0x2;
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem(res))
                    .Build()
            );
        }

        public async Task S1F21(SecsGemRequestContext req)
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

        public async Task S1F23(SecsGemRequestContext req)
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