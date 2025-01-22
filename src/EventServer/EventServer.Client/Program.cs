using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();
builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    builder.Configuration.Bind("keycloak", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "id_token toekn";
    options.UserOptions.NameClaim = "preferred_username";
    options.UserOptions.RoleClaim = "roles";
    options.UserOptions.ScopeClaim = "scopes";
});

//builder.Services.AddCascadingAuthenticationState();
//builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddHttpClient();
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

