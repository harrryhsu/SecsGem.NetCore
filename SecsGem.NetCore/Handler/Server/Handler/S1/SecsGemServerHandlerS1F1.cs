using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(1, 1)]
    [SecsGemFunctionType(SecsGemFunctionType.Communication)]
    public class SecsGemServerHandlerS1F1 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            if (Context.Kernel.State.IsMoreThan(GemServerStateModel.ControlOnlineLocal))
            {
                await Context.ReplyAsync(
                    HsmsMessage.Builder
                        .Reply(Context.Message)
                        .Item(new ListDataItem(
                            new ADataItem(Context.Kernel.Feature.Device.Model),
                            new ADataItem(Context.Kernel.Feature.Device.Revision)
                        ))
                        .Build()
                );
            }
            else
            {
                await Context.ReplyAsync(
                    HsmsMessage.Builder
                        .Reply(Context.Message)
                        .Stream(1)
                        .Func(0)
                        .Build()
                );
            }
        }
    }
}