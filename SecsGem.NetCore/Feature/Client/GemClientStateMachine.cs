using SecsGem.NetCore.Event.Client;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Helper;
using Stateless;

namespace SecsGem.NetCore.Feature.Client
{
    public enum GemClientStateModel
    {
        Disconnected = 0,

        Connected,

        Selected,

        ControlOffLine,

        ControlOnline,
    }

    public enum GemClientStateTrigger
    {
        Connect,

        Disconnect,

        Select,

        Deselect,

        EstablishCommunication,

        GoOffline,

        GoOnline,
    }

    public class GemClientStateMachine
    {
        private readonly StateMachine<GemClientStateModel, GemClientStateTrigger> _state;

        private readonly Dictionary<KeyValuePair<GemClientStateModel, GemClientStateTrigger>, GemClientStateModel> _transition = new();

        private readonly SecsGemClient _kernel;

        public GemClientStateModel Current => _state.State;

        public GemClientStateMachine(SecsGemClient kernel)
        {
            _kernel = kernel;
            _state = new(GemClientStateModel.Disconnected);

            _state.Configure(GemClientStateModel.Disconnected)
                .Permit(GemClientStateTrigger.Connect, GemClientStateModel.Connected)
                .OnEntryAsync(async () =>
                {
                    await _kernel.Disconnect();
                });

            _state.Configure(GemClientStateModel.Connected)
                .Permit(GemClientStateTrigger.Select, GemClientStateModel.Selected)
                .Permit(GemClientStateTrigger.Disconnect, GemClientStateModel.Disconnected);

            _state.Configure(GemClientStateModel.Selected)
                .Permit(GemClientStateTrigger.EstablishCommunication, GemClientStateModel.ControlOffLine)
                .Permit(GemClientStateTrigger.Deselect, GemClientStateModel.Connected)
                .Permit(GemClientStateTrigger.Disconnect, GemClientStateModel.Disconnected);

            _state.Configure(GemClientStateModel.ControlOffLine)
                .PermitIf(GemClientStateTrigger.GoOnline, GemClientStateModel.ControlOnline)
                .Permit(GemClientStateTrigger.Deselect, GemClientStateModel.Connected)
                .Permit(GemClientStateTrigger.Disconnect, GemClientStateModel.Disconnected);

            _state.Configure(GemClientStateModel.ControlOnline)
                .Permit(GemClientStateTrigger.GoOffline, GemClientStateModel.ControlOffLine)
                .Permit(GemClientStateTrigger.Deselect, GemClientStateModel.Connected)
                .Permit(GemClientStateTrigger.Disconnect, GemClientStateModel.Disconnected);

            foreach (var state in _state.GetInfo().States)
            {
                foreach (var transition in state.FixedTransitions)
                {
                    var key = KeyValuePair.Create((GemClientStateModel)state.UnderlyingState, (GemClientStateTrigger)transition.Trigger.UnderlyingTrigger);
                    _transition[key] = (GemClientStateModel)transition.DestinationState.UnderlyingState;
                }
            }
        }

        public bool IsExact(GemClientStateModel state)
        {
            return Current == state;
        }

        public bool IsMoreThan(GemClientStateModel state)
        {
            return Current >= state;
        }

        public async Task WaitForState(GemClientStateModel state, int timeoutMs = 1000, bool isIncremental = false, CancellationToken ct = default)
        {
            await TaskHelper.WaitFor(() => isIncremental ? IsMoreThan(state) : IsExact(state), timeoutMs / 100, 100, ct);
        }

        public async Task<bool> TriggerAsync(GemClientStateTrigger trigger, bool force)
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

                var res = await _kernel.Emit(new SecsGemClientStateChangeEvent
                {
                    OldState = Current,
                    Trigger = trigger,
                    NewState = destination,
                    Force = force
                });

                if (!res.Accept) return false;

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