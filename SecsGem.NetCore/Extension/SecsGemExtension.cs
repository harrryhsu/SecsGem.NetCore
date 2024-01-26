using SecsGem.NetCore.Event.Server;

namespace SecsGem.NetCore.Extension
{
    public static class SecsGemExtension
    {
        public static void AddSecsGem(this IServiceCollection collection, Action<SecsGemOption> configure = null)
        {
            var option = new SecsGemOption();
            configure?.Invoke(option);
            collection.AddSingleton<SecsGemServer, SecsGemServer>(p => new(option));
            collection.AddHostedService(p => p.GetRequiredService<SecsGemServer>());
        }

        public static void UseSecsGem<THandler>(this WebApplication app) where THandler : ISecsGemServerEventHandler
        {
            var kernel = app.Services.GetRequiredService<SecsGemServer>();
            var handler = app.Services.GetRequiredService<THandler>() as ISecsGemServerEventHandler;

            kernel.OnEvent += async (sender, e) =>
            {
                using var scope = app.Services.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>() as ISecsGemServerEventHandler;
                var executer = new SecsGemServerEventHandlerExecuter(handler);
                await executer.Execute(e);
            };
        }
    }
}