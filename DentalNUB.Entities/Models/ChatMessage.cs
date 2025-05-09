using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalNUB.Entities.Models
{

    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // زيادة تلقائية
        public int MessageID { get; set; }


        public int SenderID { get; set; }


        public int ReceiverID { get; set; }

        public int? DoctorID { get; set; }

        public int ToolID { get; set; }

        public string? MessageContent { get; set; }


        public DateTime DateSent { get; set; }


        [ForeignKey("DoctorID")]
        public Doctor? Doctor { get; set; }
        public ChatMessage() { }

        public ChatMessage(int senderID, int receiverID, int doctorID, int toolID, string? messageContent = null)
        {
            SenderID = senderID;
            ReceiverID = receiverID;
            DoctorID = doctorID;
            ToolID = toolID;
            MessageContent = messageContent;
        }
    }
}