using Microsoft.AspNetCore.Identity;

namespace EmlakPortal.API.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        // Bir kullanıcının (emlakçının) birden fazla ilanı olabilir
        public ICollection<Estate>? Estates { get; set; }
    }
}