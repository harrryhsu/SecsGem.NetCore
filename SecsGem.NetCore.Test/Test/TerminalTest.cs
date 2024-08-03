using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Test.Test
{
    public class TerminalTest : SecsGemTestBase
    {
        private SecsGemTerminalDisplayEvent ServerEvent { get; set; }

        private SecsGemTerminalDisplayEvent ClientEvent { get; set; }

        public override async Task Setup()
        {
            await base.Setup();

            _server.Feature.Terminals.Add(new Terminal
            {
                Id = 1,
                Name = "Test",
            });

            _server.OnEvent += async (sender, e) =>
            {
                if (e is SecsGemTerminalDisplayEvent evt)
                {
                    ServerEvent = evt;
                    var text = evt.Texts.First();
                    if (text == "Accepted")
                    {
                        evt.Return = SECS_RESPONSE.ACKC10.Accepted;
                    }
                    else
                    {
                        evt.Return = SECS_RESPONSE.ACKC10.WillNotBeDisplayed;
                    }
                }
            };

            _client.OnEvent += async (sender, e) =>
            {
                if (e is SecsGemTerminalDisplayEvent evt)
                {
                    ClientEvent = evt;
                    var text = evt.Texts.First();
                    if (evt.Id == 2)
                    {
                        evt.Return = SECS_RESPONSE.ACKC10.NotAvailable;
                    }
                    else if (text == "Accepted")
                    {
                        evt.Return = SECS_RESPONSE.ACKC10.Accepted;
                    }
                    else
                    {
                        evt.Return = SECS_RESPONSE.ACKC10.WillNotBeDisplayed;
                    }
                }
            };
        }

        [Test]
        public async Task Terminal_Display_Response()
        {
            Assert.That(await _client.Function.SendTerminal(1, "Accepted"), Is.EqualTo(SECS_RESPONSE.ACKC10.Accepted));
            Assert.That(await _client.Function.SendTerminal(2, "Accepted"), Is.EqualTo(SECS_RESPONSE.ACKC10.NotAvailable));
            Assert.That(await _client.Function.SendTerminal(1, "WillNotBeDisplayed"), Is.EqualTo(SECS_RESPONSE.ACKC10.WillNotBeDisplayed));

            var texts = new List<string> { "Accepted", "Test" };
            Assert.That(await _client.Function.SendTerminalMulti(1, texts), Is.EqualTo(SECS_RESPONSE.ACKC10.Accepted));
            Assert.That(ServerEvent.Texts, Is.EquivalentTo(texts));

            Assert.That(await _client.Function.SendTerminalBroadcast("Accepted"), Is.EqualTo(SECS_RESPONSE.ACKC10.Accepted));
            Assert.That(ServerEvent.IsBroadcast, Is.True);

            Assert.That(await _server.Function.SendTerminal(1, "Accepted"), Is.EqualTo(SECS_RESPONSE.ACKC10.Accepted));
            Assert.That(await _server.Function.SendTerminal(2, "Accepted"), Is.EqualTo(SECS_RESPONSE.ACKC10.NotAvailable));
            Assert.That(await _server.Function.SendTerminal(1, "WillNotBeDisplayed"), Is.EqualTo(SECS_RESPONSE.ACKC10.WillNotBeDisplayed));
        }
    }
}