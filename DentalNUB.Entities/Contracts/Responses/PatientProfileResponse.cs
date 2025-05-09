namespace DentalNUB.Entities
{

    public class PatientProfileResponse
    {
        public string PatientName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PatPhone { get; set; }
    }
}