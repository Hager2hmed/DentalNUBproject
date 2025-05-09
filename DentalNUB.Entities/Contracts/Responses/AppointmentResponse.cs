using Azure.Core;
using DentalNUB.Entities.Models;


namespace DentalNUB.Entities
{
    public record AppointmentResponse
    {
        public int AppointmentId { get; set; }
        public string? PatientName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int QueueNumber { get; set; }
        public string EstimatedTime { get; set; } = string.Empty;
        public string? ChronicalDiseases { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? BookingTypeName { get; set; } // مثلاً "Normal" أو "VLP"


        // constructor modified to take the Appointment parameter
        public AppointmentResponse(Appointment appointment)
        {
            AppointmentId = appointment.AppointID;
            PatientName = appointment.Patient.PatientName;
            AppointmentDate = appointment.AppointDate;
            QueueNumber = 1; // or any logic to compute queue number
            EstimatedTime = appointment.AppointDate.ToString("hh:mm tt");
            ChronicalDiseases = appointment.Patient.ChronicalDiseases;
            Message = $"Your reservation has been completed successfully. Your Queue number is 1.";
        }
    }
}