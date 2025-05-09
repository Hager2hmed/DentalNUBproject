using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DentalNUB.Entities.Models

{
    public class Answer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnsID { get; set; }

        [Required]
        public string AnsText { get; set; }

        [ForeignKey("Question")]
        public int QuestID { get; set; }


        public virtual Question Question { get; set; }

        public Answer(string ansText, int questID)
        {
            AnsText = ansText;
            QuestID = questID;
        }
    }
}
