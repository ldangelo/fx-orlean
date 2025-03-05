using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();
builder.Services.AddTransient<AntiforgeryHandler>();


builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

var app = builder.Build();

static void RegisterHttpClient(WebAssemblyHostBuilder builder, IServiceCollection services)
{
    var httpClientName = "Default";

    services
        .AddHttpClient(
            httpClientName,
            client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
        )
        .AddHttpMessageHandler<AntiforgeryHandler>();

    services.AddSingleton(sp =>
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