using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server.Handler.Message
{
    [SecsGemMessage(HsmsMessageType.DeselectReq)]
    public class SecsGemServerHandlerDeselectReq : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            await Context.Kernel.State.TriggerAsync(GemServerStateTrigger.Deselect);
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