using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server.Handler.Message
{
    [SecsGemMessage(HsmsMessageType.SelectReq)]
    public class SecsGemServerHandlerSelectReq : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            if (Context.Kernel.State.Current == GemServerStateModel.Connected)
            {
                await Context.ReplyAsync(
                    HsmsMessage.Builder
                        .Reply(Context.Message)
                        .Func(0)
                        .Type(HsmsMessageType.SelectRsp)
                        .Build()
                );
                await Context.Kernel.State.TriggerAsync(GemServerStateTrigger.Select);
            }
            else
            {
                await Context.ReplyAsync(
                    HsmsMessage.Builder
                        .Reply(Context.Message)
                        .Func(1)
                        .Type(HsmsMessageType.SelectRsp)
                        .Build()
                );
            }
        }
    }
}