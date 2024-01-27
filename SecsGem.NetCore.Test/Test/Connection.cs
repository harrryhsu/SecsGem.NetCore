using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Helper;
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
            await TaskHelper.WaitFor(() => _client.Device.IsSelected && _server.Device.IsSelected, 10, 100);
        }

        [Test]
        public async Task Client_Communication_Online()
        {
            await Init(true, false);
            await TaskHelper.WaitFor(() => _client.Device.IsCommunicationOnline && _server.Device.IsCommunicationOnline, 10, 100);
        }

        [Test]
        public async Task Server_Communication_Online()
        {
            await Init(false, true);
            await TaskHelper.WaitFor(() => _client.Device.IsCommunicationOnline && _server.Device.IsCommunicationOnline, 10, 100);
        }

        [Test]
        public async Task Communication_Timeout()
        {
            await Init(false, false);
            Assert.CatchAsync<TimeoutException>(async () =>
            {
                await TaskHelper.WaitFor(() => _client.Device.IsCommunicationOnline && _server.Device.IsCommunicationOnline, 10, 100);
            });

            await _client.Function.CommunicationEstablish();
            Assert.That(_client.Device.IsCommunicationOnline, Is.True);
        }

        [Test]
        public async Task Connection_Dispose()
        {
            await Init(true, false);
            await TaskHelper.WaitFor(() => _client.Device.IsSelected && _server.Device.IsSelected, 10, 100);
            await _client.Function.Separate();
            await Task.Delay(100);
            Assert.That(_server.Device.IsSelected, Is.False);
        }

        [Test]
        public async Task Control_Online()
        {
            await Init(true, false);
            await TaskHelper.WaitFor(() => _client.Device.IsCommunicationOnline && _server.Device.IsCommunicationOnline, 10, 100);
            var ack = await _client.Function.ControlOnline();
            Assert.That(ack, Is.False);
            _server.Device.ControlState.State = ControlStateModel.ControlHostOffLine;
            ack = await _client.Function.ControlOnline();
            Assert.Multiple(() =>
            {
                Assert.That(ack, Is.True);
                Assert.That(_client.Device.ControlState, Is.EqualTo(ControlStateModel.ControlOnline));
                Assert.That(_server.Device.ControlState.IsControlOnline, Is.True);
            });
        }

        [Test]
        public async Task Control_Offline_Invalid_Message()
        {
            await Init(true, false);
            await TaskHelper.WaitFor(() => _client.Device.IsCommunicationOnline && _server.Device.IsCommunicationOnline, 10, 100);
            var online = await _client.Function.IsEquipmentControlOnline();

            Assert.That(online, Is.False);
            Assert.CatchAsync<SecsGemTransactionException>(async () =>
            {
                await _client.Function.AlarmDefinitionGet();
            });

            _server.Device.ControlState.State = ControlStateModel.ControlHostOffLine;
            var ack = await _client.Function.ControlOnline();
            await _client.Function.AlarmDefinitionGet();
        }
    }
}