using SecsGem.NetCore.Buffer;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server.Handler.Message
{
    [SecsGemMessage(HsmsMessageType.DataMessage)]
    public class SecsGemClientHandlerDataMessage : SecsGemClientStreamHandler
    {
        protected async Task Error(HsmsErrorCode code)
        {
            var writer = new ByteBufferWriter();
            Context.Message.Header.Write(writer);
            var mhead = writer.ToMemory().ToArray().Skip(4).Take(10).ToArray();

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Stream(9)
                    .Func((byte)code)
                    .Item(new BinDataItem(mhead))
                    .Build()
            );
        }

        public override async Task Execute()
        {
            var shortName = Context.Message.ToShortName();
            var handler = Context.Handlers.FirstOrDefault(x => x.IsMatch(Context.Message, false)) as SecsGemStreamHandlerCache;

            if (Context.Message.Header.S == 9)
            {
                await Context.Kernel.Emit(new SecsGemErrorEvent
                {
                    Message = $"Reply error {shortName}",
                });
            }
            else if (Context.Message.Header.S == 0)
            {
                await Context.Kernel.Emit(new SecsGemErrorEvent
                {
                    Message = $"Reply abort {shortName}",
                });
            }
            else if (handler != null)
            {
                try
                {
                    var executor = Activator.CreateInstance(handler.HandlerType) as SecsGemClientStreamHandler;
                    executor.Context = Context;
                    await executor.Execute();
                }
                catch (Exception ex)
                {
                    await Context.Kernel.Emit(new SecsGemErrorEvent
                    {
                        Message = $"Error while handling Contextuest {shortName}: {ex}",
                    });

                    await Error(HsmsErrorCode.IllegalData);
                }
            }
            else
            {
                await Error(HsmsErrorCode.UnrecognizedFunction);
            }
        }
    }
}