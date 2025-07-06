using EventServer.Aggregates.Partners;
using Fortium.Types;
using Marten;
using Newtonsoft.Json;
using OpenAI.Chat;
using Serilog;

public class ChatGPTWithRAG
{
  private const string SamplePartnerJson = @"[
  {
    ""partnerId"": ""leo.dangelo@fortiumpartners.com"",
    ""skills"": [""leadership"", ""strategic thinking""]
  },
  {
    ""partnerId"": ""burke.autry@fortiumpartners.com"",
    ""skills"": [""leadership"", ""agile"", ""AWS"", ""Architecture"", ""dotnet"", ""c#"", ""java""]
  }
]";

  private static readonly string prompt = @"
You are a managing partner for a consulting firm that specializes in fractional leadership.
given this list of partners use the associated skills and experience to tell me which partners
would be best suited to solve the {problem} at hand.  in descending order of their relevance to the problem.
Add a rank to each partner.  Add a reason why you think the partner is a good fit.
Return a rank sorted list of all partners as a properly format json object.
Only return json no additonal information is necessary.
Do not halucinate.
";

  private readonly List<Partner> samplePartners = new();

  private readonly IDocumentStore _store;
  public ChatGPTWithRAG(IDocumentStore store)
  {
    _store = store;
    var leo = new Partner();

    leo.FirstName = "Leo";
    leo.LastName = "DAngelo";
    leo.EmailAddress = "leo.dangelo@fortiumpartners.com";
    leo.Skills.Add(new PartnerSkill("leadership", 30, ExperienceLevel.Expert));
    leo.Skills.Add(new PartnerSkill("architecture", 30, ExperienceLevel.Expert));
    leo.Skills.Add(new PartnerSkill("aws", 30, ExperienceLevel.Expert));
    leo.Skills.Add(new PartnerSkill("agile", 20, ExperienceLevel.Expert));
    leo.Skills.Add(new PartnerSkill("test driven development", 20, ExperienceLevel.Expert));
    leo.Skills.Add(new PartnerSkill("AI", 20, ExperienceLevel.Expert));
    leo.Skills.Add(new PartnerSkill("dotnet", 20, ExperienceLevel.Expert));
    leo.Skills.Add(new PartnerSkill("c#", 20, ExperienceLevel.Expert));
    leo.Skills.Add(new PartnerSkill("java", 30, ExperienceLevel.Expert));
    leo.WorkHistories.Add(new WorkHistory(DateOnly.FromDateTime(DateTime.Now.AddYears(-10)), null,
        "Fortium Partners", "CTO",
        "Fractional CTO with experience working with SaaS companies in the financial services industry"));
    leo.WorkHistories.Add(new WorkHistory(DateOnly.FromDateTime(DateTime.Now.AddYears(-6)),
        DateOnly.FromDateTime(DateTime.Now.AddYears(-1)),
        "Allied Payment Network", "CTO",
        "As CTO, I was responsible for the development of the company's payments platform. I led a team of 15 C# developers, 7 QA engineers, and 2 business analysts. To produce a state of the art consumer bill payment platform, I was responsible for the development of a new payment platform that was designed to be scalable, flexible, and easy to use."));
    samplePartners.Add(leo);

    var burke = new Partner();
    burke.FirstName = "Burke";
    burke.LastName = "Autrey";
    burke.EmailAddress = "burke.autrey@fortiumpartners.com";
    burke.Skills.Add(new PartnerSkill("leadership", 30, ExperienceLevel.Expert));
    burke.Skills.Add(new PartnerSkill("strategic thinking", 30, ExperienceLevel.Expert));
    burke.WorkHistories.Add(new WorkHistory(DateOnly.FromDateTime(DateTime.Now.AddYears(-10)), null,
        "Fortium Partners", "CEO", "Founder and CEO of Fortium Partners"));
    samplePartners.Add(burke);
  }

  public async Task<List<Partner>> GetChatGPTResponse(string userQuery)
  {
    // Step 1: Retrieve relevant documents                                                                                                                                          
    var relevantInfo = RetrievePartnersAndSkills(userQuery);

    // Step 2: Combine user query with retrieved information                                                                                                                        
    var combinedInput = $"{relevantInfo}\n\nUser: {userQuery}";

    // Step 3: Call OpenAI API                                                                                                                                                      
    var response = await CallOpenAIAPI(combinedInput);

    return response;
  }

  private string RetrievePartnersAndSkills(string query)
  {
    PartnerProjection projection = new();

    var session = _store.QuerySession();
    var partners = session.Query<Partner>().Where(p => p.AvailabilityNext30Days > 0);


    // Implement your retrieval logic here                                                                                                                                          
    // For example, search a database or use a search engine                                                                                                                        
    return JsonConvert.SerializeObject(partners.ToArray());
  }

  private static string CleanUpAnswer(string answer)
  {
    answer = answer.Replace("json", "");
    answer = answer.TrimStart('`');
    answer = answer.TrimEnd('`');

    return answer;
  }

  private static Task<List<Partner>> CallOpenAIAPI(string input)
  {
    List<ChatMessage> prompts =
    [
        new SystemChatMessage(prompt),
            new UserChatMessage(input)
    ];

    var client = new ChatClient("gpt-4o", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

    ChatCompletion response = client.CompleteChat(prompts);
    var responseString = response.Content[0].Text;

    responseString = CleanUpAnswer(responseString);

    Log.Information("ChatGPTWithRag: Response {response}", responseString);

    var result = JsonConvert.DeserializeObject<List<Partner>>(responseString);

    Log.Information("Result: {result}", result?.ToString() ?? "null");

    return Task.FromResult(result ?? new List<Partner>());
  }
}
