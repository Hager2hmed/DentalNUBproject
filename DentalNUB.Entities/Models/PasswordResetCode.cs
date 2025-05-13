namespace DentalNUB.Entities.Models
{

    public class PasswordResetCode
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime Expiration { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
    }
}