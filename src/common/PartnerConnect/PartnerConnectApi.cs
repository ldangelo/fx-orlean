using System.Net;
using FluentAssertions;
using RestSharp;

namespace common.PartnerConnect;

public class PartnerConnectApi : IPartnerConnectApi
{
    public async Task<string?> Authorize(CancellationToken token)
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

        var request = new RestRequest("/");
        request.Method = Method.Post;
        //
        // Add authorization headers
        List<KeyValuePair<string, string>> headers = new();
        headers.Add(new KeyValuePair<string, string>("Content-Type", "application/x-www-form-urlencoded"));
        headers.Add(new KeyValuePair<string, string>("Accept", "*/*"));
        headers.Add(new KeyValuePair<string, string>("Connection", "keep-alive"));
        headers.Add(new KeyValuePair<string, string>("Accept-Encoding", "gzip, deflate"));

        var body = "grant_type=client_credentials&client_id=" + clientId + "&client_secret=" + clientSecret +
                   "&audience=partner-connect-api";

        request.AddHeaders(headers);
        request.AddJsonBody(body);

        var response = await restClient.ExecuteAsync<AuthResponse>(request, token);
        if (response.StatusCode != HttpStatusCode.OK) throw new Exception("Failed to authorize");

        return response?.Data?.access_token;
    }

    public async Task<User?> GetUser(string primaryEmail, CancellationToken cancellationToken)
    {
        var baseUrl = Environment.GetEnvironmentVariable("PARTNER_CONNECT_URL");
        var authToken = await Authorize(cancellationToken);

        var restOptions = new RestClientOptions(baseUrl);
        var restClient = new RestClient(restOptions);

        var request = new RestRequest("/api/Users");
        request.Method = Method.Get;
        request.AddHeader("Authorization", $"Bearer {authToken}");
        request.AddQueryParameter("Active", true);
        request.AddQueryParameter("PrimaryEmail", primaryEmail);

        var user = await restClient.ExecuteAsync<User[]>(request, cancellationToken);
        if (user.StatusCode != HttpStatusCode.OK) throw new Exception("Failed to get user");
        return user.Data?[0];
    }


    private class AuthResponse
    {
        public string? access_token { get; set; }
    }
}
