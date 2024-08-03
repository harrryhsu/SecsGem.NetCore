namespace SecsGem.NetCore.Feature.Server
{
    public class SecsGemServerFeature
    {
        public List<Alarm> Alarms { get; } = new();

        public List<CollectionEvent> CollectionEvents { get; } = new();

        public List<CollectionReport> CollectionReports { get; } = new();

        public List<Command> Commands { get; } = new();

        public List<DataVariable> DataVariables { get; } = new();

        public List<EquipmentConstant> EquipmentConstants { get; } = new();

        public List<StatusVariable> StatusVariables { get; } = new();

        public List<ProcessProgram> ProcessPrograms { get; } = new();

        public List<Terminal> Terminals { get; } = new();

        public GemServerDevice Device { get; } = new();
    }
}