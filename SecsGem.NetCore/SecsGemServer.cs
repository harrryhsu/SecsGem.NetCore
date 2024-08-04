using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Error;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Function;
using SecsGem.NetCore.Handler.Server;
using SecsGem.NetCore.Hsms;
using SecsGem.NetCore.Mutex;
using SecsGem.NetCore.State.Server;
using TrafficCom.V3.Connection;

namespace SecsGem.NetCore
{
    public class SecsGemServer : IHostedService, IAsyncDisposable
    {
        public SecsGemServerFeature Feature { get; }

        public GemServerFunction Function { get; }

        public GemServerStateMachine State { get; }

        public SecsGemServerRequestHandler Handler { get; } = new();

        public event SecsGemEventHandler OnEvent;

        private readonly CancellationTokenSource _cts = new();

        internal readonly SecsGemOption _option;

        internal readonly SecsGemTcpServer _tcp;

        private readonly AsyncExecutionLock _lock = new();

        private TcpConnection _host;

        public SecsGemServer(SecsGemOption option)
        {
            if (option.Target == null) throw new ArgumentException("Target");

            _option = option;
            _tcp = new(_option);
            _tcp.OnMessageReceived += OnTcpMessageReceived;
            _tcp.OnConnection += OnTcpConnection;
            _tcp.OnSecsError += OnSecsError;
            Function = new(this);
            Feature = new();
            State = new(this);
        }

        private async Task OnTcpMessageReceived(SecsGemTcpClient sender, TcpConnection con, HsmsMessage message)
        {
            await _lock.ExecuteAsync(async () =>
            {
                try
                {
                    await Handler.Handle(sender, con, this, message);
                }
                catch (Exception ex)
                {
                    await Emit(new SecsGemErrorEvent
                    {
                        Message = "Error during Contextuest handling",
                        Exception = new SecsGemException("Error during Contextuest handling", ex),
                    });
                }
            });
        }

        private async Task OnTcpConnection(SecsGemTcpClient sender, TcpConnection con)
        {
            await _lock.ExecuteAsync(async () =>
            {
                if (_host != null)
                {
                    con.Close();
                }
                else
                {
                    _host = con;
                    await State.TriggerAsync(GemServerStateTrigger.Connect, true);
                    _ = Task.Run(() => SecsGemServerWorker(con));
                }
            });
        }

        private async Task OnSecsError(SecsGemTcpClient sender, SecsGemException ex)
        {
            await Emit(new SecsGemErrorEvent
            {
                Message = "Secs Error",
                Exception = ex,
            });
        }

        internal async Task<TEvent> Emit<TEvent>(TEvent e) where TEvent : SecsGemEvent
        {
            try
            {
                if (e is SecsGemErrorEvent ex)
                {
                    _option.DebugLog(ex.ToString());
                }

                if (OnEvent != null)
                {
                    _option.DebugLog($"Event emit: {e.Event}");
                    await OnEvent.Invoke(this, e);
                }
            }
            catch (Exception ex)
            {
                if (e.Event == SecsGemEventType.Error)
                {
                    throw;
                }
                else
                {
                    await Emit(new SecsGemErrorEvent
                    {
                        Message = "Event Handler Error",
                        Exception = new SecsGemException("Event Handler Error", ex),
                    });
                }
            }

            return e;
        }

        internal async Task Disconnect()
        {
            if (_host != null)
            {
                _host.Close();
                _host = null;
            }
        }

        private async Task SecsGemServerWorker(TcpConnection con)
        {
            try
            {
                while (!_cts.IsCancellationRequested && con.TcpClient.Connected)
                {
                    switch (State.Current)
                    {
                        case GemServerStateModel.Connected:
                            try
                            {
                                if (_option.T7 < 100) _option.T7 = 100;
                                await State.WaitForState(GemServerStateModel.Selected, ct: _cts.Token);
                                _option.DebugLog("Selected");
                            }
                            catch (TimeoutException)
                            {
                                if (!con.TcpClient.Connected) return;
                                await Emit(new SecsGemErrorEvent
                                {
                                    Message = "T7 Timeout"
                                });
                                await State.TriggerAsync(GemServerStateTrigger.Disconnect, true);
                                return;
                            }
                            break;

                        case GemServerStateModel.Selected:
                            if (_option.ActiveConnect)
                            {
                                var success = await Function.S1F13EstablishCommunicationRequest(_cts.Token);
                                if (success)
                                {
                                    _option.DebugLog("Communication Established");
                                    continue;
                                }
                            }
                            await Task.Delay(3000, _cts.Token);
                            break;

                        case GemServerStateModel.ControlOnlineLocal:
                        case GemServerStateModel.ControlOnlineRemote:
                            var reply = await _tcp.SendAndWaitForReplyAsync(
                                HsmsMessage.Builder
                                    .Type(HsmsMessageType.LinkTestReq)
                                    .Build(),
                                _cts.Token
                            );
                            await Task.Delay(_option.TLinkTest, _cts.Token);
                            break;

                        default:
                            await Task.Delay(100, _cts.Token);
                            break;
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (SecsGemConnectionException ex)
            {
                if (ex.Code != "not_connected")
                {
                    _option.DebugLog($"SecsGemWorker Error: {ex}");
                    await Emit(new SecsGemErrorEvent
                    {
                        Exception = ex,
                    });
                }

                await State.TriggerAsync(GemServerStateTrigger.Disconnect, true);
            }
            catch (Exception ex)
            {
                _option.DebugLog($"SecsGemWorker Error: {ex}");
                await Emit(new SecsGemErrorEvent
                {
                    Exception = new SecsGemException("SecsGemWorker Error", ex),
                });

                await State.TriggerAsync(GemServerStateTrigger.Disconnect, true);
            }
        }

        public async Task StartAsync()
        {
            await StartAsync(CancellationToken.None);
        }

        public async Task StartAsync(CancellationToken ct)
        {
            await Emit(new SecsGemInitEvent());
            await _tcp.StartAsync(ct);
        }

        public async Task StopAsync()
        {
            await StopAsync(CancellationToken.None);
        }

        public async Task StopAsync(CancellationToken ct)
        {
            await DisposeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            await Emit(new SecsGemStopEvent());
            await Function.Separate();
            _tcp.Dispose();
        }
    }
}