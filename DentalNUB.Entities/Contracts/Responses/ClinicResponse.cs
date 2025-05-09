namespace DentalNUB.Entities
{
    public class ClinicResponse
    {
        public string ClinicName { get; set; } = string.Empty;
        public int MaxStudent { get; set; }
        public List<string> ScheduleDays { get; set; } = new();
        public int? AllowedYear { get; set; }
    }
}