using SecsGem.NetCore;
using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Hsms;
using System.Net.Sockets;

namespace TrafficCom.V3.Connection
{
    public delegate Task OnConnectionEventHandler(SecsGemTcpClient sender, TcpConnection con);

    public class SecsGemTcpServer : SecsGemTcpClient
    {
        private readonly List<TcpConnection> _clients = new();

        private readonly TcpListener _server;

        public event OnConnectionEventHandler OnConnection;

        public SecsGemTcpServer(SecsGemOption option) : base(option)
        {
            _server = new(_option.Target);
        }

        public override Task StartAsync()
        {
            Online = true;
            _server.Start();
            Console.WriteLine($"SecsGem Listening At {_option.Target}");
            _ = SecsGemServerWorker();
            return Task.CompletedTask;
        }

        private async Task SecsGemServerWorker()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    TcpClient handler = await _server.AcceptTcpClientAsync(_cts.Token);
                    var con = new TcpConnection
                    {
                        TcpClient = handler,
                        NetworkStream = handler.GetStream(),
                        SendBuffer = new byte[_option.TcpBufferSize]
                    };
                    _clients.Add(con);
                    await OnConnection?.Invoke(this, con);
                    _ = Task.Run(() => SecsGemClientWorker(con), _cts.Token);
                }
                catch
                {
                }
            }
        }

        public override async Task<HsmsMessage> SendAndWaitForReplyAsync(HsmsMessage msg, CancellationToken token)
        {
            var client = _clients.FirstOrDefault();
            if (client == null) throw new SecsGemConnectionException("Client not connected") { Code = "not_connected" };
            return await SendAndWaitForReplyAsync(client, msg, token);
        }

        public override async Task SendAsync(HsmsMessage msg, CancellationToken token)
        {
            var client = _clients.FirstOrDefault();
            if (client == null) throw new SecsGemConnectionException("Client not connected") { Code = "not_connected" };
            await SendAsync(client, msg, token);
        }

        public override void Dispose()
        {
            base.Dispose();
            _server.Stop();
        }
    }
}