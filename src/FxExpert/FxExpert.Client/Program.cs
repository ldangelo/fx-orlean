using Blazor6.Client.BFF;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, BffAuthenticationStateProvider>();
// HTTP client configuration
builder.Services.AddTransient<AntiforgeryHandler>();

builder.Services.AddHttpClient("backend", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AntiforgeryHandler>();
builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("backend"));

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
        .AddHttpMessageHandler<AntiforgeryHandler>();

    services.AddScoped(sp =>
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientName)
    );
}

public class AntiforgeryHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Add("X-CSRF", "1");
        return base.SendAsync(request, cancellationToken);
    }
}