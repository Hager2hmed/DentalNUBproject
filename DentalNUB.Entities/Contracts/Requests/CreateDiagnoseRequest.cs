namespace DentalNUB.Entities

{
    public record CreateDiagnoseRequest
    {
        public int CaseID { get; set; }
        public int ConsID { get; set; }
        public string AssignedClinic { get; set; } = string.Empty;
        public string FinalDiagnose { get; set; } = string.Empty;
    }
}