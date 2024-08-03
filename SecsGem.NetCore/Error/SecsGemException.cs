namespace SecsGem.NetCore.Error
{
    public class SecsGemException : Exception
    {
        public virtual SecsGemExceptionType Type => SecsGemExceptionType.General;

        public string Code { get; set; }

        public SecsGemException(string msg) : base(msg)
        {
        }

        public SecsGemException(string msg, Exception ex) : base(msg, ex)
        {
        }
    }
}