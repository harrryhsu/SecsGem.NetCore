![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/harrryhsu/SecsGem.NetCore/docker-image.yml) 

# SecsGem.NetCore

Just a naive implementation of HSMS protocol and certain SecsGem function.

Only the below feature is supported and not all function of the below feature is implemented.

    Alarms
    CollectionEvents
    CollectionReports
    Commands
    DataVariables
    EquipmentConstants
    StatusVariables
    ProcessPrograms





## SecsGemServer Usage

The library was designed to be used with Asp.NetCore Web Application.

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSecsGem();
    var app = builder.Build();
    app.UseSecsGem<CustomSecsGemHandler>();
    app.Run();

However it is also possible to access it by directly creating an instance, you will have to handle event category manually.

    var secsgem = new SecsGemServer(new SecsGemOption { Target = new(IPAddress.Any, 5000) });
    secsgem.OnEvent += async (sender, evt) =>
    {
    };
    await secsgem.StartAsync();


The UseSecsGem expect to find a service that implemented ISecsGemServerEventHandler from service provider, and create a new scope for each event. 

The event handler will define all interation the equipment needed to operate SecsGem protocol.

    public class CustomSecsGemHandler : ISecsGemServerEventHandler
    {
        private readonly SecsGemServer _kernel;

        public CustomSecsGemHandler(SecsGemServer kernel)
        {
            _kernel = kernel;
        }

        // On SecsGem server start for initializing features and equipment data
        public async Task Init(SecsGemInitEvent evt)
        {
            _kernel.Device.Model = "Test Model";
            _kernel.Device.Revision = "Test Revision";
            _kernel.Feature.StatusVariables.Add(new StatusVariable { Id = 1, Name = "Test SV 1", Unit = "Test Unit 1" });
            _kernel.Feature.StatusVariables.Add(new StatusVariable { Id = 2, Name = "Test SV 2", Unit = "Test Unit 2" });
            _kernel.Feature.EquipmentConstants.Add(new EquipmentConstant { Id = 1, Name = "Test EC 1", Unit = "Test Unit 1", Min = 0, Max = 1000, Default = 0 });
            _kernel.Feature.DataVariables.Add(new DataVariable { Id = "1", Description = "Test DV 1", Unit = "Test Unit 1" });
            _kernel.Feature.Commands.Add(new Command { Name = "START", Description = "Start Production", });
            _kernel.Feature.Alarms.Add(new Alarm { Id = 1, Description = "Test Alarm", Enabled = false });
            _kernel.Feature.CollectionEvents.Add(new CollectionEvent { Id = 1, Name = "Test CE 1", Enabled = true });

            return Task.CompletedTask;
        }

        // On SecsGem server stop
        public async Task Stop(SecsGemStopEvent evt)
        {
        }

        // Command execute
        // Triggered by S2F41
        public async Task CommandExecute(SecsGemCommandExecuteEvent evt)
        {
            Console.WriteLine($"Command Execute: {evt.Cmd.Name}");
            evt.Return = CommandExecuteResult.Okay;
        }

        // Communication state change alert that nofitied about new connection or connection dropping
        public async Task CommunicationStateChange(SecsGemCommunicationStateChangeEvent evt)
        {
            Console.WriteLine($"Communication State Change: {evt.NewState}");
            evt.Return = true;
        }

        // Error event for any SecsGem or HSMS exception
        public async Task Error(SecsGemErrorEvent evt)
        {
            Console.WriteLine($"SecsGem Error: {evt.Message} {evt.Exception}");
        }

        // Request to populate Data Variables in evt.Params
        // Triggered by S6F15 or equipment initiated a SendCollectionEvent
        public async Task GetDataVariable(SecsGemGetDataVariableEvent evt)
        {
        }

        // Request to populate Equipment Constants in evt.Params
        // Triggered by S2F13
        public async Task GetEquipmentConstant(SecsGemGetEquipmentConstantEvent evt)
        {
        }

        // Request to populate Status Variables in evt.Params
        // Triggered by S1F3
        public async Task GetStatusVariable(SecsGemGetStatusVariableEvent evt)
        {
        }

        // Notification for any unhandled host initiated transaction
        public async Task OrphanMessage(SecsGemOrphanMessageEvent evt)
        {
        }

        // Request to update Equipment Constants
        // Triggered by S2F15
        public async Task SetEquipmentConstant(SecsGemSetEquipmentConstantEvent evt)
        {
        }

        // Request for displaying message on terminal
        // Triggered by S10F3/S10F5/S10F9
        public async Task TerminalDisplay(SecsGemTerminalDisplayEvent evt)
        {
            evt.Return = true;
        }
    }


SecsGem interface only provides four active methods that you can use to send event to host, you can find them in SecsGemServer.Function

SecsGemServer will only send the data if server is online.

    // S5F1
    Task<bool> TriggerAlarm(uint id, CancellationToken ct = default);

    // S10F1
    Task<bool> SendHostDisplay(byte id, string text, CancellationToken ct = default);
    
    // S6F11
    // GetDataVariable event will be triggered to fill data variables
    Task<bool> SendCollectionEvent(uint id, CancellationToken ct = default);

    // S5F9
    Task<bool> NotifyException(string id, Exception ex, string recoveryMessage, DateTime timestamp = default, CancellationToken ct = default);

    Task<bool> NotifyException(string id, string type, string message, string recoveryMessage, DateTime timestamp = default, CancellationToken ct = default);


## SecsGemClient Usage

Client interface is developed for testing purpose, but the actual usage is also supported, all client function is in SecsGemClient.Function

    await using var client = new SecsGemClient(new SecsGemOption
    {
        // Network target
        Target = new IPEndPoint(IPAddress.Parse(ip), port),
        // Tcp receive buffer size
        TcpBufferSize = 4096,
        // Enable debug log through Logger 
        Debug = false,
        // If client should initiate S1F13
        // Can also be initialted by calling SecsGemClient.Function.CommunicationEstablish 
        ActiveConnect = true, 
        // Debug logger
        Logger = (msg) => Console.WriteLine(msg),
        // Default message timeout
        T3 = 3000,
        // Not selected timeout, only has effect for SecsGemServer
        T7 = 3000,
        // Byte receive timeout
        T8 = 500,
    });

     _client.OnEvent += async (sender, evt) =>
    {
        if (evt is SecsGemAlarmEvent nevt)
        {
            // Alarm nevt.Alarm triggered
        }
    };

    var ecs = await client.Function.EquipmentConstantValueGet();

