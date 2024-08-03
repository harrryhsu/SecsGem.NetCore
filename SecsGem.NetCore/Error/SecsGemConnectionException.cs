namespace SecsGem.NetCore.Error
{
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
}