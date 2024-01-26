using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Helper;
using System.Diagnostics;
using System.Net;

namespace SecsGem.NetCore.Test
{
    [NonParallelizable]
    [TestFixture]
    public class SecsGemTestBase
    {
        protected SecsGemClient _client;

        protected SecsGemServer _server;

        protected bool _controlOnline = true;

        [SetUp]
        public virtual async Task Setup()
        {
            var target = new IPEndPoint(IPAddress.Loopback, 8080);
            _client = new(new SecsGemOption
            {
                Debug = true,
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

            if (_controlOnline)
            {
                await TaskHelper.WaitFor(() => _client.Device.IsCommunicationOnline && _server.Device.IsCommunicationOnline, 10, 50);
                _server.Device.ControlState.State = ControlStateModel.ControlHostOffLine;
                await _client.Function.ControlOnline();
            }
        }

        [TearDown]
        public virtual async Task TearDown()
        {
            await _server.DisposeAsync();
            await Task.Delay(50);
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