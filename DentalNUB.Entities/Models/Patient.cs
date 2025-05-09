using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalNUB.Entities.Models
{

    public class Patient
    {
        public enum ChronicDisease
        {
            [Description("Heart disease")]
            HeartDisease = 1,

            [Description("Liver disease")]
            LiverDisease = 2,

            [Description("Renal disease")]
            RenalDisease = 3,

            [Description("Rheumatic fever")]
            RheumaticFever = 4,

            [Description("Hypertension")]
            Hypertension = 5,

            [Description("Diabetes")]
            Diabetes = 6,

            [Description("Stroke")]
            Stroke = 7,

            [Description("Radiotherapy")]
            Radiotherapy = 8
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PatientID { get; set; }

        [Required]
        [MaxLength(100)]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PatPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string NationalID { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public int Age { get; set; }

        public string? ChronicalDiseases { get; set; }

        public int? CigarettesPerDay { get; set; }
        public int? TeethBrushingFrequency { get; set; }
        public virtual ICollection<Appointment>? Appointments { get; set; }
        public virtual ICollection<Question>? Questions { get; set; }
        public virtual ICollection<ChatNUB>? ChatNUB { get; set; }
        public virtual ICollection<Notification>? Notification { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }

        public Patient() { }
        public Patient(string patientName, string patPhone, string nationalID, string gender, int age, string chronicalDiseases)
        {
            PatientName = patientName;
            PatPhone = patPhone;
            NationalID = nationalID;
            Gender = gender;
            Age = age;
            ChronicalDiseases = chronicalDiseases;
        }


    }
}