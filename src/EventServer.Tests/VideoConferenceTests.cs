using EventServer.Aggregates.VideoConference.Commands;
using Xunit.Abstractions;

namespace EventServer.Tests;


public class VideoConferenceTests : IntegrationContext
{
    public VideoConferenceTests(AppFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public async Task CreateVideoConference()
    {
        var command = new CreateVideoConferenceCommand(Guid.NewGuid(),DateTime.Now,DateTime.Now.AddHours(1),"test@fortiumpartners.com","test@google.com");

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/videos");
            x.StatusCodeShouldBe(201);
        });

    }
}
