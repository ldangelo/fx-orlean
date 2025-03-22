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
using System.Security.Claims;
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
        
        Console.WriteLine($"OIDC Configuration:");
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
        {
            options.Scope.Add(scope);
        }
        
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
                
                if (context.TokenEndpointResponse?.AccessToken != null)
                {
                    var roleClaimType = config["TokenValidationParameters:RoleClaimType"] ?? "roles";
                    // Add roles from access token if present
                    foreach (var claim in context.Principal.Claims)
                    {
                        if (claim.Type == roleClaimType && !identity.HasClaim(roleClaimType, claim.Value))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                        }
                    }
                }
                
                return Task.CompletedTask;
            },
            
            // Handle authentication errors
            OnRemoteFailure = context =>
            {
                context.Response.Redirect("/authentication-failed?error=" + Uri.EscapeDataString(context.Failure.Message));
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