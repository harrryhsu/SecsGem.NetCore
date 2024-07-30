using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(7, 1)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS7F1 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var id = Context.Message.Root[0].GetString();
            var pp = Context.Kernel.Feature.ProcessPrograms.FirstOrDefault(x => x.Id == id);
            if (pp == null)
            {
                Context.Kernel.Feature.ProcessPrograms.Add(new ProcessProgram { Id = id });
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem((byte)(pp != null ? SECS_RESPONSE.PPGNT.AlreadyHave : SECS_RESPONSE.PPGNT.Ok)))
                    .Build()
            );
        }
    }
}