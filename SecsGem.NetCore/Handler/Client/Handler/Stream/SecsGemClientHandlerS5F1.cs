using SecsGem.NetCore.Event.Client;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(5, 1)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemClientHandlerS5F1 : SecsGemClientStreamHandler
    {
        public override async Task Execute()
        {
            var id = Context.Message.Root[1].GetU4();
            var description = Context.Message.Root[2].GetString();

            await Context.Kernel.Emit(new SecsGemAlarmEvent
            {
                Alarm = new Alarm
                {
                    Description = description,
                    Id = id,
                    Enabled = true
                }
            });

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem(0))
                    .Build()
            );
        }
    }
}