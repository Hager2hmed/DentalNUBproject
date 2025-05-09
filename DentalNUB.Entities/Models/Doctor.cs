using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalNUB.Entities.Models
{

    public class Doctor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DoctorID { get; set; }


        [MaxLength(100)]
        public string? DoctorName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string DoctorEmail { get; set; }

        [Required]
        public string DoctorPassword { get; set; }

        [Required]
        [MaxLength(20)]
        public string DoctorPhone { get; set; }

        [Required]

        public int DoctorYear { get; set; }

        [Required]

        public int UniversityID { get; set; }
        public int? ClinicID { get; set; }
        public Clinic? Clinic { get; set; }
        public int SectionID { get; set; }

        [ForeignKey("SectionID")]
        public ClinicSection? ClinicSection { get; set; }

        public int? SectionOrder { get; set; }

        public virtual ICollection<PatientCase>? Patientcases { get; set; }
        public virtual ICollection<ChatMessage>? ChatMessages { get; set; }
        public virtual ICollection<ToolPost>? ToolPost { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }

        public Doctor() { }

        public Doctor(string doctorName, string doctorEmail, string doctorPassword, string doctorphone, int doctorYear, int universityID)
        {
            DoctorName = doctorName;
            DoctorEmail = doctorEmail;
            DoctorPassword = doctorPassword;
            DoctorPhone = doctorphone;
            DoctorYear = doctorYear;
            UniversityID = universityID;
        }
    }
}