using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using static SecsGem.NetCore.Handler.Server.SecsGemStream2Handler;

namespace SecsGem.NetCore.Test.Test
{
    public class EquipmentConstantTest : SecsGemTestBase
    {
        public EquipmentConstant EC = new()
        {
            Id = 1,
            Name = "Test",
            Unit = "/g",
            Min = 1,
            Max = 200,
            Value = 100,
            Default = 100
        };

        public override async Task Setup()
        {
            await base.Setup();
            _server.Feature.EquipmentConstants.Add(EC);
        }

        [Test]
        public async Task Get_Equipment_Constant()
        {
            var vs = await _client.Function.GetEquipmentConstantDefinitions();
            var values = await _client.Function.GetEquipmentConstantValues(new uint[] { EC.Id });

            Assert.That(vs.Count(), Is.EqualTo(1));
            Assert.That(values.Count(), Is.EqualTo(1));

            var vf = vs.First();
            vf.Value = values[vf.Id];

            Assert.Multiple(() =>
            {
                Assert.That(vf.Id, Is.EqualTo(EC.Id));
                Assert.That(vf.Unit, Is.EqualTo(EC.Unit));
                Assert.That(vf.Name, Is.EqualTo(EC.Name));
                Assert.That(vf.Value, Is.EqualTo(EC.Value));
                Assert.That(vf.Default, Is.EqualTo(EC.Default));
                Assert.That(vf.Min, Is.EqualTo(EC.Min));
                Assert.That(vf.Max, Is.EqualTo(EC.Max));
            });
        }

        [Test]
        public async Task Get_Equipment_Constant_Event_Callback()
        {
            _server.OnEvent += async (sender, evt) =>
            {
                if (evt is SecsGemGetEquipmentConstantEvent ecEvt)
                {
                    foreach (var item in ecEvt.Params)
                    {
                        item.Value = 105;
                    }
                }
            };

            var vs = await _client.Function.GetEquipmentConstantDefinitions();
            var values = await _client.Function.GetEquipmentConstantValues(new uint[] { EC.Id });

            Assert.That(vs.Count(), Is.EqualTo(1));
            Assert.That(values.Count(), Is.EqualTo(1));

            var vf = vs.First();
            vf.Value = values[vf.Id];

            Assert.Multiple(() =>
            {
                Assert.That(vf.Id, Is.EqualTo(EC.Id));
                Assert.That(vf.Unit, Is.EqualTo(EC.Unit));
                Assert.That(vf.Name, Is.EqualTo(EC.Name));
                Assert.That(vf.Value, Is.EqualTo(105));
                Assert.That(vf.Default, Is.EqualTo(EC.Default));
                Assert.That(vf.Min, Is.EqualTo(EC.Min));
                Assert.That(vf.Max, Is.EqualTo(EC.Max));
            });
        }

        [Test]
        public async Task Set_Equipment_Constant()
        {
            var ack = await _client.Function.SetEquipmentConstants(new List<EquipmentConstant> { new() { Id = EC.Id, Value = -5 } });
            Assert.That(ack, Is.EqualTo(S2F15_EAC.OneOrMoreValueOutOfRange));

            ack = await _client.Function.SetEquipmentConstants(new List<EquipmentConstant> { new() { Id = EC.Id, Value = 205 } });
            Assert.That(ack, Is.EqualTo(S2F15_EAC.OneOrMoreValueOutOfRange));

            ack = await _client.Function.SetEquipmentConstants(new List<EquipmentConstant> { new() { Id = EC.Id + 1, Value = 10 } });
            Assert.That(ack, Is.EqualTo(S2F15_EAC.OneOrMoreConstantDoNotExist));

            ack = await _client.Function.SetEquipmentConstants(new List<EquipmentConstant> { new() { Id = EC.Id, Value = 50 } });
            Assert.That(ack, Is.EqualTo(S2F15_EAC.Ok));
            Assert.That(_server.Feature.EquipmentConstants.First().Value, Is.EqualTo(50));
        }
    }
}