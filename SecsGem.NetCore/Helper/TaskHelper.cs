using System.Diagnostics;

namespace SecsGem.NetCore.Helper
{
    public static class TaskHelper
    {
        public static async Task WaitFor(Func<bool> condition, int maxRety, int retryDelay, CancellationToken token = default)
        {
            var retryCount = 0;
            while (retryCount < maxRety)
            {
                token.ThrowIfCancellationRequested();
                if (condition()) return;
                retryCount++;
                await Task.Delay(retryDelay, token);
            }

            throw new TimeoutException("Retry Timeout");
        }

        private static readonly double TickPerUs = 0.001 * 0.001 * Stopwatch.Frequency;

        public static void SpinWait(int us)
        {
            var durationTicks = us * TickPerUs;
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedTicks < durationTicks) ;
        }
    }
}