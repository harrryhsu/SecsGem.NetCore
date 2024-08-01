using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(1, 15)]
    [SecsGemFunctionType(SecsGemFunctionType.Communication)]
    public class SecsGemServerHandlerS1F15 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            await Context.Kernel.State.TriggerAsync(GemServerStateTrigger.GoOffline, true);
            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem(0x0))
                    .Build()
            );
        }
    }
}