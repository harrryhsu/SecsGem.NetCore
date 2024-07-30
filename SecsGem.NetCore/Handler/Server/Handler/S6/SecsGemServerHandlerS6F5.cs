using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(6, 5)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS6F5 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            await Context.ReplyAsync(
                 HsmsMessage.Builder
                     .Reply(Context.Message)
                     .Item(new BinDataItem(0x0))
                     .Build()
             );
        }
    }
}