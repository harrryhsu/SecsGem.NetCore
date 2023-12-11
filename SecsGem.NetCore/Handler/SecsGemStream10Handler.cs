using SecsGem.NetCore.Event;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler
{
    public class SecsGemStream10Handler : ISecsGemStreamHandler
    {
        public async Task S10F3(SecsGemRequestContext req)
        {
            var id = req.Message.Root[0].GetBin();
            var text = req.Message.Root[1].GetString();

            var arg = await req.Kernel.Emit(new SecsGemTerminalDisplayEvent
            {
                Id = id,
                Text = new List<string> { text }
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)(arg.Return ? 0x0 : 0x2)))
                    .Build()
            );
        }

        public async Task S10F5(SecsGemRequestContext req)
        {
            var id = req.Message.Root[0].GetBin();
            var texts = req.Message.Root[1].GetListItem().Select(x => x.GetString()).ToList();

            var arg = await req.Kernel.Emit(new SecsGemTerminalDisplayEvent
            {
                Id = id,
                Text = texts
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)(arg.Return ? 0x0 : 0x2)))
                    .Build()
            );
        }

        public async Task S10F9(SecsGemRequestContext req)
        {
            var text = req.Message.Root[0].GetString();

            var arg = await req.Kernel.Emit(new SecsGemTerminalDisplayEvent
            {
                Text = new List<string> { text }
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)(arg.Return ? 0x0 : 0x2)))
                    .Build()
            );
        }
    }
}