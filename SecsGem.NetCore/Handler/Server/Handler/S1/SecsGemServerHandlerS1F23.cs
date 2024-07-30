using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(1, 23)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS1F23 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            IEnumerable<CollectionEvent> items;
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