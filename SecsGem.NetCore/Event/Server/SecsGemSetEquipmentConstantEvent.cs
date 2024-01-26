using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemSetEquipmentConstantEvent : SecsGemEvent<IEnumerable<EquipmentConstant>>
    {
        public override SecsGemEventType Event => SecsGemEventType.SetEquipmentConstant;
    }
}