using SecsGem.NetCore.Error;

namespace SecsGem.NetCore.Event.Common
{
    public class SecsGemErrorEvent : SecsGemEvent
    {
        public string Message { get; set; }

        public SecsGemException Exception { get; set; }

        public override SecsGemEventType Event => SecsGemEventType.Error;

        public override string ToString()
        {
            return $"{Message} {Exception?.ToString()}";
        }
    }
}