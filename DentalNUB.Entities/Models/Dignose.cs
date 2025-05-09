using DentalNUB.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DentalNUB.Entities.Models
{
    public class Diagnose
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DiagnoseID { get; set; }
        public int ConsID { get; set; }
        public int AppointID { get; set; }
        public int ClinicID { get; set; }

        [MaxLength(255)]
        public string AssignedClinic { get; set; } = string.Empty;

        [MaxLength(500)]
        public string FinalDiagnose { get; set; } = string.Empty;

        public virtual ICollection<PatientCase> Cases { get; set; } = new List<PatientCase>();

        [ForeignKey("ConsID")]
        public Consultant? Consultant { get; set; }

        [ForeignKey("AppointID")]
        public Appointment appointment { get; set; }

        public Clinic clinic { get; set; }
        public Diagnose() { }

    }  
}