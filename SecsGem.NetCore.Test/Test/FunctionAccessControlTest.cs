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
                var ecs = await _client.Function.EquipmentConstantDefinitionGet();
            });

            await _client.Function.ControlOnline();
            await _server.Function.GoOnlineLocal();

            await AssertEx.DoesNotThrowAsync(async () =>
            {
                var ecs = await _client.Function.EquipmentConstantDefinitionGet();
            });

            await AssertEx.ThrowAsync<SecsGemTransactionException>(async () =>
            {
                var ecs = await _client.Function.ServerTimeSet(DateTime.Now);
            });

            await _server.Function.GoOnlineRemote();

            await AssertEx.DoesNotThrowAsync(async () =>
            {
                var ecs = await _client.Function.ServerTimeSet(DateTime.Now);
            });
        }
    }
}