using SecsGem.NetCore.Buffer;
using SecsGem.NetCore.Hsms;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace SecsGem.NetCore.Connection
{
    public delegate Task OnMessageReceivedEventHandler(SecsGemTcpClient sender, TcpConnection con, HsmsMessage message);

    public delegate Task OnErrorEventHandler(SecsGemTcpClient sender, SecsGemException ex);

    public class SecsGemTcpClient : IDisposable
    {
        protected TcpConnection _client;

        protected readonly CancellationTokenSource _cts = new();

        protected readonly ConcurrentDictionary<uint, ReplyTask> _query = new();

        public bool Online { get; protected set; }

        public event OnMessageReceivedEventHandler OnMessageReceived;

        public event OnErrorEventHandler OnError;

        protected readonly SecsGemOption _option;

        public SecsGemTcpClient(SecsGemOption option)
        {
            _option = option;
        }

        public virtual async Task StartAsync()
        {
            await ConnectAsync(CancellationToken.None);
        }

        public virtual async Task ConnectAsync(CancellationToken cts)
        {
            var tcp = new TcpClient();
            try
            {
                await tcp.ConnectAsync(_option.Target, cts);
                var con = new TcpConnection
                {
                    TcpClient = tcp,
                    NetworkStream = tcp.GetStream(),
                    SendBuffer = new byte[_option.TcpBufferSize]
                };
                _client = con;
                Online = true;
                _ = Task.Run(() => SecsGemClientWorker(con), cts);
            }
            catch (Exception ex)
            {
                throw new SecsGemConnectionException("Connection Failure", ex);
            }
        }

        protected async Task TcpRead(NetworkStream ns, byte[] buffer, int offset, int count, CancellationToken token)
        {
            var read = 0;
            while (read < count && !token.IsCancellationRequested)
            {
                read += await ns.ReadAsync(buffer.AsMemory(offset, count), _cts.Token);
            }
        }

        protected async Task SecsGemClientWorker(TcpConnection con)
        {
            var buffer = new byte[_option.TcpBufferSize];
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await TcpRead(con.NetworkStream, buffer, 0, 4, CancellationToken.None);
                    var size = ByteBufferReader.ReadU4(buffer);

                    var cts = new CancellationTokenSource();
                    var task = TcpRead(con.NetworkStream, buffer, 4, (int)size, cts.Token);
                    await Task.WhenAny(task, Task.Delay(_option.T8));

                    if (!task.IsCompleted)
                    {
                        cts.Cancel();
                        OnError?.Invoke(this, new SecsGemConnectionException("T8 Timeout") { Code = "timeout_t8" });
                        continue;
                    }

                    var reader = new ByteBufferReader(buffer);
                    var msg = new HsmsMessage(reader);

                    if (_query.ContainsKey(msg.Header.Context))
                    {
                        _query[msg.Header.Context].Task.SetResult(msg);
                        _query.Remove(msg.Header.Context, out _);
                    }
                    else
                    {
                        OnMessageReceived?.Invoke(this, con, msg);
                    }
                }
                catch
                {
                    con.Close();
                    _client = null;
                    return;
                }
            }
        }

        public virtual async Task<HsmsMessage> SendAndWaitForReplyAsync(HsmsMessage msg)
        {
            return await SendAndWaitForReplyAsync(msg, CancellationToken.None);
        }

        public virtual async Task<HsmsMessage> SendAndWaitForReplyAsync(HsmsMessage msg, CancellationToken token)
        {
            if (_client == null) throw new SecsGemConnectionException("Client not connected") { Code = "not_connected" };
            return await SendAndWaitForReplyAsync(_client, msg, token);
        }

        public virtual async Task<HsmsMessage> SendAndWaitForReplyAsync(TcpConnection con, HsmsMessage msg)
        {
            return await SendAndWaitForReplyAsync(con, msg, CancellationToken.None);
        }

        public virtual async Task<HsmsMessage> SendAndWaitForReplyAsync(TcpConnection con, HsmsMessage msg, CancellationToken token)
        {
            var task = new ReplyTask();
            _query[msg.Header.Context] = task;
            await SendAsync(con, msg, token);

            if (!msg.ReplyTimeout.HasValue) msg.ReplyTimeout = _option.T3;
            await Task.WhenAny(
                task.Task.Task,
                Task.Delay(msg.ReplyTimeout.Value, token)
            );

            if (!task.Task.Task.IsCompletedSuccessfully)
            {
                _query.Remove(msg.Header.Context, out _);
                throw new SecsGemTransactionException("Wait For Reply Timeout") { Code = "reply_timeout" };
            }
            else
            {
                var reply = task.Task.Task.Result;
                if (msg.Header.SType == HsmsMessageType.DataMessage && (
                    reply.Header.SType != HsmsMessageType.DataMessage ||
                    msg.Header.S != reply.Header.S ||
                    msg.Header.F + 1 != reply.Header.F
                ))
                {
                    throw new SecsGemTransactionException($"Unexpected Reply: T{reply.Header.SType}{reply.ToShortName()}") { Code = "unexpected_reply" };
                }
                else
                {
                    return reply;
                }
            }
        }

        public virtual async Task SendAsync(HsmsMessage msg)
        {
            await SendAsync(msg, CancellationToken.None);
        }

        public virtual async Task SendAsync(HsmsMessage msg, CancellationToken token)
        {
            if (_client == null) throw new SecsGemConnectionException("Client not connected") { Code = "not_connected" };
            await SendAsync(_client, msg, token);
        }

        public virtual async Task SendAsync(TcpConnection con, HsmsMessage msg)
        {
            await SendAsync(con, msg, CancellationToken.None);
        }

        public virtual async Task SendAsync(TcpConnection con, HsmsMessage msg, CancellationToken token)
        {
            if (!Online) throw new SecsGemConnectionException("Server/Client is not online") { Code = "not_connected" };
            await con.Lock.WaitAsync(token);

            try
            {
                var writer = new ByteBufferWriter();
                msg.Write(writer);
                var memory = writer.ToMemory();
                await con.NetworkStream.WriteAsync(memory, token);
            }
            catch (ObjectDisposedException)
            {
                throw new SecsGemConnectionException("Client Disconnected") { Code = "not_connected" };
            }
            catch (Exception ex)
            {
                throw new SecsGemConnectionException("Message Send Error", ex);
            }
            finally
            {
                con.Lock.Release();
            }
        }

        public virtual void Dispose()
        {
            Online = false;
            _cts.Cancel();
            if (_client != null) _client.Close();
        }
    }

    public class TcpConnection
    {
        public TcpClient TcpClient { get; set; }

        public NetworkStream NetworkStream { get; set; }

        public SemaphoreSlim Lock { get; set; } = new(1, 1);

        public byte[] SendBuffer { get; set; }

        public void Close()
        {
            TcpClient.Close();
            NetworkStream.Close();
        }
    }

    public class ReplyTask
    {
        public TaskCompletionSource<HsmsMessage> Task { get; set; } = new();
    }
}