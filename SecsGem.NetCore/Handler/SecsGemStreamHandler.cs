using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler
{
    public interface ISecsGemStreamHandler
    {
    }

    public class SecsGemRequestContext
    {
        public SecsGemKernel Kernel { get; set; }

        public SecsGemTcpClient Client { get; set; }

        public TcpConnection Connection { get; set; }

        public HsmsMessage Message { get; set; }

        private bool _hasReplied = false;

        public async Task ReplyAsync(HsmsMessage msg)
        {
            if (_hasReplied) throw new NotImplementedException($"Replied has already been set: {Message.ToShortName()}");
            await Client.SendAsync(Connection, msg);
            _hasReplied = true;
        }
    }
}