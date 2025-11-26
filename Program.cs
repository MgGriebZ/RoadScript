using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RoadScript;
using RoadScript.Models;
using RoadScript.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register RoadScript services
builder.Services.AddSingleton<SelectionState>();
builder.Services.AddSingleton<EditorInteropService>();

await builder.Build().RunAsync();
