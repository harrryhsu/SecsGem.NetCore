using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(5, 3)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS5F3 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var enabled = Context.Message.Root[0].GetBin() == 128;
            var id = Context.Message.Root[1].GetU4();
            var alarm = Context.Kernel.Feature.Alarms.FirstOrDefault(x => x.Id == id);

            if (alarm != null)
            {
                alarm.Enabled = enabled;
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem(0x0))
                    .Build()
            );
        }
    }
}