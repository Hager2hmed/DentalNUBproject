using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalNUB.Entities.Models
{
    public class ToolPost
    {
        [Key]
        public int ToolPostID { get; set; }

        public int DoctorID { get; set; }

        [ForeignKey("DoctorID")]
        public Doctor Doctor { get; set; }

        public string ToolName { get; set; } = string.Empty;

        public decimal? Price { get; set; }

        public bool IsFree { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
    }
}