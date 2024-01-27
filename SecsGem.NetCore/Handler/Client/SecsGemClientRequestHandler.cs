using SecsGem.NetCore.Buffer;
using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Handler.Server;
using SecsGem.NetCore.Hsms;
using System.Reflection;

namespace SecsGem.NetCore.Handler.Client
{
    public class SecsGemClientRequestHandler
    {
        private readonly Dictionary<string, Func<SecsGemClientRequestContext, Task>> _handlers = new();

        public SecsGemClientRequestHandler()
        {
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsAssignableTo(typeof(ISecsGemClientStreamHandler)) && !x.IsAbstract && !x.IsInterface)
                .Select(x => Activator.CreateInstance(x) as ISecsGemClientStreamHandler)
                .ToList()
                .ForEach(x =>
                {
                    foreach (var method in x.GetType().GetMethods().Where(x => x.Name.StartsWith("S")))
                    {
                        _handlers.Add(method.Name, async (SecsGemClientRequestContext request) =>
                        {
                            await (Task)method.Invoke(x, new object[] { request });
                        });
                    }
                });
        }

        protected async Task UnrecognizedFunction(SecsGemClientRequestContext req)
        {
            var writer = new ByteBufferWriter();
            req.Message.Header.Write(writer);
            var mhead = writer.ToMemory().ToArray().Skip(4).Take(10).ToArray();

            await req.Kernel.Emit(new SecsGemOrphanMessageEvent
            {
                Params = req.Message,
            });

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Stream(9)
                    .Func((byte)ErrorCode.UnrecognizedFunction)
                    .Item(new BinDataItem(mhead))
                    .Build()
            );
        }

        public async Task Handle(SecsGemTcpClient sender, TcpConnection con, SecsGemClient kernel, HsmsMessage message)
        {
            var req = new SecsGemClientRequestContext
            {
                Client = sender,
                Connection = con,
                Message = message,
                Kernel = kernel,
            };

            switch (message.Header.SType)
            {
                case HsmsMessageType.DataMessage:
                    var shortName = message.ToShortName();
                    if (message.Header.S == 9)
                    {
                        await kernel.Emit(new SecsGemErrorEvent
                        {
                            Message = $"Reply error {shortName}",
                        });
                    }
                    else if (message.Header.S == 0)
                    {
                        await kernel.Emit(new SecsGemErrorEvent
                        {
                            Message = $"Reply abort {shortName}",
                        });
                    }
                    else if (_handlers.TryGetValue(message.ToShortName(), out var handler))
                    {
                        try
                        {
                            await handler.Invoke(req);
                        }
                        catch (Exception ex)
                        {
                            kernel._option.DebugLog($"Error while handling request {message.ToShortName()}: {ex}");

                            var writer = new ByteBufferWriter();
                            message.Header.Write(writer);
                            var mhead = writer.ToMemory().ToArray().Skip(4).Take(10).ToArray();

                            await req.ReplyAsync(
                                HsmsMessage.Builder
                                    .Reply(message)
                                    .Stream(9)
                                    .Func((byte)ErrorCode.IllegalData)
                                    .Item(new BinDataItem(mhead))
                                    .Build()
                            );
                        }
                    }
                    else
                    {
                        await UnrecognizedFunction(req);
                    }
                    break;

                case HsmsMessageType.SelectReq:
                    await UnrecognizedFunction(req);

                    break;

                case HsmsMessageType.SeparateReq:
                    await req.Kernel.Disconnect();
                    break;

                case HsmsMessageType.DeselectReq:
                    await UnrecognizedFunction(req);
                    break;

                case HsmsMessageType.LinkTestReq:
                    await req.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(message)
                            .Type(HsmsMessageType.LinkTestRsp)
                            .Build()
                    );
                    break;

                default:
                    break;
            }
        }
    }
}