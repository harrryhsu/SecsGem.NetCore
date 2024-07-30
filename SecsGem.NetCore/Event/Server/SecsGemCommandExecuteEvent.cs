using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemCommandExecuteEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.CommandExecute;

        public Command Cmd;

        public Dictionary<string, string> Params = new();

        public SECS_RESPONSE.HCACK Return { get; set; } = SECS_RESPONSE.HCACK.Ok;
    }
}