using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Test.Test
{
    public class DataVariableTest : SecsGemTestBase
    {
        [Test]
        public async Task Get_Data_Variable()
        {
            var v = new DataVariable
            {
                Id = "1",
                Description = "Test",
                Unit = "/g",
                Value = "123",
            };
            _server.Feature.DataVariables.Add(v);

            var vs = await _client.Function.DataVariableDefinitionGet();

            Assert.That(vs.Count(), Is.EqualTo(1));

            var fv = vs.First();

            Assert.Multiple(() =>
            {
                Assert.That(fv.Id, Is.EqualTo(v.Id));
                Assert.That(fv.Unit, Is.EqualTo(v.Unit));
                Assert.That(fv.Description, Is.EqualTo(v.Description));
            });
        }
    }
}