using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    public class SecsGemStream6Handler : ISecsGemServerStreamHandler
    {
        public async Task S6F5(SecsGemServerRequestContext req)
        {
            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem(0x0))
                    .Build()
            );
        }

        public async Task S6F15(SecsGemServerRequestContext req)
        {
            var id = req.Message.Root.GetU4();
            var ce = req.Kernel.Feature.CollectionEvents.FirstOrDefault(x => x.Id == id) ?? new CollectionEvent { Id = id };

            await req.Kernel.Emit(new SecsGemGetDataVariableEvent
            {
                Params = ce.CollectionReports.SelectMany(x => x.DataVariables)
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        new U4DataItem(0),
                        new U4DataItem(id),
                        new ListDataItem(
                            ce.CollectionReports
                                .Select(x => new ListDataItem(
                                    new U4DataItem(x.Id),
                                    new ListDataItem(
                                        x.DataVariables
                                        .Select(x => new ADataItem(x.Value))
                                        .ToArray()
                                )
                            )).ToArray()
                        )
                    ))
                    .Build()
            );
        }
    }
}