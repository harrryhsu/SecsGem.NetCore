using SecsGem.NetCore.Event.Server;

namespace SecsGem.NetCore.Test.Test
{
    public class TimeTest : SecsGemTestBase
    {
        [Test]
        public async Task Get_Time()
        {
            var timeStr = await _client.Function.ServerTimeGet();
            var success = DateTime.TryParse(timeStr, out var serverTime);
            Assert.That(success, Is.True);
            Assert.That(serverTime - DateTime.Now, Is.LessThan(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public async Task Set_Time()
        {
            DateTime serverTime = DateTime.MinValue;
            _server.OnEvent += async (sender, evt) =>
            {
                if (evt is SecsGemSetTimeEvent nevt)
                {
                    if (DateTime.TryParse(nevt.Time, out var time))
                    {
                        serverTime = time;
                        nevt.Success = true;
                    }
                }
            };

            var success = await _client.Function.ServerTimeSet("test");
            Assert.That(success, Is.False);
            Assert.That(serverTime, Is.EqualTo(DateTime.MinValue));

            success = await _client.Function.ServerTimeSet(DateTime.Now);
            Assert.That(success, Is.True);
            Assert.That(serverTime - DateTime.Now, Is.LessThan(TimeSpan.FromSeconds(1)));
        }
    }
}