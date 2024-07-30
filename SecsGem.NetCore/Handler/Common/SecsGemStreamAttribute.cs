namespace SecsGem.NetCore.Handler.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SecsGemStreamAttribute : Attribute
    {
        public int Stream { get; set; }

        public int Function { get; set; }

        public SecsGemStreamAttribute(int stream, int function)
        {
            Stream = stream;
            Function = function;
        }
    }
}