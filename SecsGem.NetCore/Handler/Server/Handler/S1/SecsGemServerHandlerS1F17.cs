using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(1, 17)]
    [SecsGemFunctionType(SecsGemFunctionType.Communication)]
    public class SecsGemServerHandlerS1F17 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var success = await Context.Kernel.State.TriggerAsync(GemServerStateTrigger.GoOnline);
            var res = (byte)(success ? 0 : 1);
            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem(res))
                    .Build()
            );
        }
    }
}