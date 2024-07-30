using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(7, 17)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS7F17 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            if (Context.Message.Root.Count == 0)
            {
                Context.Kernel.Feature.ProcessPrograms.Clear();
            }
            else
            {
                var pps = Context.Message.Root.GetListItem().Select(x => Context.Kernel.Feature.ProcessPrograms.FirstOrDefault(y => y.Id == x.GetString()));

                if (pps.Any(x => x == null))
                {
                    await Context.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(Context.Message)
                            .Item(new BinDataItem((byte)SECS_RESPONSE.ACKC7.PPIDNotFound))
                            .Build()
                    );
                    return;
                }

                foreach (var pp in pps)
                {
                    Context.Kernel.Feature.ProcessPrograms.Remove(pp);
                }
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem((byte)SECS_RESPONSE.ACKC7.Accept))
                    .Build()
            );
        }
    }
}