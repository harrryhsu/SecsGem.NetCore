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
    }
}