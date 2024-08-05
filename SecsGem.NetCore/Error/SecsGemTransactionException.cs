using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Error
{
    public class SecsGemTransactionException : SecsGemException
    {
        public override SecsGemExceptionType Type => SecsGemExceptionType.Transaction;

        public HsmsMessage HsmsMessage { get; set; }

        public SecsGemTransactionException(HsmsMessage hsms, string msg) : base(msg)
        {
            HsmsMessage = hsms;
        }
    }
}