using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemCollectionEventEvent : SecsGemEvent
    {
        public CollectionEvent CollectionEvent { get; set; }

        public override SecsGemEventType Event => SecsGemEventType.CollectionEvent;
    }
}