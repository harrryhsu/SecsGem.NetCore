using SecsGem.NetCore.Feature;

namespace SecsGem.NetCore.Event
{
    public class SecsGemGetEquipmentConstantEvent : SecsGemEvent<IEnumerable<EquipmentConstant>>
    {
        public override SecsGemEventType Event => SecsGemEventType.GetEquipmentConstant;
    }
}