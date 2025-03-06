using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using Serilog;

public class ChatGPTWithRAG
{
  private static readonly string prompt = @"You are a managing partner for a consulting firm that specializes in fractional leadership.
                                 given this list of partners and associated skills {partnersAndSkills} tell me which partners
                                 would be best suited to solve the {problem} at hand.  Return a list of partners 
                                 as a properly format json object that looks like this [{ 'partinerId': 'email', 'skills': ['skill1','skill2', ...]}
                                 where partnerid is an e-mail addresses and skills are the relevant skills of the partner.";

  public async Task<List<PartnerInfo>> GetChatGPTResponse(string userQuery)
  {
    // Step 1: Retrieve relevant documents                                                                                                                                          
    String relevantInfo = RetrievePartnersAndSkills(userQuery);

    // Step 2: Combine user query with retrieved information                                                                                                                        
    string combinedInput = $"{relevantInfo}\n\nUser: {userQuery}";

    // Step 3: Call OpenAI API                                                                                                                                                      
    var response = await CallOpenAIAPI(combinedInput);

    return response;
  }

  private static readonly string sampleJson = @"
    [
      {parnterId: leo.dangelo@fortiumpartners.com, skills: [leadership, strategic thinking]},
      {partnerId: burke.autry@fortiumpartners.com, skills: [leadership, agile, AWS, Architecture, dotnet, c#, java]
    ]";


  private static string RetrievePartnersAndSkills(string query)
  {
    // Implement your retrieval logic here                                                                                                                                          
    // For example, search a database or use a search engine                                                                                                                        
    return sampleJson;
  }

  public class PartnerInfo
  {
    public string PartnerId { get; set; }
    public List<string> Skills { get; set; }
  }

  public class PartnerInfoList
  {
    List<PartnerInfo> Partners { get; set; }
  }
  private static async Task<List<PartnerInfo>> CallOpenAIAPI(string input)
  {
    List<ChatMessage> prompts = [
      new SystemChatMessage(prompt),
      new UserChatMessage(input)
    ];

    var client = new ChatClient("gpt-4o", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

    ChatCompletion response = client.CompleteChat(prompts);
    string responseString = response.Content[0].Text;
    responseString = responseString.TrimStart('`');
    responseString = responseString.TrimEnd('`');
    responseString = responseString.Replace("json", "");
    Serilog.Log.Information("ChatGPTWithRag: Response {response}", responseString);

    List<PartnerInfo> result = JsonConvert.DeserializeObject<List<PartnerInfo>>(responseString);
    Log.Information("Result: {result}", result.ToString());
    return result;
  }
}

