using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(2, 55)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS2F55 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            List<CollectionEvent> items;
            if (Context.Message.Root.Count == 0)
            {
                items = Context.Kernel.Feature.CollectionEvents;
            }
            else
            {
                items = Context.Message.Root.GetListItem().Select(x =>
                    Context.Kernel.Feature.CollectionEvents.FirstOrDefault(y => y.Id == x.GetU4())
                    ?? new CollectionEvent { Id = x.GetU4() }
                ).ToList();
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
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
    }
}