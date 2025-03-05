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
using Events = Google.Apis.Calendar.v3.Data.Events;

namespace EventServer.Controllers;

public class AIController
{
  public AIController(ChatGPTWithRAG chatGPTWithRAG)
  {

  }

  [WolverineGet("/api/ai/problem/{description}")]
  static List<Partner> GetPartners(string description)
  {

    return new List<Partner>();
  }
}
