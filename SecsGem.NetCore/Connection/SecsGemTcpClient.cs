﻿using SecsGem.NetCore.Buffer;
using SecsGem.NetCore.Error;
using SecsGem.NetCore.Hsms;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace SecsGem.NetCore.Connection
{
    public delegate Task OnMessageReceivedEventHandler(SecsGemTcpClient sender, TcpConnection con, HsmsMessage message);

    public delegate Task OnSecsErrorEventHandler(SecsGemTcpClient sender, SecsGemException ex);

    public class SecsGemTcpClient : IDisposable
    {
        protected List<TcpConnection> _clients = new();

        protected readonly ConcurrentDictionary<uint, ReplyTask> _query = new();

        public bool Online { get; protected set; }

        public event OnMessageReceivedEventHandler OnMessageReceived;

        public event OnSecsErrorEventHandler OnSecsError;

        protected readonly SecsGemOption _option;

        protected CancellationTokenSource _cts = new();

        public SecsGemTcpClient(SecsGemOption option)
        {
            _option = option;
        }

        public async Task<TcpConnection> ConnectAsync()
        {
            return await ConnectAsync(CancellationToken.None);
        }

        protected async Task EmitException(SecsGemException exception)
        {
            await OnSecsError?.Invoke(this, exception);
            throw exception;
        }

        public async Task<TcpConnection> ConnectAsync(CancellationToken cts = default)
        {
            var tcp = new TcpClient();
            _cts = new();

            try
            {
                await tcp.ConnectAsync(_option.Target, cts);
                var con = new TcpConnection
                {
                    TcpClient = tcp,
                    NetworkStream = tcp.GetStream(),
                    SendBuffer = new byte[_option.TcpBufferSize]
                };
                _clients.Add(con);
                Online = true;
                _ = Task.Run(() => TcpClientWorker(con), cts);
                return con;
            }
            catch (Exception ex)
            {
                throw new SecsGemConnectionException("Connection Failure", ex);
            }
        }

        protected async Task TcpRead(NetworkStream ns, byte[] buffer, int offset, int count)
        {
            var value = await ns.ReadAsync(buffer.AsMemory(offset, count), _cts.Token);
            if (value != count) throw new SecsGemConnectionException("T8 Timeout") { Code = "timeout_t8" };
        }

        protected async Task TcpClientWorker(TcpConnection con)
        {
            var buffer = new byte[_option.TcpBufferSize];
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    con.NetworkStream.ReadTimeout = Timeout.Infinite;
                    await TcpRead(con.NetworkStream, buffer, 0, 4);
                    con.NetworkStream.ReadTimeout = _option.T8;

                    var size = ByteBufferReader.ReadU4(buffer);
                    await TcpRead(con.NetworkStream, buffer, 4, (int)size);

                    var reader = new ByteBufferReader(buffer);
                    var msg = new HsmsMessage(reader);

                    if (_query.ContainsKey(msg.Header.Context))
                    {
                        _option.DebugLog($"RECV {msg}, Setting Result");
                        _query[msg.Header.Context].Task.SetResult(msg);
                        _query.TryRemove(msg.Header.Context, out _);
                    }
                    else
                    {
                        _option.DebugLog($"RECV {msg}, Invoking Event");
                        _ = OnMessageReceived?.Invoke(this, con, msg);
                    }
                }
                catch (TimeoutException)
                {
                    Array.Clear(buffer);
                    await con.NetworkStream.FlushAsync();
                }
                catch
                {
                    break;
                }
            }

            OnClientDisconnected(con);
        }

        protected virtual void OnClientDisconnected(TcpConnection con)
        {
            con.Close();
            _clients.Remove(con);
            Online = false;
        }

        public virtual async Task<HsmsMessage> SendAndWaitForReplyAsync(HsmsMessage msg, CancellationToken token = default)
        {
            var client = _clients.FirstOrDefault();
            if (client == null)
            {
                await EmitException(new SecsGemConnectionException("Client not connected") { Code = "not_connected" });
            }
            return await SendAndWaitForReplyAsync(client, msg, token);
        }

        public virtual async Task<HsmsMessage> SendAndWaitForReplyAsync(TcpConnection con, HsmsMessage msg, CancellationToken token = default)
        {
            var task = new ReplyTask();
            _query[msg.Header.Context] = task;
            await SendAsync(con, msg, token);

            if (!msg.ReplyTimeout.HasValue) msg.ReplyTimeout = _option.T3;

            try
            {
                var reply = await task.Task.Task.WaitAsync(TimeSpan.FromMilliseconds(msg.ReplyTimeout.Value), token);

                if (msg.Header.SType == HsmsMessageType.DataMessage && msg.Header.F == 0)
                {
                    _option.DebugLog($"Message is aborted {reply}");
                    await EmitException(new SecsGemTransactionException(reply, $"Message is aborted {reply}") { Code = "abort" });
                    return null;
                }
                else if (msg.Header.SType == HsmsMessageType.DataMessage && (
                    reply.Header.SType != HsmsMessageType.DataMessage ||
                    msg.Header.S != reply.Header.S ||
                    msg.Header.F + 1 != reply.Header.F
                ))
                {
                    _option.DebugLog($"WAIT {msg}, Unexpected reply {reply}");
                    await EmitException(new SecsGemTransactionException(reply, $"Unexpected Reply: {reply}") { Code = "unexpected_reply" });
                    return null;
                }
                else
                {
                    return reply;
                }
            }
            catch (TimeoutException)
            {
                _option.DebugLog($"WAIT {msg}, Timeout");
                await EmitException(new SecsGemConnectionException("Wait For Reply Timeout") { Code = "reply_timeout" });
                return null;
            }
            catch (ObjectDisposedException)
            {
                _option.DebugLog($"WAIT {msg}, Connection disposed");
                await EmitException(new SecsGemConnectionException("Connection Dispoed") { Code = "disposed" });
                return null;
            }
            finally
            {
                _query.TryRemove(msg.Header.Context, out _);
            }
        }

        public virtual async Task SendAsync(HsmsMessage msg, CancellationToken token = default)
        {
            var client = _clients.FirstOrDefault();
            if (client == null)
            {
                await EmitException(new SecsGemConnectionException("Client not connected") { Code = "not_connected" });
            }
            await SendAsync(client, msg, token);
        }

        public virtual async Task SendAsync(TcpConnection con, HsmsMessage msg, CancellationToken token = default)
        {
            if (!Online) await EmitException(new SecsGemConnectionException("Server/Client is not online") { Code = "not_connected" });
            await con.Lock.WaitAsync(token);

            _option.DebugLog($"SEND {msg}");

            try
            {
                var writer = new ByteBufferWriter();
                msg.Write(writer);
                var memory = writer.ToMemory();
                await con.NetworkStream.WriteAsync(memory, token);
            }
            catch (ObjectDisposedException)
            {
                _option.DebugLog($"SEND {msg}, Connection disposed");
                await EmitException(new SecsGemConnectionException("Connection Disposed") { Code = "disposed" });
            }
            catch (Exception ex)
            {
                _option.DebugLog($"SEND {msg}, Error {ex}");
                await EmitException(new SecsGemConnectionException("Message Send Error", ex));
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
            _clients.Clear();
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