using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;
using System.Globalization;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(2, 17)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS2F17 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var now = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new ADataItem(now))
                    .Build()
            );
        }
    }
}