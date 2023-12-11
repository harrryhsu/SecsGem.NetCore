using SecsGem.NetCore.Event;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler
{
    public class SecsGemStream6Handler : ISecsGemStreamHandler
    {
        public async Task S6F5(SecsGemRequestContext req)
        {
            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem(0x0))
                    .Build()
            );
        }

        public async Task S6F15(SecsGemRequestContext req)
        {
            var id = req.Message.Root.GetU4();
            var ce = req.Kernel.Feature.CollectionEvents.FirstOrDefault(x => x.Id == id) ?? new Feature.CollectionEvent { Id = id };

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