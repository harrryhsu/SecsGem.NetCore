using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Test.Test
{
    public class ProcessProgramTest : SecsGemTestBase
    {
        public override async Task Setup()
        {
            await base.Setup();

            _server.Feature.ProcessPrograms.Add(new ProcessProgram
            {
                Id = "TEST1",
                Body = new byte[] { 1, 2, 3, 4 }
            });
        }

        [Test]
        public async Task Load_Process_Program()
        {
            var ack1 = await _client.Function.S7F1ProcessProgramLoadInquire("TEST1", 4);
            Assert.That(ack1, Is.EqualTo(SECS_RESPONSE.PPGNT.AlreadyHave));

            ack1 = await _client.Function.S7F1ProcessProgramLoadInquire("TEST2", 4);
            Assert.That(ack1, Is.EqualTo(SECS_RESPONSE.PPGNT.Ok));

            var ack2 = await _client.Function.S7F3ProcessProgramSend("TEST3", new byte[] { 1, 2, 3, 4 });
            Assert.That(ack2, Is.EqualTo(SECS_RESPONSE.ACKC7.PPIDNotFound));

            ack2 = await _client.Function.S7F3ProcessProgramSend("TEST2", new byte[] { 1, 2, 3, 4 });
            Assert.That(ack2, Is.EqualTo(SECS_RESPONSE.ACKC7.Accept));
        }

        [Test]
        public async Task Get_Process_Program()
        {
            var pp = await _client.Function.S7F5ProcessProgramRequest("TEST2");
            Assert.That(pp.Body, Is.Empty);

            pp = await _client.Function.S7F5ProcessProgramRequest("TEST1");
            Assert.That(pp.Body, Has.Length.EqualTo(4));

            var ids = await _client.Function.S7F19CurrentProcessProgramDirRequest();
            Assert.That(ids.Count(), Is.EqualTo(1));

            await _client.Function.S7F1ProcessProgramLoadInquire("TEST2", 4);
            await _client.Function.S7F3ProcessProgramSend("TEST2", new byte[] { 1, 2, 3, 4 });
            ids = await _client.Function.S7F19CurrentProcessProgramDirRequest();
            Assert.That(ids.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Delete_Process_Program()
        {
            var ack = await _client.Function.S7F17DeleteProcessProgramSend(new string[] { "TEST2" });
            Assert.That(ack, Is.EqualTo(SECS_RESPONSE.ACKC7.PPIDNotFound));

            ack = await _client.Function.S7F17DeleteProcessProgramSend(new string[] { "TEST1" });
            Assert.That(ack, Is.EqualTo(SECS_RESPONSE.ACKC7.Accept));
            var ids = await _client.Function.S7F19CurrentProcessProgramDirRequest();
            Assert.That(ids.Count(), Is.EqualTo(0));
        }
    }
}