using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DentalNUB.Entities.Models
{

    public class DoctorSectionRanking
    {
        [Key]
        public int RankID { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorID { get; set; }

        public int SectionNumber { get; set; }

        public int OrderInSection { get; set; }

        [ForeignKey("DoctorID")]
        public Doctor? Doctor { get; set; }
    }
}