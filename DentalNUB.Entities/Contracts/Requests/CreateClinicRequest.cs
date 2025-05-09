namespace DentalNUB.Entities
{
    public record CreateClinicRequest
    {
        public string ClinicName { get; set; } = string.Empty;

        public int MaxStudent { get; set; }

        public string Schedule { get; set; } = string.Empty;

        public int? AllowedYear { get; set; }
    }
}
