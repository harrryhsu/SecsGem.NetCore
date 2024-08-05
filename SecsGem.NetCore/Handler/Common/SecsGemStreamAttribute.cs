namespace SecsGem.NetCore.Handler.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SecsGemStreamAttribute : Attribute
    {
        public byte Stream { get; set; }

        public byte Function { get; set; }

        public SecsGemStreamAttribute(byte stream, byte function)
        {
            Stream = stream;
            Function = function;
        }
    }
}