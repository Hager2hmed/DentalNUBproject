namespace DentalNUB.Entities
{

    public class CasesDetailsResponse
    {
        public int CaseID { get; set; } // مهم
        public string PatientName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string PatPhone { get; set; } = string.Empty;
        public string? ChronicalDiseases { get; set; }

    }
}