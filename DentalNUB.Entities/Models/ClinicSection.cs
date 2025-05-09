using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalNUB.Entities.Models
{

    public class ClinicSection
    {
        [Key]
        public int SectionID { get; set; }

        public int ClinicID { get; set; }

        public string SectionName { get; set; } = string.Empty;

        public int DoctorYear { get; set; }

        public int MaxStudents { get; set; }

        [ForeignKey("ClinicID")]
        public Clinic? Clinic { get; set; }


        public ICollection<Doctor>? Doctors { get; set; }
    }
}