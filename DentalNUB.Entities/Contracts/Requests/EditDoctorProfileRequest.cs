namespace DentalNUB.Entities

{
    public class EditDoctorProfileRequest
    {
        public string? DoctorPhone { get; set; }
        public int DoctorYear { get; set; }

        public string ClinicName { get; set; } = string.Empty;
    }
}