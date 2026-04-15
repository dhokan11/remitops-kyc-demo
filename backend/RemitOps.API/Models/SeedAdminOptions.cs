namespace RemitOps.API.Models;

public class SeedAdminOptions
{
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FirstName { get; set; } = "Platform";
    public string LastName { get; set; } = "Admin";
}