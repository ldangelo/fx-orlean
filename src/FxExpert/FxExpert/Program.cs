using EventServer.Aggregates.Partners;
using EventServer.Aggregates.Users;
using EventServer.Client.Services;
using FxExpert.Components;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor.Services;
using Nextended.Core.Extensions;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder
            .Services.AddHttpClient(
                "WeatherAPI",
                c => c.BaseAddress = new Uri("https://localhost:7020/api/weather/")
            )
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

        //
        // Add MudBlazor services
        builder.Services.AddMudServices();

        // add wolverine/marten
        builder.Services.AddWolverine(opts => { opts.Policies.AutoApplyTransactions(); });
        builder
            .Services.AddMarten(opts =>
            {
                opts.Connection("");
                opts.Projections.Add<PartnerProjection>(ProjectionLifecycle.Inline);
                opts.Projections.Add<UserProjection>(ProjectionLifecycle.Inline);
            })
            .IntegrateWithWolverine()
            .AddAsyncDaemon(DaemonMode.HotCold);

        // BFF backend setup
        builder.Services.AddBff();

        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookie";
                options.DefaultChallengeScheme = "oidc";
                options.DefaultSignOutScheme = "oidc";
            })
            .AddCookie(
                "cookie",
                options =>
                {
                    options.Cookie.Name = "__Host-blazor";
                    options.Cookie.SameSite = SameSiteMode.Strict;
                }
            )
            .AddOpenIdConnect(
                "oidc",
                options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Authority = Environment.GetEnvironmentVariable("KEYCLOAK_URL");

                    options.ClientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ID");
                    options.ClientSecret = Environment.GetEnvironmentVariable(
                        "KEYCLOAK_CLIENT_SECRET"
                    );

                    if (options.Authority.IsNullOrEmpty())
                        throw new Exception("KEYCLOAK_URL is not set");

                    if (options.ClientId.IsNullOrEmpty())
                        throw new Exception("KEYCLOAK_CLIENT_ID is not set");

                    if (options.ClientSecret.IsNullOrEmpty())
                        throw new Exception("KEYCLOAK_CLIENT_SECRET is not set");

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
                }
            );

        // In Startup.cs (or wherever you configure your asp.net services)

        builder.Services.AddAntiforgery(c => { c.SuppressXFrameOptionsHeader = true; });

        // Add Weather Service
        builder.Services.AddScoped<IPartnerService, PartnerService>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddWolverineHttp();

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
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseWebAssemblyDebugging();

            //
        }
        else
        {
            app.UseExceptionHandler("/Error", true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

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
        //        app.MapGroup("/authentication").MapLoginAndLogout();
        app.Run();
    }
}