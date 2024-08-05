using SecsGem.NetCore.Error;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Handler.Server;
using SecsGem.NetCore.Hsms;
using SecsGem.NetCore.Test.Helper;

namespace SecsGem.NetCore.Test.Test
{
    public class InvalidMessageHandlingTest : SecsGemTestBase
    {
        [Test]
        public async Task Handler_Does_Not_Reply()
        {
            _server.Handler.Register<SecsGemServerHandlerTest1>();

            var exception = await AssertEx.ThrowAsync<SecsGemTransactionException>(async () =>
            {
                await _client.Function.S10F3TerminalDisplaySingle(1, "Test");
            });

            Assert.That((HsmsErrorCode)exception.HsmsMessage.Header.F, Is.EqualTo(HsmsErrorCode.IllegalData));
        }

        [Test]
        public async Task Handler_Throw()
        {
            _server.Handler.Register<SecsGemServerHandlerTest2>();

            var exception = await AssertEx.ThrowAsync<SecsGemTransactionException>(async () =>
            {
                await _client.Function.S10F3TerminalDisplaySingle(1, "Test");
            });

            Assert.That((HsmsErrorCode)exception.HsmsMessage.Header.F, Is.EqualTo(HsmsErrorCode.IllegalData));
        }

        [Test]
        public async Task Orphan_Message()
        {
            _server.OnEvent += async (sender, evt) =>
            {
                if (evt is SecsGemServerOrphanMessageEvent nevt)
                {
                    var context = nevt.Context;
                    await context.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(context.Message)
                            .Item(new BinDataItem((byte)(context.Message.Root.GetBin() + 1)))
                            .Build()
                    );
                }
            };

            var message = await _client.Function.Send(HsmsMessage.Builder.Stream(11).Func(1).Item(new BinDataItem(1)).Build());

            byte ret = 0;
            Assert.DoesNotThrow(() =>
            {
                ret = message.Root.GetBin();
            });

            Assert.That(ret, Is.EqualTo(2));
        }
    }

    [SecsGemStream(10, 3)]
    [SecsGemFunctionType(SecsGemFunctionType.Communication)]
    public class SecsGemServerHandlerTest1 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
        }
    }

    [SecsGemStream(10, 3)]
    [SecsGemFunctionType(SecsGemFunctionType.Communication)]
    public class SecsGemServerHandlerTest2 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            throw new Exception("Test");
        }
    }
}