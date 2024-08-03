using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(10, 9)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS10F9 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var text = Context.Message.Root[0].GetString();

            var arg = await Context.Kernel.Emit(new SecsGemTerminalDisplayEvent
            {
                Texts = new List<string> { text },
                IsBroadcast = true
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