using SecsGem.NetCore.Buffer;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server.Handler.Message
{
    [SecsGemMessage(HsmsMessageType.DataMessage)]
    public class SecsGemServerHandlerDataMessage : SecsGemServerStreamHandler
    {
        protected async Task Error(HsmsErrorCode code)
        {
            var writer = new ByteBufferWriter();
            Context.Message.Header.Write(writer);
            var mhead = writer.ToMemory().ToArray().Skip(4).Take(10).ToArray();

            await Context.Kernel.Emit(new SecsGemServerOrphanMessageEvent
            {
                Params = Context,
            });

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
                if ((int)Context.Kernel.State.Current < (int)handler.FunctionType)
                {
                    await Context.ReplyAsync(
                           HsmsMessage.Builder
                               .Reply(Context.Message)
                               .Func(0)
                               .Build()
                       );
                    return;
                }

                try
                {
                    var executor = Activator.CreateInstance(handler.HandlerType) as SecsGemServerStreamHandler;
                    executor.Context = Context;
                    await executor.Execute();
                    if (handler.FunctionType == SecsGemFunctionType.Operation)
                    {
                        await Context.Kernel.Emit(new SecsGemDataChangeEvent());
                    }
                    if (!Context.HasReplied)
                    {
                        await Error(HsmsErrorCode.IllegalData);
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

                    if (!Context.HasReplied)
                    {
                        await Error(HsmsErrorCode.IllegalData);
                    }
                }
            }
            else
            {
                await Context.Kernel.Emit(new SecsGemServerOrphanMessageEvent
                {
                    Params = Context,
                });

                if (!Context.HasReplied)
                {
                    await Error(HsmsErrorCode.UnrecognizedFunction);
                }
            }
        }
    }
}