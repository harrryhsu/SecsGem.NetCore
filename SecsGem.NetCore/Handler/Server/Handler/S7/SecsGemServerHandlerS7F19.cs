using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(7, 19)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS7F19 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            await Context.ReplyAsync(
                 HsmsMessage.Builder
                     .Reply(Context.Message)
                     .Item(new ListDataItem(
                         Context.Kernel.Feature.ProcessPrograms.Select(x => new ADataItem(x.Id)).ToArray()
                     ))
                     .Build()
             );
        }
    }
}