using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(1, 3)]
    [SecsGemFunctionType(SecsGemFunctionType.Communication)]
    public class SecsGemServerHandlerS1F3 : SecsGemServerStreamHandler
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

            await Context.Kernel.Emit(new SecsGemGetStatusVariableEvent
            {
                Params = items
            });

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new ListDataItem(
                        items.Select(x => new ADataItem(x.Value)).ToArray()
                    ))
                    .Build()
            );
        }
    }
}