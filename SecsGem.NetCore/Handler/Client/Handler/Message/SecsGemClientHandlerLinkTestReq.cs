using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server.Handler.Message
{
    [SecsGemMessage(HsmsMessageType.LinkTestReq)]
    public class SecsGemClientHandlerLinkTestReq : SecsGemClientStreamHandler
    {
        public override async Task Execute()
        {
            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Type(HsmsMessageType.LinkTestRsp)
                    .Build()
            );
        }
    }
}