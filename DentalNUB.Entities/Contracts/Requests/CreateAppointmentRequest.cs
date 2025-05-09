using Azure.Core;
using DentalNUB.Entities;
using DentalNUB.Entities.Models;
using Microsoft.AspNetCore.Http;  // إضافة المسار الصحيح للـ enum

namespace DentalNUB.Entities
{
    public record CreateAppointmentRequest
    {
        public CreatePatientRequest? PatientData { get; set; }
        public string Complaint { get; set; } = string.Empty;
        public IFormFile? XRayImage { get; set; }
        public Appointment.BookingTypeEnum BookingType { get; set; } = Appointment.BookingTypeEnum.Normal;  // استخدام BookingType
      
    }
}
