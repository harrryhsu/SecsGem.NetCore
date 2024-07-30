using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(7, 5)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS7F5 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var id = Context.Message.Root.GetString();
            var pp = Context.Kernel.Feature.ProcessPrograms.FirstOrDefault(x => x.Id == id) ?? new ProcessProgram { Id = id };

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new ListDataItem(
                        new ADataItem(id),
                        new BinDataItem(pp.Body)
                    ))
                    .Build()
            );
        }
    }
}