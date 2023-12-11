using SecsGem.NetCore.Feature;

namespace SecsGem.NetCore.Event
{
    public class SecsGemGetDataVariableEvent : SecsGemEvent<IEnumerable<DataVariable>>
    {
        public override SecsGemEventType Event => SecsGemEventType.GetDataVariable;
    }
}