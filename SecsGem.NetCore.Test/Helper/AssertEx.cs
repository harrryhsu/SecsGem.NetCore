namespace SecsGem.NetCore.Test.Helper
{
    public static class AssertEx
    {
        public static async Task<Exception> CatchAsync(Func<Task> callback)
        {
            return await CatchAsync<Exception>(callback);
        }

        public static async Task<TException> CatchAsync<TException>(Func<Task> callback) where TException : Exception
        {
            TException ex = null;
            try
            {
                await callback();
            }
            catch (TException x)
            {
                ex = x;
            }

            Assert.Catch(() =>
            {
                if (ex != null) throw ex;
            });

            return ex;
        }
    }
}