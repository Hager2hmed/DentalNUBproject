
namespace DentalNUB.Entities
{
    public class GetAppointByIDResponse
    {
        public CreatePatientRequest? PatientDetails { get; set; }
        public string Complaint { get; set; } = string.Empty;
        public string? XRayImage { get; set; }
    }
}
