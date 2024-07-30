using SecsGem.NetCore.Feature.Client;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Helper;
using SecsGem.NetCore.Test.Helper;
using System.Net;

namespace SecsGem.NetCore.Test.Test
{
    public class Connection : SecsGemTestBase
    {
        public override async Task Setup()
        {
        }

        private async Task Init(bool clientActive, bool serverActive)
        {
            var target = new IPEndPoint(IPAddress.Loopback, 8080);
            _client = new(new SecsGemOption
            {
                Debug = true,
                ActiveConnect = clientActive,
                Target = target,
                Logger = (msg) => Console.WriteLine(DateTime.Now.ToString("ss:fff") + " Client " + msg),
            });

            _server = new(new SecsGemOption
            {
                Debug = true,
                ActiveConnect = serverActive,
                Target = target,
                Logger = (msg) => Console.WriteLine(DateTime.Now.ToString("ss:fff") + " Server " + msg)
            });

            await _server.StartAsync();
            await _client.ConnectAsync();
        }

        [Test]
        public async Task Test_Connection()
        {
            await Init(false, false);
            await TaskHelper.WaitFor(() => _client.State.IsExact(GemClientStateModel.Selected) && _server.State.IsExact(GemServerStateModel.Selected), 10, 100);
        }

        [Test]
        public async Task Client_Communication_Online()
        {
            await Init(true, false);
            await TaskHelper.WaitFor(() => _client.State.IsExact(GemClientStateModel.ControlOffLine) && _server.State.IsExact(GemServerStateModel.ControlOffLine), 10, 100);
        }

        [Test]
        public async Task Server_Communication_Online()
        {
            await Init(false, true);
            await TaskHelper.WaitFor(() => _client.State.IsExact(GemClientStateModel.ControlOffLine) && _server.State.IsExact(GemServerStateModel.ControlOffLine), 10, 100);
        }

        [Test]
        public async Task Communication_Timeout()
        {
            await Init(false, false);
            await AssertEx.ThrowAsync<TimeoutException>(async () =>
            {
                await TaskHelper.WaitFor(
                    () => _client.State.IsExact(GemClientStateModel.ControlOffLine)
                        && _server.State.IsExact(GemServerStateModel.ControlOffLine),
                    10,
                    100
                );
            });

            await _client.Function.CommunicationEstablish();
            Assert.That(_client.State.Current, Is.EqualTo(GemClientStateModel.ControlOffLine));
        }

        [Test]
        public async Task Connection_Dispose()
        {
            await Init(true, false);
            await TaskHelper.WaitFor(() => _client.State.IsMoreThan(GemClientStateModel.Selected) && _server.State.IsMoreThan(GemServerStateModel.Selected), 10, 100);
            await _client.Function.Separate();
            await Task.Delay(100);
            Assert.That(_server.State.Current, Is.EqualTo(GemServerStateModel.Disconnected));
        }

        [Test]
        public async Task Control_Online()
        {
            await Init(true, false);
            await TaskHelper.WaitFor(() => _client.State.IsExact(GemClientStateModel.ControlOffLine) && _server.State.IsExact(GemServerStateModel.ControlOffLine), 10, 100);
            var ack = await _client.Function.ControlOnline();
            Assert.Multiple(() =>
            {
                Assert.That(ack, Is.True);
                Assert.That(_client.State.Current, Is.EqualTo(GemClientStateModel.ControlOnline));
                Assert.That(_server.State.Current, Is.EqualTo(GemServerStateModel.ControlOnlineRemote));
            });
        }
    }
}