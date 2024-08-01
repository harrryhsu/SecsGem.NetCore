using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(1, 13)]
    [SecsGemFunctionType(SecsGemFunctionType.Communication)]
    public class SecsGemServerHandlerS1F13 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var success = await Context.Kernel.State.TriggerAsync(GemServerStateTrigger.EstablishCommunication, false);
            var res = (byte)(success ? 0 : 1);

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new ListDataItem(
                        new BinDataItem(res),
                        new ListDataItem(
                            new ADataItem(Context.Kernel.Feature.Device.Model),
                            new ADataItem(Context.Kernel.Feature.Device.Revision)
                        )
                    ))
                    .Build()
            );
        }
    }
}