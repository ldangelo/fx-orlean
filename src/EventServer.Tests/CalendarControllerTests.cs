using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EventServer.Services;
using Fortium.Types;
using Google.Apis.Calendar.v3.Data;
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
            Description = "Event Description",
            Start = new EventDateTime(),
            End = new EventDateTime(),
        };
        newEvent.Start.DateTime = DateTime.Now;
        newEvent.End.DateTime = DateTime.Now.AddHours(1);

        // Act
        var response = await Scenario(x =>
        {
            x.Post.Json(newEvent).ToUrl("/api/calendar/1/events");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var result = response.ReadAsJson<Event>();
        //        Assert.NotNull(result?.EventId);
    }
}
