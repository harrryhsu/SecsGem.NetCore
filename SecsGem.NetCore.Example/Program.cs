using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SecsGem.NetCore;
using SecsGem.NetCore.Example;
using SecsGem.NetCore.Extension;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.State.Client;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<Equipment>();
builder.Services.AddSingleton<CustomSecsGemHandler>();
builder.Services.AddSecsGem(option =>
{
    option.Target = new IPEndPoint(IPAddress.Loopback, 5000);
});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8000);
});
var app = builder.Build();
app.UseSecsGem<CustomSecsGemHandler>();
await app.StartAsync();

var kernal = app.Services.GetService<SecsGemServer>();
var equipment = app.Services.GetService<Equipment>();

var client = new SecsGemClient(new SecsGemOption
{
    Target = new IPEndPoint(IPAddress.Loopback, 5000),
    ActiveConnect = true,
});
await client.ConnectAsync();
await client.State.WaitForState(GemClientStateModel.ControlOffLine);
await client.Function.S1F17RequestOnline();
await client.State.WaitForState(GemClientStateModel.ControlOnline);

await client.Function.S2F15NewEquipmentConstantSend(new List<EquipmentConstant>
{
    new() {
        Id = 1,
        Value = 20
    }
});
Console.WriteLine($"Equipment.Arg: {equipment.Arg}");
//Equipment.Arg: 20

await client.Function.S2F41HostCommandSend("START", new());
var sv = await client.Function.S1F3SelectedEquipmentStatusRequest(new uint[] { 1 });
Console.WriteLine($"Equipment.Running: {sv.FirstOrDefault().Value}");
//Equipment.Running: true

await client.Function.S2F41HostCommandSend("STOP", new());
sv = await client.Function.S1F3SelectedEquipmentStatusRequest(new uint[] { 1 });
Console.WriteLine($"Equipment.Running: {sv.FirstOrDefault().Value}");
//Equipment.Running: false

await client.Function.S10F3TerminalDisplaySingle(1, "Terminal Display");
Console.WriteLine($"Equipment.Terminal: {equipment.Display}");
//Equipment.Terminal: Terminal Display