using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalNUB.Entities.Models
{

    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationID { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [ForeignKey("Patient")]
        [Required]
        public int PatientID { get; set; }

        [Required]
        public string Message { get; set; }


        public virtual Patient? Patient { get; set; }

        public Notification(int patientID, string title, string message)
        {
            PatientID = patientID;
            Title = title;
            Message = message;
        }
    }
}