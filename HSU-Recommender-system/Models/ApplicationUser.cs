using Microsoft.AspNetCore.Identity;

namespace HSU_recommneder_system.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}

