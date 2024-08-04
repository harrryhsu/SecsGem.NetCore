![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/harrryhsu/SecsGem.NetCore/docker-image.yml)  [![NuGet Downloads](https://img.shields.io/nuget/dt/SecsGem.NetCore)](https://www.nuget.org/packages/SecsGem.NetCore)

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
    builder.Services.AddSecsGem(option =>
    {
        option.Target = new IPEndPoint(IPAddress.Any, 5000);
    });
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

        /// <summary>
        /// On SecsGem server start for initializing features and equipment data
        /// </summary>
        /// <param name="evt"></param>
        public async Task Init(SecsGemInitEvent evt)
        {
            _kernel.Feature.Device.Model = "Test Model";
            _kernel.Feature.Device.Revision = "Test Revision";
            _kernel.Feature.StatusVariables.Add(new StatusVariable { Id = 1, Name = "Test SV 1", Unit = "Test Unit 1" });
            _kernel.Feature.StatusVariables.Add(new StatusVariable { Id = 2, Name = "Test SV 2", Unit = "Test Unit 2" });
            _kernel.Feature.EquipmentConstants.Add(new EquipmentConstant { Id = 1, Name = "Test EC 1", Unit = "Test Unit 1", Min = 0, Max = 1000, Default = 0 });
            _kernel.Feature.DataVariables.Add(new DataVariable { Id = "1", Description = "Test DV 1", Unit = "Test Unit 1" });
            _kernel.Feature.Commands.Add(new Command { Name = "START", Description = "Start Production", });
            _kernel.Feature.Alarms.Add(new Alarm { Id = 1, Description = "Test Alarm", Enabled = false });
            _kernel.Feature.CollectionEvents.Add(new CollectionEvent { Id = 1, Name = "Test CE 1", Enabled = true });
            _kernel.Feature.Terminals.Add(new Terminal { Id = 1, Name = "Main Display" });
        }

        /// <summary>
        /// On SecsGem server stop
        /// </summary>
        /// <param name="evt"></param>
        public async Task Stop(SecsGemStopEvent evt)
        {
        }

        /// <summary>
        /// Request to populate Status Variables in evt.Params
        /// Triggered by S1F3
        /// </summary>
        /// <param name="evt"></param>
        public async Task GetStatusVariable(SecsGemGetStatusVariableEvent evt)
        {
        }

        /// <summary>
        /// Request to populate Data Variables in evt.Params
        /// Triggered by S6F15 or equipment initiated a SendCollectionEvent
        /// </summary>
        /// <param name="evt"></param>
        public async Task GetDataVariable(SecsGemGetDataVariableEvent evt)
        {
        }

        /// <summary>
        /// Request to populate Equipment Constants in evt.Params, Triggered by S2F13
        /// </summary>
        /// <param name="evt"></param>
        public async Task GetEquipmentConstant(SecsGemGetEquipmentConstantEvent evt)
        {
        }

        /// <summary>
        /// Request to update Equipment Constants, Triggered by S2F15
        /// </summary>
        /// <param name="evt"></param>
        public async Task SetEquipmentConstant(SecsGemSetEquipmentConstantEvent evt)
        {
        }

        /// <summary>
        /// Command execute, Triggered by S2F41
        /// </summary>
        /// <param name="evt"></param>
        public async Task CommandExecute(SecsGemCommandExecuteEvent evt)
        {
            Console.WriteLine($"Command Execute: {evt.Cmd.Name}");
            evt.Return = SECS_RESPONSE.HCACK.Ok;
        }

        /// <summary>
        /// Request for displaying message on terminal
        /// Triggered by S10F3/S10F5/S10F9
        /// </summary>
        /// <param name="evt"></param>
        public async Task TerminalDisplay(SecsGemTerminalDisplayEvent evt)
        {
            evt.Return = SECS_RESPONSE.ACKC10.Accepted;
        }

        /// <summary>
        /// Triggered whenever there is a change in the kernel state,
        /// set evt.Accept to false will cancel the transition,
        /// if evt.Force is true, the evt.Accept has no effect, the message become notification only
        /// </summary>
        /// <param name="evt"></param>
        public async Task StateChange(SecsGemServerStateChangeEvent evt)
        {
        }

        /// <summary>
        /// Notification for any unhandled message
        /// </summary>
        /// <param name="evt"></param>
        public async Task OrphanMessage(SecsGemServerOrphanMessageEvent evt)
        {
        }

        /// <summary>
        /// Error event for any SecsGem or HSMS exception
        /// </summary>
        /// <param name="evt"></param>
        public async Task Error(SecsGemErrorEvent evt)
        {
            Console.WriteLine($"SecsGem Error: {evt.Message} {evt.Exception}");
        }

        /// <summary>
        /// Set time request, Triggered by S2F31
        /// </summary>
        /// <param name="evt"></param>
        public async Task SetTime(SecsGemSetTimeEvent evt)
        {
        }

        /// <summary>
        /// Triggered whenever there is a data change,
        /// this event is used to notify for saving the data
        /// </summary>
        /// <param name="evt"></param>
        public async Task DataChange(SecsGemDataChangeEvent evt)
        {
        }
    }

SecsGem.Function provides active methods that you can use to send event to host, the method will throw if there is a tcp error or invalid state.

All function's spec and naming are from [Hume software](http://www.hume.com/secs/)

    /// <summary>
    /// S1F13 Establish communication to transition into control offline state
    /// </summary>
    /// <returns>If state transition succeeded</returns>
    public async Task<bool> S1F13EstablishCommunicationRequest(CancellationToken ct = default);

    /// <summary>
    /// S5F1 Trigger alarm, message is only sent if kernel state is readable and alarm is enabled
    /// </summary>
    Task<bool> S5F1AlarmReportSend(uint id, CancellationToken ct = default);

    /// <summary>
    /// S10F1 Send single line terminal display
    /// </summary>
    /// <returns>Terminal display result</returns>
    public async Task<SECS_RESPONSE.ACKC10> S10F1TerminalRequest(byte id, string text, CancellationToken ct = default);

    /// <summary>
    /// S6F11 Send collection event, SecsGemGetDataVariableEvent is triggered to populate the collection event data variables
    /// </summary>
    public async Task S6F11EventReportSend(uint id, CancellationToken ct = default);

    /// <summary>
    /// S5F9 Notify host of an equipment exception
    /// </summary>
    Task<bool> S5F9ExceptionPostNotify(string id, Exception ex, string recoveryMessage, DateTime timestamp = default, CancellationToken ct = default);
    Task<bool> S5F9ExceptionPostNotify(string id, string type, string message, string recoveryMessage, DateTime timestamp = default, CancellationToken ct = default);

    /// <summary>
    /// Disconnect immediately
    /// </summary>
    public async Task Separate(CancellationToken ct = default);

    /// <summary>
    /// Transition the local state machine to online remote
    /// </summary>
    /// <returns>If state transition succeeded</returns>
    public async Task<bool> GoOnlineRemote();

    /// <summary>
    /// Transition the local state machine to online local
    /// </summary>
    /// <returns>If state transition succeeded</returns>
    public async Task<bool> GoOnlineLocal();

    /// <summary>
    /// Send user provided HSMS message to host
    /// </summary>
    /// <returns>HSMS message response</returns>
    public async Task<HsmsMessage> Send(HsmsMessage message, CancellationToken ct = default);

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

    var ecs = await client.Function.S2F29EquipmentConstantNamelistRequest();
