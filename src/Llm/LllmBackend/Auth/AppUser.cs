using Microsoft.AspNetCore.Identity;

namespace LllmBackend.Auth
{
    public class AppUser : IdentityUser
    {
        public IEnumerable<IdentityRole>? Roles { get; set; }
    }
}
