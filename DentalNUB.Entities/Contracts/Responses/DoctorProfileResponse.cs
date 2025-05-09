namespace DentalNUB.Entities
{

    public class DoctorProfileResponse
    {
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorEmail { get; set; } = string.Empty;
        public string? DoctorPhone { get; set; }
        public int DoctorYear { get; set; }
        public int UniversityID { get; set; }

        public string ClinicName { get; set; } = string.Empty;
    }
}