using SecsGem.NetCore.Feature.Client;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Helper;
using SecsGem.NetCore.Test.Helper;
using System.Net;

namespace SecsGem.NetCore.Test.Test
{
    public class FunctionAccessControlTest : SecsGemTestBase
    {
        [SetUp]
        public override async Task Setup()
        {
            var target = new IPEndPoint(IPAddress.Loopback, 8080);
            _client = new(new SecsGemOption
            {
                Debug = true,
                ActiveConnect = true,
                Target = target,
                Logger = (msg) => Console.WriteLine(DateTime.Now.ToString("ss:fff") + " Client " + msg),
            });

            _server = new(new SecsGemOption
            {
                Debug = true,
                Target = target,
                Logger = (msg) => Console.WriteLine(DateTime.Now.ToString("ss:fff") + " Server " + msg)
            });

            await _server.StartAsync();
            await _client.ConnectAsync();

            await TaskHelper.WaitFor(() => _client.State.IsExact(GemClientStateModel.ControlOffLine) && _server.State.IsExact(GemServerStateModel.ControlOffLine), 10, 100);
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