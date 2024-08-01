using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server.Handler.Message
{
    [SecsGemMessage(HsmsMessageType.SeparateReq)]
    public class SecsGemServerHandlerSeperateReq : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            await Context.Kernel.State.TriggerAsync(GemServerStateTrigger.Disconnect, true);
        }
    }
}