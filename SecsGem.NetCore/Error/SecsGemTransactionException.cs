namespace SecsGem.NetCore.Error
{
    public class SecsGemTransactionException : SecsGemException
    {
        public override SecsGemExceptionType Type => SecsGemExceptionType.Transaction;

        public SecsGemTransactionException(string msg) : base(msg)
        {
        }

        public SecsGemTransactionException(string msg, Exception ex) : base(msg, ex)
        {
        }
    }
}