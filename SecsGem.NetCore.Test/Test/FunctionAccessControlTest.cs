using SecsGem.NetCore.Error;
using SecsGem.NetCore.Test.Helper;

namespace SecsGem.NetCore.Test.Test
{
    public class FunctionAccessControlTest : SecsGemTestBase
    {
        protected override async Task AfterSetup()
        {
        }

        [Test]
        public async Task Check_Function_Access_Control()
        {
            await AssertEx.ThrowAsync<SecsGemTransactionException>(async () =>
            {
                var ecs = await _client.Function.S2F29EquipmentConstantNamelistRequest();
            });

            await _client.Function.S1F17RequestOnline();
            await _server.Function.GoOnlineLocal();

            await AssertEx.DoesNotThrowAsync(async () =>
            {
                var ecs = await _client.Function.S2F29EquipmentConstantNamelistRequest();
            });

            await AssertEx.ThrowAsync<SecsGemTransactionException>(async () =>
            {
                var ecs = await _client.Function.S2F31DateAndTimeSetRequest(DateTime.Now);
            });

            await _server.Function.GoOnlineRemote();

            await AssertEx.DoesNotThrowAsync(async () =>
            {
                var ecs = await _client.Function.S2F31DateAndTimeSetRequest(DateTime.Now);
            });
        }
    }
}