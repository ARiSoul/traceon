using Microsoft.AspNetCore.Identity;

namespace Traceon.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string? PreferredTheme { get; set; }
    public string? PreferredLanguage { get; set; }
}
