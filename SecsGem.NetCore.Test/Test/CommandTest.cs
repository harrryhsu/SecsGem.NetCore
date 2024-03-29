using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using static SecsGem.NetCore.Handler.Server.SecsGemStream2Handler;

namespace SecsGem.NetCore.Test.Test
{
    public class CommandTest : SecsGemTestBase
    {
        public override async Task Setup()
        {
            await base.Setup();
            _server.Feature.Commands.Add(new Command
            {
                Name = "TEST",
                Description = "TEST DESCRIPTION",
            });
        }

        [Test]
        public async Task Command_Send()
        {
            var ack = await _client.Function.CommandSend("TEST1", new Dictionary<string, string> { });
            Assert.That(ack, Is.EqualTo(S2F42_HCACK.InvalidCommand));

            var testVal = "1";
            _server.OnEvent += async (sender, evt) =>
            {
                if (evt is SecsGemCommandExecuteEvent nevt)
                {
                    if (nevt.Cmd.Name == "TEST")
                    {
                        testVal = nevt.Params.FirstOrDefault(x => x.Key == "Key").Value;
                    }
                }
            };

            ack = await _client.Function.CommandSend("TEST", new Dictionary<string, string> { { "Key", "2" } });
            Assert.That(ack, Is.EqualTo(S2F42_HCACK.Ok));
            Assert.That(testVal, Is.EqualTo("2"));
        }
    }
}