using SecsGem.NetCore.State.Client;
using SecsGem.NetCore.State.Server;
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

            await AssertEx.DoesNotThrowAsync(async () =>
            {
                await _client.State.WaitForState(GemClientStateModel.Selected);
                await _server.State.WaitForState(GemServerStateModel.Selected);
            });
        }

        [Test]
        public async Task Client_Communication_Online()
        {
            await Init(true, false);
            await AssertEx.DoesNotThrowAsync(async () =>
            {
                await _client.State.WaitForState(GemClientStateModel.ControlOffLine);
                await _server.State.WaitForState(GemServerStateModel.ControlOffLine);
            });
        }

        [Test]
        public async Task Server_Communication_Online()
        {
            await Init(false, true);
            await AssertEx.DoesNotThrowAsync(async () =>
            {
                await _client.State.WaitForState(GemClientStateModel.ControlOffLine);
                await _server.State.WaitForState(GemServerStateModel.ControlOffLine);
            });
        }

        [Test]
        public async Task Communication_Timeout()
        {
            await Init(false, false);
            await AssertEx.ThrowAsync(async () =>
            {
                await _client.State.WaitForState(GemClientStateModel.ControlOffLine);
                await _server.State.WaitForState(GemServerStateModel.ControlOffLine);
            });

            await _client.Function.CommunicationEstablish();
            Assert.That(_client.State.Current, Is.EqualTo(GemClientStateModel.ControlOffLine));
        }

        [Test]
        public async Task Connection_Dispose()
        {
            await Init(true, false);
            await AssertEx.DoesNotThrowAsync(async () =>
            {
                await _client.State.WaitForState(GemClientStateModel.ControlOffLine);
                await _server.State.WaitForState(GemServerStateModel.ControlOffLine);
            });
            await _client.Function.Separate();
            await AssertEx.DoesNotThrowAsync(async () =>
            {
                await _server.State.WaitForState(GemServerStateModel.Disconnected);
            });
        }

        [Test]
        public async Task Control_Online()
        {
            await Init(true, false);
            await AssertEx.DoesNotThrowAsync(async () =>
            {
                await _client.State.WaitForState(GemClientStateModel.ControlOffLine);
                await _server.State.WaitForState(GemServerStateModel.ControlOffLine);
            });
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