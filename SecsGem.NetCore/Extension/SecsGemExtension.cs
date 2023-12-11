using SecsGem.NetCore.Event;

namespace SecsGem.NetCore.Extension
{
    public static class SecsGemExtension
    {
        public static void AddSecsGem(this IServiceCollection collection, Action<SecsGemOption> configure = null)
        {
            var option = new SecsGemOption();
            configure?.Invoke(option);
            collection.AddSingleton<SecsGemKernel, SecsGemKernel>(p => new(option));
            collection.AddHostedService(p => p.GetRequiredService<SecsGemKernel>());
        }

        public static void UseSecsGem<THandler>(this WebApplication app) where THandler : ISecsGemEventHandler
        {
            var kernel = app.Services.GetRequiredService<SecsGemKernel>();
            var handler = app.Services.GetRequiredService<THandler>() as ISecsGemEventHandler;

            kernel.OnEvent += async (sender, e) =>
            {
                using var scope = app.Services.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>() as ISecsGemEventHandler;
                switch (e.Event)
                {
                    case SecsGemEventType.Init:
                        await handler.Init(e as SecsGemInitEvent);
                        break;

                    case SecsGemEventType.Stop:
                        await handler.Stop(e as SecsGemStopEvent);
                        break;

                    case SecsGemEventType.GetStatusVariable:
                        await handler.GetStatusVariable(e as SecsGemGetStatusVariableEvent);
                        break;

                    case SecsGemEventType.GetDataVariable:
                        await handler.GetDataVariable(e as SecsGemGetDataVariableEvent);
                        break;

                    case SecsGemEventType.GetEquipmentConstant:
                        await handler.GetEquipmentConstant(e as SecsGemGetEquipmentConstantEvent);
                        break;

                    case SecsGemEventType.SetEquipmentConstant:
                        await handler.SetEquipmentConstant(e as SecsGemSetEquipmentConstantEvent);
                        break;

                    case SecsGemEventType.CommandExecute:
                        await handler.CommandExecute(e as SecsGemCommandExecuteEvent);
                        break;

                    case SecsGemEventType.TerminalDisplay:
                        await handler.TerminalDisplay(e as SecsGemTerminalDisplayEvent);
                        break;

                    case SecsGemEventType.ControlStateChange:
                        await handler.ControlStateChange(e as SecsGemControlStateChangeEvent);
                        break;

                    case SecsGemEventType.CommunicationStateChange:
                        await handler.CommunicationStateChange(e as SecsGemCommunicationStateChangeEvent);
                        break;

                    case SecsGemEventType.OrphanMessage:
                        await handler.OrphanMessage(e as SecsGemOrphanMessageEvent);
                        break;

                    case SecsGemEventType.Error:
                        await handler.Error(e as SecsGemErrorEvent);
                        break;
                }
            };
        }
    }
}