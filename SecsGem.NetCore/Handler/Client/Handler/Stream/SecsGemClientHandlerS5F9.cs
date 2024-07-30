using SecsGem.NetCore.Event.Client;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(5, 9)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemClientHandlerS5F9 : SecsGemClientStreamHandler
    {
        public override async Task Execute()
        {
            await Context.Kernel.Emit(new SecsGemNotifyExceptionEvent
            {
                Timestamp = Context.Message.Root[0].GetString(),
                Id = Context.Message.Root[1].GetString(),
                Type = Context.Message.Root[2].GetString(),
                Message = Context.Message.Root[3].GetString(),
                RecoveryMessage = string.Join("", Context.Message.Root[4].GetListItem().Select(x => x.GetString())),
            });

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Build()
            );
        }
    }
}