using SecsGem.NetCore.Buffer;
using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Common
{
    public abstract class SecsGemStreamHandler<TKernal>
    {
        public SecsGemRequestContext<TKernal> Context { get; set; }

        public abstract Task Execute();
    }

    public class SecsGemRequestContext<TKernal>
    {
        public TKernal Kernel { get; set; }

        public List<SecsGemHandlerCache> Handlers { get; set; }

        public SecsGemTcpClient Client { get; set; }

        public TcpConnection Connection { get; set; }

        public HsmsMessage Message { get; set; }

        public bool HasReplied { get; private set; }

        public async Task ReplyAsync(HsmsMessage msg)
        {
            if (HasReplied) throw new NotImplementedException($"Replied has already been set: {Message}");
            await Client.SendAsync(Connection, msg);
            HasReplied = true;
        }

        public async Task Error(HsmsErrorCode code)
        {
            var writer = new ByteBufferWriter();
            Message.Header.Write(writer);
            var mhead = writer.ToMemory().ToArray().Skip(4).Take(10).ToArray();

            await ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Message)
                    .Stream(9)
                    .Func((byte)code)
                    .Item(new BinDataItem(mhead))
                    .Build()
            );
        }
    }
}