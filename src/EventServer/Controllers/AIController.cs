using Fortium.Types;
using Newtonsoft.Json;
using Serilog;

namespace EventServer.Controllers;

public class AIRequest
{
  public string? ProblemDescription { get; set; }
}

public class AIController : FastEndpoints.Endpoint<AIRequest, List<Partner>>
{
  private readonly ChatGPTWithRAG _chatGPTWithRAG;

  public AIController(ChatGPTWithRAG chatGPTWithRAG)
  {

    _chatGPTWithRAG = chatGPTWithRAG;
  }

  public override void Configure()
  {
    Post("/api/ai/partners");
    AllowAnonymous();
  }

  public override async Task HandleAsync(AIRequest r, CancellationToken cancellationToken)
  {
    Log.Information("AIController: {description}", r.ProblemDescription);

    var partners = _chatGPTWithRAG.GetChatGPTResponse(r.ProblemDescription!).Result;
    Log.Information("AIController: {output}", JsonConvert.SerializeObject(partners));
    await SendAsync(partners);
  }
}
