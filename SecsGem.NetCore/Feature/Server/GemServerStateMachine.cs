using SecsGem.NetCore.Event.Server;
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
                .PermitIf(GemServerStateTrigger.GoOnline, GemServerStateModel.ControlOnlineRemote, () =>
                {
                    var res = _kernel.Emit(new SecsGemServerStateChangeEvent()).Result;
                    return res.Accept;
                })
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

        public async Task<bool> TriggerAsync(GemServerStateTrigger trigger)
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