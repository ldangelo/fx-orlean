using FastEndpoints;
using Users.Request;
using Users.Response;

namespace Users.Api;

public class CreateUsers: Endpoint<UserRequest, UserResponse>
{
  public override void Configure() {
      Post("/api/user/create");
      AllowAnonymous();
  }

  public override async Task HandleAsync(UserRequest request, CancellationToken ct)
  {
      await SendAsync(new () { Id = new Guid() });
  }
}
