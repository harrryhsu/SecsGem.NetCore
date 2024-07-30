using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(10, 5)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS10F5 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var id = Context.Message.Root[0].GetBin();
            var texts = Context.Message.Root[1].GetListItem().Select(x => x.GetString()).ToList();

            var arg = await Context.Kernel.Emit(new SecsGemTerminalDisplayEvent
            {
                Id = id,
                Text = texts
            });

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem((byte)(arg.Return)))
                    .Build()
            );
        }
    }
}