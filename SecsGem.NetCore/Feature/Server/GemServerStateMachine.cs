using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Helper;
using Stateless;

namespace SecsGem.NetCore.Feature.Server
{
    public enum GemServerStateModel
    {
        Disconnected = 0,

        Connected,

        Selected,

        ControlOffLine,

        ControlOnlineLocal,

        ControlOnlineRemote,
    }

    public enum GemServerStateTrigger
    {
        Connect,

        Disconnect,

        Select,

        Deselect,

        EstablishCommunication,

        GoOnline,

        GoOffline,

        GoOnlineLocal,

        GoOnlineRemote,
    }

    public class GemServerStateMachine
    {
        private readonly StateMachine<GemServerStateModel, GemServerStateTrigger> _state;

        private readonly Dictionary<KeyValuePair<GemServerStateModel, GemServerStateTrigger>, GemServerStateModel> _transition = new();

        private readonly SecsGemServer _kernel;

        public GemServerStateModel Current => _state.State;

        public GemServerStateMachine(SecsGemServer kernel)
        {
            _kernel = kernel;
            _state = new(GemServerStateModel.Disconnected);

            _state.Configure(GemServerStateModel.Disconnected)
                .Permit(GemServerStateTrigger.Connect, GemServerStateModel.Connected)
                .OnEntryAsync(async () =>
                {
                    await _kernel.Disconnect();
                });

            _state.Configure(GemServerStateModel.Connected)
                .Permit(GemServerStateTrigger.Select, GemServerStateModel.Selected)
                .Permit(GemServerStateTrigger.Disconnect, GemServerStateModel.Disconnected);

            _state.Configure(GemServerStateModel.Selected)
                .Permit(GemServerStateTrigger.EstablishCommunication, GemServerStateModel.ControlOffLine)
                .Permit(GemServerStateTrigger.Deselect, GemServerStateModel.Connected)
                .Permit(GemServerStateTrigger.Disconnect, GemServerStateModel.Disconnected);

            _state.Configure(GemServerStateModel.ControlOffLine)
                .Permit(GemServerStateTrigger.GoOnline, GemServerStateModel.ControlOnlineRemote)
                .Permit(GemServerStateTrigger.Deselect, GemServerStateModel.Connected)
                .Permit(GemServerStateTrigger.Disconnect, GemServerStateModel.Disconnected);

            _state.Configure(GemServerStateModel.ControlOnlineRemote)
                .Permit(GemServerStateTrigger.GoOffline, GemServerStateModel.ControlOffLine)
                .Permit(GemServerStateTrigger.GoOnlineLocal, GemServerStateModel.ControlOnlineLocal)
                .Permit(GemServerStateTrigger.Deselect, GemServerStateModel.Connected)
                .Permit(GemServerStateTrigger.Disconnect, GemServerStateModel.Disconnected);

            _state.Configure(GemServerStateModel.ControlOnlineLocal)
                .Permit(GemServerStateTrigger.GoOffline, GemServerStateModel.ControlOffLine)
                .Permit(GemServerStateTrigger.GoOnlineRemote, GemServerStateModel.ControlOnlineRemote)
                .Permit(GemServerStateTrigger.Deselect, GemServerStateModel.Connected)
                .Permit(GemServerStateTrigger.Disconnect, GemServerStateModel.Disconnected);

            foreach (var state in _state.GetInfo().States)
            {
                foreach (var transition in state.FixedTransitions)
                {
                    var key = KeyValuePair.Create((GemServerStateModel)state.UnderlyingState, (GemServerStateTrigger)transition.Trigger.UnderlyingTrigger);
                    _transition[key] = (GemServerStateModel)transition.DestinationState.UnderlyingState;
                }
            }
        }

        public bool IsExact(GemServerStateModel state)
        {
            return Current == state;
        }

        public bool IsMoreThan(GemServerStateModel state)
        {
            return Current >= state;
        }

        public bool IsOperable => IsExact(GemServerStateModel.ControlOnlineRemote);

        public bool IsReadable => IsMoreThan(GemServerStateModel.ControlOnlineLocal);

        public async Task WaitForState(GemServerStateModel state, int timeoutMs = 1000, bool isIncremental = false, CancellationToken ct = default)
        {
            await TaskHelper.WaitFor(() => isIncremental ? IsMoreThan(state) : IsExact(state), timeoutMs / 100, 100, ct);
        }

        public async Task<bool> TriggerAsync(GemServerStateTrigger trigger, bool force)
        {
            try
            {
                var key = KeyValuePair.Create(Current, trigger);
                if (!_transition.TryGetValue(key, out var destination))
                {
                    await _kernel.Emit(new SecsGemErrorEvent
                    {
                        Message = $"Invalid state transition {Current}: {trigger}"
                    });
                    return false;
                }

                var res = await _kernel.Emit(new SecsGemServerStateChangeEvent
                {
                    OldState = Current,
                    Trigger = trigger,
                    NewState = destination,
                    Force = force
                });

                if (!res.Accept && !force) return false;

                await _state.FireAsync(trigger);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}