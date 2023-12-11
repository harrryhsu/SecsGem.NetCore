using SecsGem.NetCore.Feature;

namespace SecsGem.NetCore.Event
{
    public enum CommandExecuteResult
    {
        Okay = 0,

        InvalidCommand,

        CannotDoNow,

        ParameterError,

        AsyncCompletion,

        Rejected,

        InvalidObject
    }

    public class SecsGemCommandExecuteEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.CommandExecute;

        public Command Cmd;

        public Dictionary<string, string> Params = new();

        public CommandExecuteResult Return { get; set; }
    }
}