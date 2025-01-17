using FastEndpoints;

namespace org.fortium.fx;

public class TestEndpoint:Endpoint<TestRequest, TestResponse>
{
    public override void Configure()
    {
        Post("/api/test/test");
        AllowAnonymous();
    }


    public override async Task HandleAsync(TestRequest request, CancellationToken ct)
    {
            await SendAsync(new () { response = "It works!" });
    }

}

public class TestResponse
{
    public required string response;
}

public class TestRequest
{
    public required string request {get; set;}
}
