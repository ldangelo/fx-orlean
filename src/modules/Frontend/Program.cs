using common.Commands;
using Frontend.Components;
using Frontend.Services;
using Keycloak.AuthServices.Authentication;
using org.fortium.fx.Aggregates;
using Orleankka;
using Orleankka.Client;
using Orleankka.Cluster;
using static Microsoft.AspNetCore.Builder.WebApplication;

var builder = CreateBuilder(args);

builder.WebHost.ConfigureKestrel((context, options) => options.Configure(context.Configuration.GetSection("Kestrel")));
builder.Services.AddKeycloakWebAppAuthentication(builder.Configuration);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<CookiePolicyOptions>(options => { options.Secure = CookieSecurePolicy.Always; });


//
// connect too EventServer and add a singleton for dependency injection
var actorSystem = await Connect(3, TimeSpan.FromSeconds(3));


builder.Services.AddSingleton(actorSystem);
builder.Services.AddScoped<PartnerService>();

var app = builder.Build();

//
// add a partner for testing purposes
if (app.Environment.IsDevelopment())
{
    var actorRef = actorSystem.ActorOf<IPartnerAggregate>("leo.dangelo@fortiumpartners.com");
    await actorRef.Tell(new CreatePartnerCommand("Leo", "D'Angelo", "leo.dangelo@fortiumpartners.com"));
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
// keycloak authentication and authorization
app.UseAuthentication();
app.UseAuthorization();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


void OnClusterConnectionLost()
{
    Console.WriteLine("Cluster connection lost. Trying to reconnect ...");
}


async Task<IClientActorSystem> Connect(int retries = 0, TimeSpan? retryTimeout = null)
{
    if (retryTimeout == null)
        retryTimeout = TimeSpan.FromSeconds(5);

    if (retries < 0)
        throw new ArgumentOutOfRangeException(nameof(retries),
            "retries should be greater than or equal to 0");

    while (true)
        try
        {
            var host = new HostBuilder()
                .UseOrleansClient(c => c
                    .UseLocalhostClustering()
                    .AddMemoryStreams("sms")
                    .ConfigureServices(x => x
                        .AddSingleton<ConnectionToClusterLostHandler>((s, e) => OnClusterConnectionLost())))
                .UseOrleankka()
                .Build();

            await host.StartAsync();
            return host.ActorSystem();
        }
        catch (Exception ex)
        {
            if (retries-- == 0)
            {
                Console.WriteLine("Can't connect to cluster. Max retries reached.");
                throw;
            }

            Console.WriteLine(
                $"Can't connect to cluster: '{ex.Message}'. Trying again in {(int)retryTimeout.Value.TotalSeconds} seconds ...");
            await Task.Delay(retryTimeout.Value);
        }
}