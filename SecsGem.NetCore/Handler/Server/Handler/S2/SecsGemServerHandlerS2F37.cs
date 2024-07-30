using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(2, 37)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS2F37 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var enabled = Context.Message.Root[0].GetBool();
            var events = Context.Message.Root[1].GetListItem().Select(x => Context.Kernel.Feature.CollectionEvents.FirstOrDefault(y => y.Id == x.GetU4())).ToList();

            var valid = !events.Any(x => x == null);
            if (valid)
            {
                events.ForEach(x => x.Enabled = enabled);
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem((byte)(valid ? 0x1 : 0x0)))
                    .Build()
            );
        }
    }
}