using FxExpert.Blazor.Components;
using FxExpert.Blazor.Components.Account;
using FxExpert.Blazor.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

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