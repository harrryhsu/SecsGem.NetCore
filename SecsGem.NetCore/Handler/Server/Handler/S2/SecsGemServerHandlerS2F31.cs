using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(2, 31)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS2F31 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var time = Context.Message.Root.GetString();
            var evt = await Context.Kernel.Emit(new SecsGemSetTimeEvent
            {
                Time = time,
            });
            var res = evt.Success ? 0 : 1;

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem((byte)res))
                    .Build()
            );
        }
    }
}