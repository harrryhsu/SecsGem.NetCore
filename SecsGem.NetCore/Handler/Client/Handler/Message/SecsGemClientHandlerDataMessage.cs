using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server.Handler.Message
{
    [SecsGemMessage(HsmsMessageType.DataMessage)]
    public class SecsGemClientHandlerDataMessage : SecsGemClientStreamHandler
    {
        public override async Task Execute()
        {
            var handler = Context.Handlers.FirstOrDefault(x => x.IsMatch(Context.Message.Header.S, Context.Message.Header.F)) as SecsGemStreamHandlerCache;

            if (Context.Message.Header.S == 9)
            {
                await Context.Kernel.Emit(new SecsGemErrorEvent
                {
                    Message = $"Reply error {Context.Message}",
                });
            }
            else if (Context.Message.Header.S == 0)
            {
                await Context.Kernel.Emit(new SecsGemErrorEvent
                {
                    Message = $"Reply abort {Context.Message}",
                });
            }
            else if (handler != null)
            {
                try
                {
                    var executor = Activator.CreateInstance(handler.HandlerType) as SecsGemClientStreamHandler;
                    executor.Context = Context;
                    await executor.Execute();

                    if (!Context.HasReplied)
                    {
                        await Context.Error(HsmsErrorCode.IllegalData);
                        await Context.Kernel.Emit(new SecsGemErrorEvent
                        {
                            Message = $"SecsGemStreamHandler ${handler.GetType().AssemblyQualifiedName} does not reply to the message"
                        });
                    }
                }
                catch (Exception ex)
                {
                    await Context.Kernel.Emit(new SecsGemErrorEvent
                    {
                        Message = $"Error while handling Contextuest {Context.Message}: {ex}",
                    });

                    await Context.Error(HsmsErrorCode.IllegalData);
                }
            }
            else
            {
                await Context.Kernel.Emit(new SecsGemClientOrphanMessageEvent
                {
                    Context = Context,
                });

                if (!Context.HasReplied)
                {
                    await Context.Error(HsmsErrorCode.UnrecognizedFunction);
                }
            }
        }
    }
}