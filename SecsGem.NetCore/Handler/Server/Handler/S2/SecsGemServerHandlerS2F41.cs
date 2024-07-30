using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(2, 41)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS2F41 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var cmd = Context.Message.Root[0].GetString();
            var param = Context.Message.Root[1].GetListItem().ToDictionary(x => x[0].GetString(), x => x[1].GetString());
            var existing = Context.Kernel.Feature.Commands.FirstOrDefault(x => x.Name == cmd);
            SECS_RESPONSE.HCACK ret;

            if (existing == null)
            {
                ret = SECS_RESPONSE.HCACK.InvalidCommand;
            }
            else
            {
                var evt = await Context.Kernel.Emit(new SecsGemCommandExecuteEvent
                {
                    Cmd = existing,
                    Params = param,
                });
                ret = evt.Return;
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new ListDataItem(
                        new BinDataItem((byte)ret),
                        new ListDataItem()
                    ))
                    .Build()
            );
        }
    }
}