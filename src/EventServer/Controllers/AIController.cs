using EventServer.Aggregates.Calendar.Commands;
using EventServer.Aggregates.Calendar.Events;
using EventServer.Services;
using Fortium.Types;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Wolverine.Http;
using Wolverine.Marten;
using static ChatGPTWithRAG;
using Events = Google.Apis.Calendar.v3.Data.Events;

namespace EventServer.Controllers;

public class AIRequest
{
  public string ProblemDescription { get; set; }
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

    var partners = _chatGPTWithRAG.GetChatGPTResponse(r.ProblemDescription).Result;
    Log.Information("AIController: {output}", JsonConvert.SerializeObject(partners));
    await SendAsync(partners);
  }
}
using EventServer.Aggregates.Calendar.Commands;
using EventServer.Aggregates.Calendar.Events;
using EventServer.Services;
using Fortium.Types;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Wolverine.Http;
using Wolverine.Marten;
using static ChatGPTWithRAG;
using Events = Google.Apis.Calendar.v3.Data.Events;

namespace EventServer.Controllers;

public class AIRequest
{
  public string ProblemDescription { get; set; }
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

    var partners = _chatGPTWithRAG.GetChatGPTResponse(r.ProblemDescription).Result;
    Log.Information("AIController: {output}", JsonConvert.SerializeObject(partners));
    await SendAsync(partners);
  }
}
