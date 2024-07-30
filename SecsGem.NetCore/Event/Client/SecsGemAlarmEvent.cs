using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Event.Client
{
    public class SecsGemAlarmEvent : SecsGemEvent
    {
        public Alarm Alarm { get; set; }

        public override SecsGemEventType Event => SecsGemEventType.Alarm;
    }
}