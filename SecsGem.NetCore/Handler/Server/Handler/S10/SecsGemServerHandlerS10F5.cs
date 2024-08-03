using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(10, 5)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS10F5 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var id = Context.Message.Root[0].GetBin();
            var texts = Context.Message.Root[1].GetListItem().Select(x => x.GetString()).ToList();

            var terminal = Context.Kernel.Feature.Terminals.FirstOrDefault(x => x.Id == id);
            if (terminal == null)
            {
                await Context.ReplyAsync(
                    HsmsMessage.Builder
                        .Reply(Context.Message)
                        .Item(new BinDataItem((byte)SECS_RESPONSE.ACKC10.NotAvailable))
                        .Build()
                );
                return;
            }

            var arg = await Context.Kernel.Emit(new SecsGemTerminalDisplayEvent
            {
                Id = id,
                Texts = texts
            });

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem((byte)(arg.Return)))
                    .Build()
            );
        }
    }
}