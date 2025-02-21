using Microsoft.AspNetCore.Identity;

namespace LlmBackend.Auth
{
    public class AppUser : IdentityUser
    {
        public IEnumerable<IdentityRole>? Roles { get; set; }
    }
}
