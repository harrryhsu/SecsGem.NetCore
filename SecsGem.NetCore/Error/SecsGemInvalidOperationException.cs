namespace SecsGem.NetCore.Error
{
    public class SecsGemInvalidOperationException : SecsGemException
    {
        public override SecsGemExceptionType Type => SecsGemExceptionType.InvalidOperation;

        public SecsGemInvalidOperationException(string msg = "") : base(msg)
        {
        }

        public SecsGemInvalidOperationException(string msg, Exception ex) : base(msg, ex)
        {
        }
    }
}