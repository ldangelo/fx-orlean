using Xunit.DependencyInjection;

namespace EventServer.Tests.Aggregates.VideoConference;

[Collection("Fx Collection")]
public class VideoConferenceAggregateTests : FxTest
{
    public VideoConferenceAggregateTests(ITestOutputHelperAccessor accessor) : base(accessor)
    {
    }
}