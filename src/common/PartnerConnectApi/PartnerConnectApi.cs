using FluentAssertions;
using RestSharp;

namespace common.PartnerConnectApi;

public class PartnerConnectApi : IPartnerConnectApi
{
    public string? Authorize()
    {
        var authBaseUrl = Environment.GetEnvironmentVariable("PARTNER_CONNECT_AUTH_URL");
        var clientId = Environment.GetEnvironmentVariable("PARTNER_CONNECT_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("PARTNER_CONNECT_CLIENT_SECRET");
        var apiKey = Environment.GetEnvironmentVariable("PARTNER_CONNECT_API_KEY");

        authBaseUrl.Should().NotBeNullOrEmpty();
        clientId.Should().NotBeNullOrEmpty();
        clientSecret.Should().NotBeNullOrEmpty();
        apiKey.Should().NotBeNullOrEmpty();

        var restOptions = new RestClientOptions(authBaseUrl);
        var restClient = new RestClient(restOptions);

        var request = new RestRequest();
        request.RequestFormat = DataFormat.Json;

        //
        // Add authorization headers
        List<KeyValuePair<string, string>> headers = new();
        headers.Add(new KeyValuePair<string, string>("ClientId", clientId));
        headers.Add(new KeyValuePair<string, string>("ClientSecret", clientSecret));
        headers.Add(new KeyValuePair<string, string>("ApiKey", apiKey));
        headers.Add(new KeyValuePair<string, string>("Audience", "partner-connect-api"));
        request.AddHeaders(headers);

        var response = restClient.Execute(request);

        return response?.Content;
    }

    public User GetUser(string primaryEmail)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string? ToString()
    {
        return base.ToString();
    }
}