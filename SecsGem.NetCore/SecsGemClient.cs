using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Event.Client;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Client;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Function;
using SecsGem.NetCore.Handler.Client;
using SecsGem.NetCore.Hsms;
using SecsGem.NetCore.Mutex;

namespace SecsGem.NetCore
{
    public class SecsGemClient : IAsyncDisposable
    {
        public GemClientDevice Device { get; protected set; } = new();

        public GemClientFunction Function { get; protected set; }

        public event SecsGemClientEventHandler OnEvent;

        private readonly CancellationTokenSource _cts = new();

        internal readonly SecsGemOption _option;

        internal readonly SecsGemTcpClient _tcp;

        private readonly SecsGemClientRequestHandler _requestHandler = new();

        private readonly AsyncExecutionLock _lock = new();

        private TcpConnection _host;

        public SecsGemClient(SecsGemOption option)
        {
            if (option.Target == null) throw new ArgumentException("Target");

            _option = option;
            _tcp = new(_option);
            _tcp.OnMessageReceived += OnTcpMessageReceived;
            _tcp.OnError += OnTcpError;
            Function = new(this);
        }

        private async Task OnTcpMessageReceived(SecsGemTcpClient sender, TcpConnection con, HsmsMessage message)
        {
            await _lock.ExecuteAsync(async () =>
            {
                try
                {
                    await _requestHandler.Handle(sender, con, this, message);
                }
                catch (Exception ex)
                {
                    await Emit(new SecsGemErrorEvent
                    {
                        Message = "Error during request handling",
                        Exception = new SecsGemException("Error during request handling", ex),
                    });
                }
            });
        }

        private async Task OnTcpError(SecsGemTcpClient sender, SecsGemException ex)
        {
            await Emit(new SecsGemErrorEvent
            {
                Message = "Tcp Error",
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
            await SetCommunicationState(CommunicationStateModel.CommunicationDisconnected, true);
            Device.ControlState = ControlStateModel.ControlHostOffLine;
            Device.IsSelected = false;
            if (_host != null)
            {
                _host.Close();
                _host = null;
            }
        }

        internal async Task<bool> SetCommunicationState(CommunicationStateModel state, bool force = false)
        {
            if (state == Device.CommunicationState) return true;
            var evt = await Emit(new SecsGemCommunicationStateChangeEvent
            {
                OldState = Device.CommunicationState,
                NewState = state,
            });
            if (evt.Accept || force)
                Device.CommunicationState = state;

            return evt.Accept;
        }

        private async Task SecsGemClientWorker()
        {
            await SetCommunicationState(CommunicationStateModel.CommunicationOffline, true);

            try
            {
                while (!_cts.IsCancellationRequested && _tcp.Online)
                {
                    if (!Device.IsSelected)
                    {
                        var success = await Function.Select(_cts.Token);
                        if (success) continue;
                        await Task.Delay(3000, _cts.Token);
                    }
                    else if (Device.CommunicationState == CommunicationStateModel.CommunicationOffline)
                    {
                        if (_option.ActiveConnect)
                        {
                            var success = await Function.CommunicationEstablish(_cts.Token);
                            if (success) continue;
                        }

                        await Task.Delay(3000, _cts.Token);
                    }
                    else
                    {
                        await _tcp.SendAndWaitForReplyAsync(
                               HsmsMessage.Builder
                                   .Type(HsmsMessageType.LinkTestReq)
                                   .Build(),
                               _cts.Token
                           );

                        await Task.Delay(_option.TLinkTest, _cts.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
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

                await Disconnect();
            }
            catch (Exception ex)
            {
                _option.DebugLog($"SecsGemWorker Error: {ex}");
                await Emit(new SecsGemErrorEvent
                {
                    Exception = new SecsGemException("SecsGemClientWorker Error", ex),
                });

                await Disconnect();
            }
        }

        public async Task ConnectAsync(CancellationToken ct = default)
        {
            await _tcp.ConnectAsync(ct);
            _ = Task.Run(() => SecsGemClientWorker(), ct);
        }

        public async ValueTask DisposeAsync()
        {
            await Function.Separate();
            await Disconnect();
            _tcp.Dispose();
        }
    }
}