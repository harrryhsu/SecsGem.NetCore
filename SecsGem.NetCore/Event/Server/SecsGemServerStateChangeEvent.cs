﻿using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.State.Server;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemServerStateChangeEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.StateChange;

        public GemServerStateModel OldState { get; set; }

        public GemServerStateTrigger Trigger { get; set; }

        public GemServerStateModel NewState { get; set; }

        public bool Force { get; set; }

        public bool Accept { get; set; } = true;
    }
}