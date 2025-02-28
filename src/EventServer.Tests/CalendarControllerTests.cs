using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EventServer.Services;
using EventServer.Models; // Assuming Event is in this namespace
using Fortium.Types;
using Xunit;
using Xunit.Abstractions;

namespace EventServer.Tests;

public class CalendarControllerTests : IntegrationContext
{
    public CalendarControllerTests(AppFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public async Task CreateEvent_ShouldReturnSuccess()
    {
        // Arrange
        var newEvent = new Event
        {
            Title = "New Event",
            Description = "Event Description",
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        // Act
        var response = await Scenario(x =>
        {
            x.Post.Json(newEvent).ToUrl("/api/calendar/1/events");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var result = response.ReadAsJson<Event>();
        Assert.NotNull(result?.EventId);
    }
}
