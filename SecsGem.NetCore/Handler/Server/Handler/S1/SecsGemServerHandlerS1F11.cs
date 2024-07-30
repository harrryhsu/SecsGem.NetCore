using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(1, 11)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS1F11 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            IEnumerable<StatusVariable> items;
            if (Context.Message.Root.Count == 0)
            {
                items = Context.Kernel.Feature.StatusVariables;
            }
            else
            {
                items = Context.Message.Root.GetListItem().Select(x =>
                    Context.Kernel.Feature.StatusVariables.FirstOrDefault(y => y.Id == x.GetU4())
                    ?? new StatusVariable { Id = x.GetU4() }
                ).ToList();
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
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
    }
}