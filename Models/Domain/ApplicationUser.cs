using Microsoft.AspNetCore.Identity;

namespace jaswer2;

public class ApplicationUser : IdentityUser
{
    public string? Name { get; set; }
}
