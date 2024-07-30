using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(2, 53)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS2F53 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            List<CollectionReport> items;
            if (Context.Message.Root.Count == 0)
            {
                items = Context.Kernel.Feature.CollectionReports;
            }
            else
            {
                items = Context.Message.Root.GetListItem().Select(x =>
                    Context.Kernel.Feature.CollectionReports.FirstOrDefault(y => y.Id == x.GetU4())
                    ?? new CollectionReport { Id = x.GetU4() }
                ).ToList();
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new ListDataItem(
                        items.Select(x => new ListDataItem(
                            new U4DataItem(x.Id),
                            new ListDataItem(
                                x.DataVariables.Select(x => new ADataItem(x.Id)).ToArray())
                            )
                        ).ToArray()
                    ))
                    .Build()
            );
        }
    }
}