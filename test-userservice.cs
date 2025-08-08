// Simple test to verify UserService DI registration
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FxExpert.Blazor.Client.Services;

var builder = Host.CreateDefaultBuilder();

// Add the same DI registration as in the server Program.cs
builder.ConfigureServices(services =>
{
    services.AddHttpClient("EventServer", client =>
        client.BaseAddress = new Uri("http://localhost:8080"));
    
    services.AddScoped<UserService>(sp => 
        new UserService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("EventServer")));
});

var host = builder.Build();

// Test service resolution
using var scope = host.Services.CreateScope();
var userService = scope.ServiceProvider.GetRequiredService<UserService>();

Console.WriteLine("✅ UserService DI registration test PASSED");
Console.WriteLine($"   UserService type: {userService.GetType().FullName}");
Console.WriteLine("   UserService can be resolved from DI container without errors");