using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

RegisterHttpClient(builder, builder.Services);

var app = builder.Build();
await app.RunAsync();

static void RegisterHttpClient(WebAssemblyHostBuilder builder, IServiceCollection services)
{
    var httpClientName = "Default";

    services
        .AddHttpClient(
            httpClientName,
            client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
        )
        .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

    services.AddScoped(sp =>
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientName)
    );
}