using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DentalNUB.Entities.Models
{

    public enum ClinicType
    {
        VIP,
        Economic
    }
    public class Consultant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int ConsID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ConsName { get; set; }


        [EmailAddress]
        [MaxLength(100)]
        public string? ConsEmail { get; set; }


        public string? ConsPassword { get; set; }


        [MaxLength(100)]
        public string? Specialty { get; set; }

        [Required]
        public ClinicType ClinicType { get; set; }
        public virtual ICollection<Appointment>? Appointments { get; set; }
        public virtual ICollection<PatientCase> PatientCases { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public Consultant() { }


        public Consultant(string consName, string consEmail, string consPassword, string specialty, ClinicType clinicType)
        {
            ConsName = consName;
            ConsEmail = consEmail;
            ConsPassword = consPassword;
            Specialty = specialty;
            ClinicType = clinicType;
        }
    }
}