using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Test.Test
{
    public class StatusVariableTest : SecsGemTestBase
    {
        public StatusVariable SV = new StatusVariable
        {
            Id = 1,
            Name = "Test",
            Unit = "/g",
            Value = "123",
        };

        public override async Task Setup()
        {
            await base.Setup();
            _server.Feature.StatusVariables.Add(SV);
        }

        [Test]
        public async Task Get_Status_Variable()
        {
            var svs = await _client.Function.StatusVariableDefinitionGet();
            var values = await _client.Function.StatusVariableValueGet(new uint[] { 1 });

            Assert.That(svs.Count(), Is.EqualTo(1));
            Assert.That(values.Count(), Is.EqualTo(1));

            var sv = svs.First();
            sv.Value = values[sv.Id];

            Assert.Multiple(() =>
            {
                Assert.That(sv.Id, Is.EqualTo(SV.Id));
                Assert.That(sv.Unit, Is.EqualTo(SV.Unit));
                Assert.That(sv.Name, Is.EqualTo(SV.Name));
                Assert.That(sv.Value, Is.EqualTo(SV.Value));
            });
        }

        [Test]
        public async Task Get_Status_Variable_Event_Callback()
        {
            _server.OnEvent += async (sender, evt) =>
            {
                if (evt is SecsGemGetStatusVariableEvent nevt)
                {
                    foreach (var item in nevt.Params)
                    {
                        item.Value = "200";
                    }
                }
            };

            var svs = await _client.Function.StatusVariableDefinitionGet();
            var values = await _client.Function.StatusVariableValueGet(new uint[] { 1 });

            Assert.That(svs.Count(), Is.EqualTo(1));
            Assert.That(values.Count(), Is.EqualTo(1));

            var sv = svs.First();
            sv.Value = values[sv.Id];

            Assert.Multiple(() =>
            {
                Assert.That(sv.Id, Is.EqualTo(SV.Id));
                Assert.That(sv.Unit, Is.EqualTo(SV.Unit));
                Assert.That(sv.Name, Is.EqualTo(SV.Name));
                Assert.That(sv.Value, Is.EqualTo("200"));
            });
        }
    }
}