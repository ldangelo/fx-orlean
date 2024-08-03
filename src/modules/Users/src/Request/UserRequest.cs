namespace Users.Request;

public class UserRequest
{
    public required string userId { get; set; }
    public required string firstName { get; set; }
    public required string lastName { get; set; }
    public required string emailAddress { get; set; }
}
