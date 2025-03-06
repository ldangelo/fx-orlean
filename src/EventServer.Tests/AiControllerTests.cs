using EventServer.Controllers;
using Xunit.Abstractions;

namespace EventServer.Tests;

public class AiControllerTests: IntegrationContext
{
       public AiControllerTests(AppFixture fixture, ITestOutputHelper output)
           : base(fixture, output) { }
 
       [Fact]
       public async Task Test_Dotnet()
       {
           AIRequest request = new AIRequest();
           request.ProblemDescription = "I have an early stage startup in the payments space.  I have 10 dotnet developers that are only producing 10 points a sprint.  Who can help me improve my team's productivity?";
           var result = await Scenario(x =>
           {
               x.Post.Json(request).ToUrl("/api/ai/partners");
               x.StatusCodeShouldBe(200);
           });
       }
       
}