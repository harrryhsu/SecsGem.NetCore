using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;
using SecsGem.NetCore.State.Server;

namespace SecsGem.NetCore.Handler.Server.Handler.Message
{
    [SecsGemMessage(HsmsMessageType.SelectReq)]
    public class SecsGemServerHandlerSelectReq : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var res = await Context.Kernel.State.TriggerAsync(GemServerStateTrigger.Select, false);

            if (res)
            {
                await Context.ReplyAsync(
                    HsmsMessage.Builder
                        .Reply(Context.Message)
                        .Func(0)
                        .Type(HsmsMessageType.SelectRsp)
                        .Build()
                );
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