using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(10, 3)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS10F3 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var id = Context.Message.Root[0].GetBin();
            var text = Context.Message.Root[1].GetString();

            var arg = await Context.Kernel.Emit(new SecsGemTerminalDisplayEvent
            {
                Id = id,
                Text = new List<string> { text }
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