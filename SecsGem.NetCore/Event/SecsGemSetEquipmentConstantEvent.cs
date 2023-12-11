using SecsGem.NetCore.Feature;

namespace SecsGem.NetCore.Event
{
    public class SecsGemSetEquipmentConstantEvent : SecsGemEvent<IEnumerable<EquipmentConstant>>
    {
        public override SecsGemEventType Event => SecsGemEventType.SetEquipmentConstant;
    }
}