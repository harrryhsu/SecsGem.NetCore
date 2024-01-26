using SecsGem.NetCore.Event.Common;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemNotifyExceptionEvent : SecsGemEvent
    {
        public string Timestamp { get; set; }

        public string Id { get; set; }

        public string Type { get; set; }

        public string Message { get; set; }

        public string RecoveryMessage { get; set; }

        public override SecsGemEventType Event => SecsGemEventType.NotifyException;
    }
}