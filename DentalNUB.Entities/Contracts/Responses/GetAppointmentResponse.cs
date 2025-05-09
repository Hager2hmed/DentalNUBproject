namespace DentalNUB.Entities

{

    public class GetAppointmentResponse
    {
        public CreatePatientRequest? CreatePatientRequest { get; set; }
        public string Complaint { get; set; } = string.Empty;
        public string? XRayImage { get; set; }
    }
}
