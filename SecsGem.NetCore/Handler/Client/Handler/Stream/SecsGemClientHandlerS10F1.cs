using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(10, 1)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemClientHandlerS10F1 : SecsGemClientStreamHandler
    {
        public override async Task Execute()
        {
            var id = Context.Message.Root[0].GetBin();
            var text = Context.Message.Root[1].GetString();

            var evt = await Context.Kernel.Emit(new SecsGemTerminalDisplayEvent
            {
                Id = id,
                Text = new string[] { text },
            });

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem((byte)evt.Return))
                    .Build()
            );
        }
    }
}