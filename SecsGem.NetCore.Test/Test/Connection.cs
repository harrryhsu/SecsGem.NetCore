using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Helper;

namespace SecsGem.NetCore.Test.Test
{
    public class Connection : SecsGemTestBase
    {
        public Connection()
        {
            _controlOnline = false;
        }

        [Test]
        public async Task Test_Connection()
        {
            await TaskHelper.WaitFor(() => _client.Device.IsSelected && _server.Device.IsSelected, 10, 50);
        }

        [Test]
        public async Task Communication_Online()
        {
            await TaskHelper.WaitFor(() => _client.Device.IsCommunicationOnline && _server.Device.IsCommunicationOnline, 10, 50);
        }

        [Test]
        public async Task Connection_Dispose()
        {
            await TaskHelper.WaitFor(() => _client.Device.IsSelected && _server.Device.IsSelected, 10, 50);
            await _client.Function.Teardown();
            await Task.Delay(100);
            Assert.That(_server.Device.IsSelected, Is.False);
        }

        [Test]
        public async Task Control_Online()
        {
            await TaskHelper.WaitFor(() => _client.Device.IsCommunicationOnline && _server.Device.IsCommunicationOnline, 10, 50);
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
            await TaskHelper.WaitFor(() => _client.Device.IsCommunicationOnline && _server.Device.IsCommunicationOnline, 10, 50);
            var online = await _client.Function.IsEquipmentControlOnline();

            Assert.That(online, Is.False);
            Assert.CatchAsync<SecsGemTransactionException>(async () =>
            {
                await _client.Function.GetAlarmDefinitions();
            });

            _server.Device.ControlState.State = ControlStateModel.ControlHostOffLine;
            var ack = await _client.Function.ControlOnline();
            await _client.Function.GetAlarmDefinitions();
        }
    }
}