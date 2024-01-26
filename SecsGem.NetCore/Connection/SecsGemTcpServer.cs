using SecsGem.NetCore;
using SecsGem.NetCore.Connection;
using System.Net.Sockets;

namespace TrafficCom.V3.Connection
{
    public delegate Task OnConnectionEventHandler(SecsGemTcpClient sender, TcpConnection con);

    public class SecsGemTcpServer : SecsGemTcpClient, IHostedService
    {
        private readonly TcpListener _server;

        public event OnConnectionEventHandler OnConnection;

        public bool IsConnected => _clients.Any();

        public SecsGemTcpServer(SecsGemOption option) : base(option)
        {
            _server = new(_option.Target);
        }

        public void Start()
        {
            Online = true;
            _server.Start();
            _option.DebugLog($"SecsGem Listening At {_option.Target}");
            Task.Run(() => TcpServerWorker(), _cts.Token);
        }

        private async Task TcpServerWorker()
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
                    _ = Task.Run(() => TcpClientWorker(con), _cts.Token);
                    _option.DebugLog($"Client Connected");
                }
                catch
                {
                }
            }
        }

        protected override void OnClientDisconnected(TcpConnection con)
        {
            con.Close();
            _clients.Remove(con);
        }

        public override void Dispose()
        {
            _server.Stop();
            base.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }
    }
}