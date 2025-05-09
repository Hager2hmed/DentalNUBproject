using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalNUB.Entities.Models
{

    public class ChatNUB
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogID { get; set; }

        [ForeignKey("Patient")]
        [Required]
        public int PatientID { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime TimeStamp { get; set; } = DateTime.Now;

        [Required]
        public string Question { get; set; }

        public string Answer { get; set; }

        public virtual Patient? Patient { get; set; }


        public ChatNUB(int patientID, string question, string answer)
        {
            PatientID = patientID;
            Question = question;
            Answer = answer;

            TimeStamp = DateTime.Now;
        }
    }
}