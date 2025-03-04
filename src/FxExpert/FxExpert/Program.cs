using FxExpert.Components;
using MudBlazor.Services;
using Nextended.Core.Extensions;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //
        // Add MudBlazor services
        builder.Services.AddMudServices();

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