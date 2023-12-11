namespace SecsGem.NetCore
{
    public enum SecsGemExceptionType
    {
        Connection = 0,

        Transaction,

        General,
    }

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

    public class SecsGemConnectionException : SecsGemException
    {
        public override SecsGemExceptionType Type => SecsGemExceptionType.Connection;

        public SecsGemConnectionException(string msg) : base(msg)
        {
        }

        public SecsGemConnectionException(string msg, Exception ex) : base(msg, ex)
        {
        }
    }

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