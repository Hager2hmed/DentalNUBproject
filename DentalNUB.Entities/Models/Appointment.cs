using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalNUB.Entities.Models
{
    public class Appointment

    {
        public enum BookingTypeEnum
        {
            Normal = 0,   // حجز عادي
            VIP = 1      // حجز VIP
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppointID { get; set; }

        public int PatientID { get; set; }

        public int? ConsID { get; set; }

        [Required]
        public int BookingType { get; set; }

        [Required]
        public DateTime AppointDate { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string Complaint { get; set; } = string.Empty;


        public int QueueNumber { get; set; }
        public string? XRayImage { get; set; }


        [ForeignKey("PatientID")]
        public virtual Patient? Patient { get; set; }

        [ForeignKey("ConsID")]
        public Consultant? Consultant { get; set; }
        public Diagnose? Diagnose { get; set; }

        public Appointment() { }
    }
}