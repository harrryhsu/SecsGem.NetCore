using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    public class SecsGemClientStreamHandler : ISecsGemServerStreamHandler
    {
        public async Task S10F3(SecsGemServerRequestContext req)
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
                    .Item(new BinDataItem((byte)(arg.Return)))
                    .Build()
            );
        }

        public async Task S10F5(SecsGemServerRequestContext req)
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
                    .Item(new BinDataItem((byte)(arg.Return)))
                    .Build()
            );
        }

        public async Task S10F9(SecsGemServerRequestContext req)
        {
            var text = req.Message.Root[0].GetString();

            var arg = await req.Kernel.Emit(new SecsGemTerminalDisplayEvent
            {
                Text = new List<string> { text }
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)(arg.Return)))
                    .Build()
            );
        }
    }
}