namespace SecsGem.NetCore.Test.Helper
{
    public static class AssertEx
    {
        public static async Task<Exception> ThrowAsync(Func<Task> callback)
        {
            return await ThrowAsync<Exception>(callback);
        }

        public static async Task<TException> ThrowAsync<TException>(Func<Task> callback) where TException : Exception
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

        public static async Task DoesNotThrowAsync(Func<Task> callback)
        {
            Exception ex = null;
            try
            {
                await callback();
            }
            catch (Exception x)
            {
                ex = x;
            }

            Assert.DoesNotThrow(() =>
            {
                if (ex != null) throw ex;
            });
        }
    }
}