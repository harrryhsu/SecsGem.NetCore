namespace SecsGem.NetCore.Feature.Server
{
    public enum ControlStateModel
    {
        ControlEquipmentOffLine = 0,

        ControlHostOffLine,

        ControlOnline,
    }

    public enum ControlOnlineStateModel
    {
        ControlOnlineLocal,

        ControlOnlineRemote,
    }

    public enum GemControlStateChangeResult
    {
        Okay = 0,

        Deny,

        AlreadyInState,
    }

    public class GemServerControlStateMachine
    {
        public volatile ControlStateModel State = ControlStateModel.ControlEquipmentOffLine;

        public volatile ControlOnlineStateModel OnlineState = ControlOnlineStateModel.ControlOnlineLocal;

        public bool IsControlOnline => State == ControlStateModel.ControlOnline;

        public bool IsOnlineRemote => IsControlOnline && OnlineState == ControlOnlineStateModel.ControlOnlineRemote;

        public bool IsOnlineLocal => IsControlOnline && OnlineState == ControlOnlineStateModel.ControlOnlineLocal;

        public bool CanGoOnline => State != ControlStateModel.ControlEquipmentOffLine;

        public GemControlStateChangeResult GoOnline()
        {
            if (IsControlOnline) return GemControlStateChangeResult.AlreadyInState;
            else if (State == ControlStateModel.ControlEquipmentOffLine) return GemControlStateChangeResult.Deny;
            else
            {
                State = ControlStateModel.ControlOnline;
                return GemControlStateChangeResult.Okay;
            }
        }

        public void GoOffline()
        {
            State = ControlStateModel.ControlHostOffLine;
        }

        public void ChangeControlState(ControlStateModel state)
        {
            State = state;
        }

        public void ChangeOnlineState(ControlOnlineStateModel state)
        {
            OnlineState = state;
        }
    }
}