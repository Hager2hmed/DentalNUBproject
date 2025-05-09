namespace DentalNUB.Entities

{

    public record DignoseResponse
    {
        public int DiagnoseID { get; set; }
        public string FinalDiagnose { get; set; } = string.Empty;
        public string AssignedClinic { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string ConsultantName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public List<int> CaseIDs { get; set; }
    }
}