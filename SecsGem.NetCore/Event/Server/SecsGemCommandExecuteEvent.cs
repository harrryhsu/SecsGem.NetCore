using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;
using static SecsGem.NetCore.Handler.Server.SecsGemStream2Handler;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemCommandExecuteEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.CommandExecute;

        public Command Cmd;

        public Dictionary<string, string> Params = new();

        public S2F42_HCACK Return { get; set; } = S2F42_HCACK.Ok;
    }
}