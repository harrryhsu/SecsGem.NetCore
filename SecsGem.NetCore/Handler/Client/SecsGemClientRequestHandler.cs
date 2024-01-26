using SecsGem.NetCore.Buffer;
using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Hsms;
using System.Reflection;

namespace SecsGem.NetCore.Handler.Client
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
                    if (_handlers.TryGetValue(message.ToShortName(), out var handler))
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
                        var writer = new ByteBufferWriter();
                        message.Header.Write(writer);
                        var mhead = writer.ToMemory().ToArray().Skip(4).Take(10).ToArray();

                        await kernel.Emit(new SecsGemOrphanMessageEvent
                        {
                            Params = message,
                        });

                        await req.ReplyAsync(
                            HsmsMessage.Builder
                                .Reply(message)
                                .Stream(9)
                                .Func((byte)ErrorCode.UnrecognizedFunction)
                                .Item(new BinDataItem(mhead))
                                .Build()
                        );
                    }
                    break;

                case HsmsMessageType.SelectReq:
                    if (req.Kernel.Device.CommunicationState == CommunicationStateModel.CommunicationDisconnected)
                    {
                        await req.ReplyAsync(
                            HsmsMessage.Builder
                                .Reply(message)
                                .Func(0)
                                .Type(HsmsMessageType.SelectRsp)
                                .Build()
                        );
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
                    await req.Kernel.Disconnect();
                    break;

                case HsmsMessageType.DeselectReq:
                    await req.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(message)
                            .Func(0)
                            .Type(HsmsMessageType.DeselectRsp)
                            .Build()
                    );
                    await req.Kernel.Disconnect();
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