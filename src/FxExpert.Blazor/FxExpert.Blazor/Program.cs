using System.Security.Claims;
using System.Text.Json;
using FxExpert.Blazor.Components;
using FxExpert.Blazor.Components.Account;
using FxExpert.Blazor.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using Serilog;
using _Imports = FxExpert.Blazor.Client._Imports;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

Log.Information("Starting up");

// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddTransient<AntiforgeryHandler>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

// TODO: Enable HTTPS for the event server
builder.Services.AddHttpClient("EventServer",
        client => client.BaseAddress = new Uri(builder.Configuration["EventServer"] ?? "http://localhost:5032"))
    .AddHttpMessageHandler<AntiforgeryHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("backend"));

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("EventServer"));

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Configure Authentication with Keycloak
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        // Get settings from configuration
        var config = builder.Configuration.GetSection("OpenIdConnect");
        options.Authority = config["Authority"];
        options.ClientId = config["ClientId"];
        options.ClientSecret = config["ClientSecret"];
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("roles");
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.RequireHttpsMetadata = config.GetValue<bool>("RequireHttpsMetadata");
        options.UsePkce = config.GetValue<bool>("UsePkce");
        options.SaveTokens = config.GetValue<bool>("SaveTokens");
        options.GetClaimsFromUserInfoEndpoint = config.GetValue<bool>("GetClaimsFromUserInfoEndpoint");


        // Set the callback paths
        options.CallbackPath = config["CallbackPath"] ?? "/signin-oidc";
        options.SignedOutCallbackPath = config["SignedOutCallbackPath"] ?? "/signout-callback-oidc";
        options.RemoteSignOutPath = config["RemoteSignOutPath"] ?? "/signout-oidc";

        // Log the configuration for debugging
        var callbackPath = options.CallbackPath;
        var signedOutCallbackPath = options.SignedOutCallbackPath;
        var remoteSignOutPath = options.RemoteSignOutPath;
        var errorPath = config["ErrorPath"] ?? "/authentication-failed";

        Console.WriteLine("OIDC Configuration:");
        Console.WriteLine($"Authority: {options.Authority}");
        Console.WriteLine($"ClientId: {options.ClientId}");
        Console.WriteLine($"CallbackPath: {callbackPath}");
        Console.WriteLine($"SignedOutCallbackPath: {signedOutCallbackPath}");
        Console.WriteLine($"RemoteSignOutPath: {remoteSignOutPath}");
        Console.WriteLine($"ErrorPath: {errorPath}");

        // Get host from configuration or use default
        var scheme = builder.Environment.IsDevelopment() ? "http" : "https";
        var host = builder.Configuration["ApplicationUrl"] ?? "localhost:8500";

        // Extract host from applicationUrl in launchSettings if available
        var urls = builder.WebHost.GetSetting("urls")?.Split(';');
        if (urls?.Length > 0 && !string.IsNullOrEmpty(urls[0]))
        {
            var url = new Uri(urls[0]);
            host = url.Authority;
        }

        Console.WriteLine($"Application host: {host}");
        Console.WriteLine($"Redirect URI: {scheme}://{host}{callbackPath}");
        Console.WriteLine($"Signout Redirect URI: {scheme}://{host}{signedOutCallbackPath}");
        Console.WriteLine($"Error Redirect URI: {scheme}://{host}{errorPath}");

        // Add scopes
        foreach (var scope in config.GetSection("Scope").Get<string[]>() ?? Array.Empty<string>())
            options.Scope.Add(scope);

        // Configure token validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = config["TokenValidationParameters:NameClaimType"] ?? "name",
            RoleClaimType = config["TokenValidationParameters:RoleClaimType"] ?? "roles",
            ValidateIssuer = true,
            ValidateAudience = false
        };

        // Custom handling for roles and errors
        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = context =>
            {
                // Map Keycloak roles to claims
                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity == null) return Task.CompletedTask;

                Console.WriteLine("OnTokenValidated: Token validated successfully");
                Console.WriteLine($"Identity Name: {identity.Name}");

                // Log all claims in the token
                Console.WriteLine("Claims in the token:");
                foreach (var claim in context.Principal.Claims)
                    Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");

                // Look for Keycloak-specific role claims
                // 1. Check standard resource_access claim from Keycloak
                var resourceAccessClaim = context.Principal.FindFirst("resource_access");
                if (resourceAccessClaim != null)
                {
                    Console.WriteLine($"Found resource_access claim: {resourceAccessClaim.Value}");

                    // Parse the JSON and extract roles
                    try
                    {
                        var resourceAccess =
                            JsonSerializer.Deserialize<Dictionary<string, object>>(resourceAccessClaim.Value);
                        if (resourceAccess != null && resourceAccess.ContainsKey("fx-expert"))
                        {
                            var clientRoles = JsonSerializer.Deserialize<Dictionary<string, object>>(
                                resourceAccess["fx-expert"].ToString());

                            if (clientRoles != null && clientRoles.ContainsKey("roles"))
                            {
                                var roles = JsonSerializer.Deserialize<string[]>(clientRoles["roles"].ToString());
                                foreach (var role in roles)
                                {
                                    Console.WriteLine($"Adding client role: {role}");
                                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing resource_access claim: {ex.Message}");
                    }
                }

                // 2. Check realm_access claim from Keycloak
                var realmAccessClaim = context.Principal.FindFirst("realm_access");
                if (realmAccessClaim != null)
                {
                    Console.WriteLine($"Found realm_access claim: {realmAccessClaim.Value}");

                    try
                    {
                        var realmAccess =
                            JsonSerializer.Deserialize<Dictionary<string, object>>(realmAccessClaim.Value);
                        if (realmAccess != null && realmAccess.ContainsKey("roles"))
                        {
                            var roles = JsonSerializer.Deserialize<string[]>(realmAccess["roles"].ToString());
                            foreach (var role in roles)
                            {
                                Console.WriteLine($"Adding realm role: {role}");
                                identity.AddClaim(new Claim(ClaimTypes.Role, role));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing realm_access claim: {ex.Message}");
                    }
                }

                // 3. Fall back to the specified role claim type
                var roleClaimType = config["TokenValidationParameters:RoleClaimType"] ?? "role";
                Console.WriteLine($"Looking for role claims with type: {roleClaimType}");

                foreach (var claim in context.Principal.Claims)
                    if (claim.Type.Contains(roleClaimType))
                    {
                        Console.WriteLine($"Adding role from {roleClaimType} claim: {claim.Value}");
                        identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                    }

                // Display the final roles
                Console.WriteLine("Final roles after mapping:");
                foreach (var claim in identity.Claims.Where(c => c.Type == ClaimTypes.Role))
                    Console.WriteLine($"Role: {claim.Value}");

                return Task.CompletedTask;
            },

            // Handle authentication errors
            OnRemoteFailure = context =>
            {
                context.Response.Redirect("/authentication-failed?error=" +
                                          Uri.EscapeDataString(context.Failure.Message));
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    // Default policy requires authentication
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Add role-based policies
    options.AddPolicy("RequireUserRole", policy =>
        policy.RequireRole("USER"));

    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("ADMIN"));
});

var app = builder.Build();

// Ensure Serilog is flushed on application shutdown.
app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();
app.UseAuthentication(); // Adds OIDC authentication middleware.
app.UseAuthorization(); // Adds authorization middleware.

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(_Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();

public class AntiforgeryHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Add("X-CSRF", "1");
        return base.SendAsync(request, cancellationToken);
    }
}