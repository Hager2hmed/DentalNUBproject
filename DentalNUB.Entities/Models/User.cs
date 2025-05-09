namespace DentalNUB.Entities.Models
{

    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;


        public Doctor? Doctor { get; set; }
        public Patient? Patient { get; set; }
        public Consultant? Consultant { get; set; }
        public Admin? Admin { get; set; }
    }
}