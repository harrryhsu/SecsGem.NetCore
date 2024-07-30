using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(6, 15)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS6F15 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var id = Context.Message.Root.GetU4();
            var ce = Context.Kernel.Feature.CollectionEvents.FirstOrDefault(x => x.Id == id) ?? new CollectionEvent { Id = id };

            await Context.Kernel.Emit(new SecsGemGetDataVariableEvent
            {
                Params = ce.CollectionReports.SelectMany(x => x.DataVariables)
            });

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
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