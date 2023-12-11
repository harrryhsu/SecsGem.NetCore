namespace SecsGem.NetCore.Feature
{
    public class CollectionReport
    {
        public uint Id { get; set; }

        public List<DataVariable> DataVariables { get; set; } = new();
    }
}