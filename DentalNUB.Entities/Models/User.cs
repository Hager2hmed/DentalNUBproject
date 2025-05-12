using Microsoft.AspNetCore.Identity;

namespace DentalNUB.Entities.Models
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public Doctor? Doctor { get; set; }
        public Patient? Patient { get; set; }
        public Consultant? Consultant { get; set; }
        public Admin? Admin { get; set; }
    }
}