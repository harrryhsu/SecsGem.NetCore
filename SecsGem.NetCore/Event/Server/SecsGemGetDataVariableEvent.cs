using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemGetDataVariableEvent : SecsGemEvent<IEnumerable<DataVariable>>
    {
        public override SecsGemEventType Event => SecsGemEventType.GetDataVariable;
    }
}