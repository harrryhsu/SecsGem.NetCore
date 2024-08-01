using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Client;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Helper;
using System.Net;

namespace SecsGem.NetCore.Test.Test
{
    public class StateTransitionTest : SecsGemTestBase
    {
        [SetUp]
        public override async Task Setup()
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

            await TaskHelper.WaitFor(() => _client.State.IsExact(GemClientStateModel.ControlOffLine) && _server.State.IsExact(GemServerStateModel.ControlOffLine), 10, 100);
        }

        [Test]
        public async Task Allowed_State_Change()
        {
            var success = await _client.Function.ControlOnline();

            Assert.Multiple(() =>
            {
                Assert.That(success, Is.True);
                Assert.That(_server.State.Current, Is.EqualTo(GemServerStateModel.ControlOnlineRemote));
            });
        }

        [Test]
        public async Task Denied_State_Change()
        {
            _server.OnEvent += async (sender, evt) =>
            {
                if (evt is SecsGemServerStateChangeEvent e)
                {
                    if (e.Trigger == GemServerStateTrigger.GoOnline)
                    {
                        e.Accept = false;
                    }
                }
            };

            var success = await _client.Function.ControlOnline();

            Assert.Multiple(() =>
            {
                Assert.That(success, Is.False);
                Assert.That(_server.State.Current, Is.EqualTo(GemServerStateModel.ControlOffLine));
            });
        }

        [Test]
        public async Task Force_State_Change()
        {
            _server.OnEvent += async (sender, evt) =>
            {
                if (evt is SecsGemServerStateChangeEvent e)
                {
                    if (e.Trigger == GemServerStateTrigger.GoOnline)
                    {
                        e.Accept = false;
                    }
                }
            };

            var success = await _server.State.TriggerAsync(GemServerStateTrigger.GoOnline, true);

            Assert.Multiple(() =>
            {
                Assert.That(success, Is.True);
                Assert.That(_server.State.Current, Is.EqualTo(GemServerStateModel.ControlOnlineRemote));
            });
        }
    }
}