using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Hsms;
using System.Reflection;

namespace SecsGem.NetCore.Handler.Common
{
    public class SecsGemRequestHandler<TKernal>
    {
        protected readonly List<SecsGemHandlerCache> _handlers = new();

        public SecsGemRequestHandler()
        {
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsAssignableTo(typeof(SecsGemStreamHandler<TKernal>)) && !x.IsAbstract && !x.IsInterface)
                .ToList()
                .ForEach(x =>
                {
                    Register(x);
                });
        }

        public void Register<THandler>() where THandler : SecsGemStreamHandler<TKernal>
        {
            var type = typeof(THandler);
            Register(type);
        }

        public void Register(Type type)
        {
            if (!type.IsAssignableTo(typeof(SecsGemStreamHandler<TKernal>))) throw new SecsGemException($"[{type}] is not assignable to ISecsGemStreamHandler");
            var functionAttr = type.GetCustomAttribute<SecsGemFunctionTypeAttribute>();
            var messageAttr = type.GetCustomAttribute<SecsGemMessageAttribute>();
            var streamAttr = type.GetCustomAttribute<SecsGemStreamAttribute>();

            if (messageAttr != null)
            {
                _handlers.Add(new SecsGemRequestHandlerCache
                {
                    HandlerType = type,
                    MessageType = messageAttr.Type,
                });
            }
            else
            {
                if (functionAttr == null || streamAttr == null)
                {
                    throw new SecsGemException($"[{type}] Missing attribute");
                }

                _handlers.Add(new SecsGemStreamHandlerCache
                {
                    HandlerType = type,
                    FunctionType = functionAttr.Type,
                    Stream = streamAttr.Stream,
                    Function = streamAttr.Function,
                });
            }
        }

        internal async Task Handle(SecsGemTcpClient sender, TcpConnection con, TKernal kernel, HsmsMessage message)
        {
            var context = new SecsGemRequestContext<TKernal>
            {
                Handlers = _handlers,
                Client = sender,
                Connection = con,
                Message = message,
                Kernel = kernel,
            };

            var handler = _handlers.FirstOrDefault(x => x.IsMatch(context.Message, true));
            if (handler != null)
            {
                var executor = Activator.CreateInstance(handler.HandlerType) as SecsGemStreamHandler<TKernal>;
                executor.Context = context;
                await executor.Execute();
            }
            else
            {
            }
        }
    }
}