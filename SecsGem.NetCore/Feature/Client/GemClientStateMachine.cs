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
        }

        public bool IsExact(GemClientStateModel state)
        {
            return Current == state;
        }

        public bool IsMoreThan(GemClientStateModel state)
        {
            return Current >= state;
        }

        public async Task<bool> TriggerAsync(GemClientStateTrigger trigger)
        {
            try
            {
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