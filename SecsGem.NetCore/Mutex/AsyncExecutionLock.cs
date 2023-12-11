namespace SecsGem.NetCore.Mutex
{
    public class AsyncExecutionLock
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public void Execute(Action action)
        {
            _semaphore.Wait();

            try
            {
                action.Invoke();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ExecuteAsync(Func<Task> action)
        {
            await _semaphore.WaitAsync();

            try
            {
                await action.Invoke();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<TReturn> ExecuteAsync<TReturn>(Func<Task<TReturn>> action)
        {
            await _semaphore.WaitAsync();

            try
            {
                return await action.Invoke();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}