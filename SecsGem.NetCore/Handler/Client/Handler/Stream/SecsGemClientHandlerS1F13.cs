using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;
using SecsGem.NetCore.State.Client;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(1, 13)]
    [SecsGemFunctionType(SecsGemFunctionType.Communication)]
    public class SecsGemClientHandlerS1F13 : SecsGemClientStreamHandler
    {
        public override async Task Execute()
        {
            var success = await Context.Kernel.State.TriggerAsync(GemClientStateTrigger.EstablishCommunication, false);
            var res = (byte)(success ? 0 : 1);

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new ListDataItem(
                        new BinDataItem(res),
                        new ListDataItem(
                            new ADataItem(Context.Kernel.Device.Model),
                            new ADataItem(Context.Kernel.Device.Revision)
                        )
                    ))
                    .Build()
            );
        }
    }
}