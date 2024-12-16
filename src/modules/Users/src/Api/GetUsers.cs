using FastEndpoints;
using Users.Response;

namespace Users.Api;

public class GetUsers : Endpoint<GetUserRequest,UserResponse > 
{
    public override void Configure()
    {
        Get("/api/user/get");
        AllowAnonymous();
        base.Configure();
    }
    
    public override async Task HandleAsync(GetUserRequest request, CancellationToken ct)
    {
        await SendAsync(new () { Id = new Guid() });
    }
}

public class GetUserRequest
{
    public string? userId {get; set;}
}