using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;
using SecsGem.NetCore.State.Client;

namespace SecsGem.NetCore.Handler.Server.Handler.Message
{
    [SecsGemMessage(HsmsMessageType.SeparateReq)]
    public class SecsGemClientHandlerSeparateReq : SecsGemClientStreamHandler
    {
        public override async Task Execute()
        {
            await Context.Kernel.State.TriggerAsync(GemClientStateTrigger.Disconnect, true);
        }
    }
}