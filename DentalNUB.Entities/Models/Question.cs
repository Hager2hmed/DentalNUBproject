using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalNUB.Entities.Models
{

    public class Question
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuestID { get; set; }

        [Required]
        public string? QuesText { get; set; }

        [ForeignKey("Patient")]

        public int? PatientID { get; set; }

        public virtual Patient? Patient { get; set; }
        public Answer? Answer { get; set; }
        public Question()
        {

        }

        public Question(int patientID, string quesText)
        {
            PatientID = patientID;
            QuesText = quesText;
        }
    }
}