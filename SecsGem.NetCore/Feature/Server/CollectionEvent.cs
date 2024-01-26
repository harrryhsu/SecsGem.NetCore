namespace SecsGem.NetCore.Feature.Server
{
    public class CollectionEvent
    {
        public uint Id { get; set; }

        public string Name { get; set; }

        public bool Enabled { get; set; }

        public List<CollectionReport> CollectionReports { get; set; } = new();
    }
}