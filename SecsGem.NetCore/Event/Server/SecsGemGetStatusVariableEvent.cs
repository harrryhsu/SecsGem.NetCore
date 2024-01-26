using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemGetStatusVariableEvent : SecsGemEvent<IEnumerable<StatusVariable>>
    {
        public override SecsGemEventType Event => SecsGemEventType.GetStatusVariable;
    }
}