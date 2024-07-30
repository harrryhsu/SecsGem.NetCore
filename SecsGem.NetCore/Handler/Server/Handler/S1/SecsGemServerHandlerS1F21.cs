using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(1, 21)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS1F21 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            IEnumerable<DataVariable> items;
            if (Context.Message.Root.Count == 0)
            {
                items = Context.Kernel.Feature.DataVariables;
            }
            else
            {
                items = Context.Message.Root.GetListItem().Select(x =>
                    Context.Kernel.Feature.DataVariables.FirstOrDefault(y => y.Id == x.GetString())
                    ?? new DataVariable { Id = x.GetString() }
                ).ToList();
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
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
    }
}