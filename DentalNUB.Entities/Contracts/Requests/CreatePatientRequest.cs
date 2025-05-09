

namespace DentalNUB.Entities
{
    public record CreatePatientRequest
    {
        public string PatientName { get; set; } = string.Empty;
        public string PatPhone { get; set; } = string.Empty;
        public string NationalID { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public List<string>? ChronicalDiseases { get; set; }
        public int? CigarettesPerDay { get; set; }
        public int? TeethBrushingFrequency { get; set; }
    }
}
