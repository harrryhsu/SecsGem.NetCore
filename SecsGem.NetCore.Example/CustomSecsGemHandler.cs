using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Example
{
    public class CustomSecsGemHandler : ISecsGemServerEventHandler
    {
        private readonly SecsGemServer _kernel;

        private readonly Equipment _equipment;

        public CustomSecsGemHandler(SecsGemServer kernel, Equipment equipment)
        {
            _kernel = kernel;
            _equipment = equipment;
        }

        public async Task Init(SecsGemInitEvent evt)
        {
            _kernel.Feature.Device.Model = "Test Model";
            _kernel.Feature.Device.Revision = "Test Revision";
            _kernel.Feature.StatusVariables.Add(new StatusVariable { Id = 1, Name = "Test SV 1", Unit = "Test Unit 1" });
            _kernel.Feature.EquipmentConstants.Add(new EquipmentConstant { Id = 1, Name = "ARG", Unit = "Test Unit 1", Min = 0, Max = 1000, Default = 0 });
            _kernel.Feature.DataVariables.Add(new DataVariable { Id = "RUNNING", Description = "Equipment running status", Unit = "boolean" });
            _kernel.Feature.Alarms.Add(new Alarm { Id = 1, Description = "Test Alarm", Enabled = false });
            _kernel.Feature.CollectionEvents.Add(new CollectionEvent { Id = 1, Name = "Test CE 1", Enabled = true });
            _kernel.Feature.Terminals.Add(new Terminal { Id = 1, Name = "Main Display", });
            _kernel.Feature.Commands.Add(new Command { Name = "START", Description = "Start Production", });
            _kernel.Feature.Commands.Add(new Command { Name = "STOP", Description = "Stop Production", });
        }

        public async Task Stop(SecsGemStopEvent evt)
        {
        }

        public async Task CommandExecute(SecsGemCommandExecuteEvent evt)
        {
            switch (evt.Cmd.Name)
            {
                case "START":
                    _equipment.Running = true;
                    break;

                case "STOP":
                    _equipment.Running = false;
                    break;

                default:
                    break;
            }
        }

        public async Task Error(SecsGemErrorEvent evt)
        {
            Console.WriteLine($"SecsGem Error: {evt.Message} {evt.Exception}");
        }

        public async Task GetDataVariable(SecsGemGetDataVariableEvent evt)
        {
        }

        public async Task GetEquipmentConstant(SecsGemGetEquipmentConstantEvent evt)
        {
        }

        public async Task GetStatusVariable(SecsGemGetStatusVariableEvent evt)
        {
            foreach (var sv in evt.Params)
            {
                switch (sv.Id)
                {
                    case 1:
                        sv.Value = _equipment.Running ? "true" : "false";
                        break;

                    default:
                        break;
                }
            }
        }

        public async Task SetEquipmentConstant(SecsGemSetEquipmentConstantEvent evt)
        {
            foreach (var ec in evt.Params)
            {
                switch (ec.Name)
                {
                    case "ARG":
                        _equipment.Arg = ec.Value;
                        break;

                    default:
                        break;
                }
            }
        }

        public async Task TerminalDisplay(SecsGemTerminalDisplayEvent evt)
        {
            _equipment.Display = evt.Texts.FirstOrDefault();
            evt.Return = SECS_RESPONSE.ACKC10.Accepted;
        }

        public async Task SetTime(SecsGemSetTimeEvent evt)
        {
        }

        public async Task StateChange(SecsGemServerStateChangeEvent evt)
        {
        }

        public async Task DataChange(SecsGemDataChangeEvent evt)
        {
        }

        public async Task OrphanMessage(SecsGemServerOrphanMessageEvent evt)
        {
            var message = evt.Context.Message;
            if (message.Header.SType == HsmsMessageType.DataMessage && message.Header.S == 11 && message.Header.F == 1)
            {
                await evt.Context.ReplyAsync(
                    HsmsMessage.Builder
                        .Reply(message)
                        .Item(new BinDataItem(1)).Build()
                );
            }
        }

        public async Task Message(SecsGemMessageEvent evt)
        {
            Console.WriteLine($"Received message {evt.Message}");
        }
    }
}