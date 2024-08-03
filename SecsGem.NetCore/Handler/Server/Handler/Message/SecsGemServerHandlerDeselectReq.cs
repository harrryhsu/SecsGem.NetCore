using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;
using SecsGem.NetCore.State.Server;

namespace SecsGem.NetCore.Handler.Server.Handler.Message
{
    [SecsGemMessage(HsmsMessageType.DeselectReq)]
    public class SecsGemServerHandlerDeselectReq : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            await Context.Kernel.State.TriggerAsync(GemServerStateTrigger.Deselect, true);
            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Func(0)
                    .Type(HsmsMessageType.DeselectRsp)
                    .Build()
            );
        }
    }
}