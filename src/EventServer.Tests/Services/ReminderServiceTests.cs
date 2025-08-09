using EventServer.Services;
using Shouldly;
using Xunit.Abstractions;

namespace EventServer.Tests.Services;

public class ReminderServiceTests
{
    private readonly ITestOutputHelper _output;

    public ReminderServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }
    // Note: ReminderService integration tests are covered in BookingIntegrationTests
    // The service depends on EmailService and complex timer scheduling which is better tested through integration
}