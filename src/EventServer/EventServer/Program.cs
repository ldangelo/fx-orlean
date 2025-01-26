using FluentAssertions;
using EventServer;
using EventServer.Client.Services;
using EventServer.Components;
using EventServer.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MudBlazor.Services;
using static common.FxHostingExtension;
using _Imports = EventServer.Client._Imports;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpClient("WeatherAPI", c =>
            c.BaseAddress = new Uri("https://localhost:7020/api/weather/")
        ).AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

        //
        // Add MudBlazor services
        builder.Services.AddMudServices();

        // Add FastEndpoints
        builder.Services.AddFastEndpoints();

        //
        // connect too the orleans cluster
        builder.UseFx();

        // BFF backend setup
        builder.Services.AddBff();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "cookie";
        options.DefaultChallengeScheme = "oidc";
        options.DefaultSignOutScheme = "oidc";
    })
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "__Host-blazor";
        options.Cookie.SameSite = SameSiteMode.Strict;
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.RequireHttpsMetadata = false;
        options.Authority = Environment.GetEnvironmentVariable("KEYCLOAK_URL");

        options.ClientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ID");
        options.ClientSecret = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_SECRET");

        options.Authority.Should().NotBeNullOrEmpty("KEYCLOAK_URL is not set");
        options.ClientId.Should().NotBeNullOrEmpty("KEYCLOAK_CLIENT_ID is not set");
        options.ClientSecret.Should().NotBeNullOrEmpty("KEYCLOAK_CLIENT_SECRET is not set");
        options.ResponseType = "code";
        options.ResponseMode = "query";

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("api");
        options.Scope.Add("offline_access");

        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
    });


        // Add Weather Service
        builder.Services.AddScoped<IWeatherService, WeatherService>();
        builder.Services.AddScoped<IPartnerService, PartnerService>();

        // Add services to the container.
        builder
            .Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error", true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        // Use FastEndpoints
        app.UseFastEndpoints();

        app.UseRouting();
        app.UseHttpsRedirection();

        // BFF setup
        app.UseAuthentication();
        app.UseBff();
        app.UseAuthorization();
        app.MapBffManagementEndpoints();
        app.UseAntiforgery();


        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(_Imports).Assembly);
        app.MapGroup("/authentication").MapLoginAndLogout();
        app.Run();
    }
}
