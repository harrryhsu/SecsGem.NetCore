using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.State.Server;

namespace SecsGem.NetCore.Test.Test
{
    public class StateTransitionTest : SecsGemTestBase
    {
        protected override async Task AfterSetup()
        {
        }

        [Test]
        public async Task Allowed_State_Change()
        {
            var success = await _client.Function.S1F17RequestOnline();

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

            var success = await _client.Function.S1F17RequestOnline();

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