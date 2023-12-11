using SecsGem.NetCore.Buffer;
using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Event;
using SecsGem.NetCore.Feature;
using SecsGem.NetCore.Hsms;
using System.Reflection;

namespace SecsGem.NetCore.Handler
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

    public class SecsGemRequestHandler
    {
        private readonly Dictionary<string, Func<SecsGemRequestContext, Task>> _handlers = new();

        public SecsGemRequestHandler()
        {
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsAssignableTo(typeof(ISecsGemStreamHandler)) && !x.IsAbstract && !x.IsInterface)
                .Select(x => Activator.CreateInstance(x) as ISecsGemStreamHandler)
                .ToList()
                .ForEach(x =>
                {
                    foreach (var method in x.GetType().GetMethods().Where(x => x.Name.StartsWith("S")))
                    {
                        _handlers.Add(method.Name, async (SecsGemRequestContext request) =>
                        {
                            await (Task)method.Invoke(x, new object[] { request });
                        });
                    }
                });
        }

        public async Task Handle(SecsGemTcpClient sender, TcpConnection con, SecsGemKernel kernel, HsmsMessage message)
        {
            var req = new SecsGemRequestContext
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
                        catch (HsmsException)
                        {
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