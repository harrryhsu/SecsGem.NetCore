using SecsGem.NetCore.State.Client;
using SecsGem.NetCore.State.Server;
using SecsGem.NetCore.Test.Helper;
using System.Diagnostics;
using System.Net;

[assembly: LevelOfParallelism(12)]

namespace SecsGem.NetCore.Test
{
    [NonParallelizable]
    [TestFixture]
    public class SecsGemTestBase
    {
        protected SecsGemClient _client;

        protected SecsGemServer _server;

        [SetUp]
        public virtual async Task Setup()
        {
            var target = new IPEndPoint(IPAddress.Loopback, 8080);
            _client = new(new SecsGemOption
            {
                Debug = true,
                ActiveConnect = true,
                Target = target,
                Logger = (msg) => Console.WriteLine(DateTime.Now.ToString("ss:fff") + " Client " + msg),
            });

            _server = new(new SecsGemOption
            {
                Debug = true,
                Target = target,
                Logger = (msg) => Console.WriteLine(DateTime.Now.ToString("ss:fff") + " Server " + msg)
            });

            await _server.StartAsync();
            await _client.ConnectAsync();

            await AssertEx.DoesNotThrowAsync(async () =>
            {
                await _client.State.WaitForState(GemClientStateModel.ControlOffLine);
                await _server.State.WaitForState(GemServerStateModel.ControlOffLine);
            });

            await AfterSetup();
        }

        protected virtual async Task AfterSetup()
        {
            await _client.Function.S1F17RequestOnline();
        }

        [TearDown]
        public virtual async Task TearDown()
        {
            await _server.DisposeAsync();
            await _client.DisposeAsync();
        }

        [OneTimeSetUp]
        public void StartTest()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        [OneTimeTearDown]
        public void EndTest()
        {
            Trace.Flush();
        }
    }
}