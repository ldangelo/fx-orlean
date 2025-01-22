using EventServer;
using EventServer.Client.Services;
using EventServer.Components;
using EventServer.Services;
using FastEndpoints;
using MudBlazor.Services;
using static common.FxHostingExtension;
using _Imports = EventServer.Client._Imports;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

//
// Add MudBlazor services
builder.Services.AddMudServices();

// Add FastEndpoints
builder.Services.AddFastEndpoints();

//
// connect too the orleans cluster
builder.UseFx();

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
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(_Imports).Assembly);
app.MapGroup("/authentication").MapLoginAndLogout();
app.Run();