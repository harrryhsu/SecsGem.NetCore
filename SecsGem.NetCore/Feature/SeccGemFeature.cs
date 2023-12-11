namespace SecsGem.NetCore.Feature
{
    public class SecsGemFeature
    {
        public List<Alarm> Alarms { get; protected set; } = new();

        public List<CollectionEvent> CollectionEvents { get; protected set; } = new();

        public List<CollectionReport> CollectionReports { get; protected set; } = new();

        public List<Command> Commands { get; protected set; } = new();

        public List<DataVariable> DataVariables { get; protected set; } = new();

        public List<EquipmentConstant> EquipmentConstants { get; protected set; } = new();

        public List<StatusVariable> StatusVariables { get; protected set; } = new();

        public List<ProcessProgram> ProcessPrograms { get; protected set; } = new();
    }
}