using SecsGem.NetCore.Feature;

namespace SecsGem.NetCore.Event
{
    public class SecsGemGetStatusVariableEvent : SecsGemEvent<IEnumerable<StatusVariable>>
    {
        public override SecsGemEventType Event => SecsGemEventType.GetStatusVariable;
    }
}