using SecsGem.NetCore.Buffer;
using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Hsms;
using System.Reflection;

namespace SecsGem.NetCore.Handler.Server
{
    public enum ErrorCode
    {
        UnrecognizedDeviceId = 1,

        UnrecognizedStream = 3,

        UnrecognizedFunction = 5,

        IllegalData = 7,

        TransactionTimeout = 9,

        DataTooLong = 11,

        ConversionTimeout = 13,
    }

    public class SecsGemServerRequestHandler
    {
        private readonly Dictionary<string, Func<SecsGemServerRequestContext, Task>> _handlers = new();

        public SecsGemServerRequestHandler()
        {
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsAssignableTo(typeof(ISecsGemServerStreamHandler)) && !x.IsAbstract && !x.IsInterface)
                .Select(x => Activator.CreateInstance(x) as ISecsGemServerStreamHandler)
                .ToList()
                .ForEach(x =>
                {
                    foreach (var method in x.GetType().GetMethods().Where(x => x.Name.StartsWith("S")))
                    {
                        _handlers.Add(method.Name, async (request) =>
                        {
                            await (Task)method.Invoke(x, new object[] { request });
                        });
                    }
                });
        }

        public void Register(int stream, int function, Func<SecsGemServerRequestContext, Task> handler)
        {
            _handlers[$"S{stream}F{function}"] = handler;
        }

        internal async Task Handle(SecsGemTcpClient sender, TcpConnection con, SecsGemServer kernel, HsmsMessage message)
        {
            var req = new SecsGemServerRequestContext
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
                    if (!kernel.State.IsMoreThan(GemServerStateModel.ControlOnlineLocal))
                    {
                        if (shortName != "S1F13" && shortName != "S1F17" && shortName != "S1F1")
                        {
                            await req.ReplyAsync(
                                HsmsMessage.Builder
                                    .Reply(message)
                                    .Func(0)
                                    .Build()
                            );
                            return;
                        }
                    }

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
                            await kernel.Emit(new SecsGemErrorEvent
                            {
                                Message = $"Error while handling request {shortName}: {ex}",
                            });

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
                        var writer = new ByteBufferWriter();
                        message.Header.Write(writer);
                        var mhead = writer.ToMemory().ToArray().Skip(4).Take(10).ToArray();

                        await req.ReplyAsync(
                            HsmsMessage.Builder
                                .Reply(message)
                                .Stream(9)
                                .Func((byte)ErrorCode.UnrecognizedFunction)
                                .Item(new BinDataItem(mhead))
                                .Build()
                        );

                        await kernel.Emit(new SecsGemOrphanMessageEvent
                        {
                            Params = message,
                        });
                    }
                    break;

                case HsmsMessageType.SelectReq:
                    if (req.Kernel.State.Current == GemServerStateModel.Connected)
                    {
                        await req.ReplyAsync(
                            HsmsMessage.Builder
                                .Reply(message)
                                .Func(0)
                                .Type(HsmsMessageType.SelectRsp)
                                .Build()
                        );
                        await req.Kernel.State.TriggerAsync(GemServerStateTrigger.Select);
                    }
                    else
                    {
                        await req.ReplyAsync(
                            HsmsMessage.Builder
                                .Reply(message)
                                .Func(1)
                                .Type(HsmsMessageType.SelectRsp)
                                .Build()
                        );
                    }
                    break;

                case HsmsMessageType.SeparateReq:
                    await req.Kernel.State.TriggerAsync(GemServerStateTrigger.Disconnect);
                    break;

                case HsmsMessageType.DeselectReq:
                    await req.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(message)
                            .Func(0)
                            .Type(HsmsMessageType.DeselectRsp)
                            .Build()
                    );
                    await req.Kernel.State.TriggerAsync(GemServerStateTrigger.Deselect);
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