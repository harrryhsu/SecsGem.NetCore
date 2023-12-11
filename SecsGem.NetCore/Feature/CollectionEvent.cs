namespace SecsGem.NetCore.Feature
{
    public class CollectionEvent
    {
        public uint Id { get; set; }

        public string Name { get; set; }

        public bool Enabled { get; set; }

        public List<CollectionReport> CollectionReports { get; protected set; } = new();
    }
}