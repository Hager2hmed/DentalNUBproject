
using DentalNUB.Entities.Models;

namespace DentalNUB.Entities
{

    // Request لتعديل بيانات المريض
    public class UpdatePatientProfileRequest
    {
        public int PatientID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatPhone { get; set; } = string.Empty;
        public string NationalID { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? ChronicalDiseases { get; set; }
    }

    // Request لتعديل بيانات الدكتور
    public class UpdateDoctorProfileRequest
    {
        public int DoctorID { get; set; }
        public string DoctorName { get; set; } =string.Empty;
        public string DoctorEmail { get; set; } = string.Empty;
        public string DoctorPhone { get; set; } = string.Empty;
        public int DoctorYear { get; set; }
        public int UniversityID { get; set; }
    }

    // Request لتعديل بيانات الكونسلتنت
    public class UpdateConsultantProfileRequest
    {
        public int ConsID { get; set; }
        public string ConsName { get; set; } = string.Empty;
        public string ConsEmail { get; set; } = string.Empty;
        public string ConsPhone { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public ClinicType ClinicType { get; set; }
    }

}