using SecsGem.NetCore.Event.Client;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Test.Test
{
    public class AlarmTest : SecsGemTestBase
    {
        private Alarm _alarm;

        public override async Task Setup()
        {
            await base.Setup();
            _alarm = new()
            {
                Id = 1,
                Description = "Test",
                Enabled = true,
            };
            _server.Feature.Alarms.Add(_alarm);
        }

        [Test]
        public async Task Alarm_Toggle()
        {
            await _client.Function.S5F3EnableDisableAlarmSend(1, false);
            Assert.That(_alarm.Enabled, Is.False);

            await _client.Function.S5F3EnableDisableAlarmSend(1, true);
            Assert.That(_alarm.Enabled, Is.True);
        }

        [Test]
        public async Task Get_Alarm_Definitions()
        {
            var alarms = await _client.Function.S5F5ListAlarmsRequest();
            Assert.That(alarms.Count(), Is.EqualTo(1));
            var fa = alarms.First();
            Assert.That(fa.Id, Is.EqualTo(_alarm.Id));
            Assert.That(fa.Description, Is.EqualTo(_alarm.Description));
            Assert.That(fa.Enabled, Is.EqualTo(_alarm.Enabled));

            alarms = await _client.Function.S5F5ListAlarmsRequest(new uint[] { 5 });
            Assert.That(alarms.Count(), Is.EqualTo(1));
            fa = alarms.First();
            Assert.That(fa.Id, Is.EqualTo(5));
            Assert.That(fa.Description, Is.EqualTo(string.Empty));
            Assert.That(fa.Enabled, Is.EqualTo(false));
        }

        [Test]
        public async Task Get_Enabled_Alarm_Definitions()
        {
            var alarms = await _client.Function.S5F7ListEnabledAlarmRequest();
            Assert.That(alarms.Count(), Is.EqualTo(1));

            await _client.Function.S5F3EnableDisableAlarmSend(1, false);
            alarms = await _client.Function.S5F7ListEnabledAlarmRequest();
            Assert.That(alarms.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task Alarm_Send()
        {
            Alarm alarm = null;
            _client.OnEvent += async (sender, evt) =>
            {
                if (evt is SecsGemAlarmEvent nevt)
                {
                    alarm = nevt.Alarm;
                }
            };

            await _client.Function.S5F3EnableDisableAlarmSend(1, false);
            await _server.Function.S5F1AlarmReportSend(1);
            Assert.That(alarm, Is.Null);

            await _client.Function.S5F3EnableDisableAlarmSend(1, true);
            await _server.Function.S5F1AlarmReportSend(1);
            Assert.That(alarm, Is.Not.Null);
            Assert.That(alarm.Id, Is.EqualTo(1));
        }
    }
}