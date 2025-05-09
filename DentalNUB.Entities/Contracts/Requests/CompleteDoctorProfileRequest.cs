using System.ComponentModel.DataAnnotations;

namespace DentalNUB.Entities
{

    public class CompleteDoctorProfileRequest
    {
        public int UserId { get; set; }

        [Required]
        public string ClinicName { get; set; } = string.Empty;

        [Required]
        [Range(3, 6, ErrorMessage = "DoctorYear must be 3, 4, 5 or 6")]
        public int DoctorYear { get; set; }

        [Required]
        public string DoctorPhone { get; set; } = string.Empty;

        [Required]
        public int UniversityID { get; set; }
    }
}