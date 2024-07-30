using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(2, 57)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS2F57 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            await Context.ReplyAsync(
                  HsmsMessage.Builder
                      .Reply(Context.Message)
                      .Item(new ListDataItem(
                          Context.Kernel.Feature.CollectionEvents.Where(x => x.Enabled).Select(x => new U4DataItem(x.Id)).ToArray()
                      ))
                      .Build()
              );
        }
    }
}