using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(7, 3)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS7F3 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var id = Context.Message.Root[0].GetString();
            var body = Context.Message.Root[1].GetBins();
            var pp = Context.Kernel.Feature.ProcessPrograms.FirstOrDefault(x => x.Id == id);

            if (pp != null)
            {
                pp.Body = body;
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem((byte)(pp != null ? SECS_RESPONSE.ACKC7.Accept : SECS_RESPONSE.ACKC7.PPIDNotFound)))
                    .Build()
            );
        }
    }
}