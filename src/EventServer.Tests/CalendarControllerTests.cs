using System.Text.Json;
using EventServer.Aggregates.Calendar.Commands;
using EventServer.Aggregates.Calendar.Events;
using Google.Apis.Calendar.v3.Data;
using Serilog;
using Xunit.Abstractions;

namespace EventServer.Tests;

public class CalendarControllerTests : IntegrationContext
{
    public CalendarControllerTests(AppFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    private void DumpConferenceData(ConferenceData data)
    {
        if (data != null)
        {
            var jsonData = JsonSerializer.Serialize(data);
            Log.Information("Conference Data: {JsonData}", jsonData);
        }
    }

    [Fact]
    public async Task GetCalendar_ShouldReturnCalendar()
    {
        // Arrange
        // Act
        var response = await Scenario(x =>
        {
            x.Get.Url("/api/calendar/leo.dangelo@fortiumpartners.com/events");
            x.StatusCodeShouldBe(200);
        });
        var events = response.ReadAsJson<Events>();

        Log.Information("Events: {Description}", events.Description);
        foreach (var e in events.Items)
        {
            Log.Information("Event: {Description}", e.Summary);
            DumpConferenceData(e.ConferenceData);
        }
    }

    [Fact]
    public async Task CreateEvent_ShouldReturnSuccess()
    {
        // Arrange
        var command = new CreateCalendarEventCommand(
            Guid.NewGuid().ToString(),
            "Work",
            "Test",
            "Test Event",
            DateTime.Now.AddHours(1),
            DateTime.Now.AddHours(2),
            "leo.dangelo@fortiumpartners.com",
            "ldangelo@mac.com"
        );

        // Act
        var response = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/api/calendar/leo.dangelo@fortiumpartners.com/events");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var result = response.ReadAsJson<CalendarEventCreatedEvent>();
        //        Assert.NotNull(result?.EventId);
    }
}
