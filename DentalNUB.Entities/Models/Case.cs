using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalNUB.Entities.Models
{

    [Table("Case")]
    public class PatientCase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int CaseID { get; set; }


        public int PatientID { get; set; }


        public int DiagnoseID { get; set; }


        public int? DoctorID { get; set; }


        public int? ConsID { get; set; }

        public int ClinicID { get; set; }


        [MaxLength(50)]
        public string? CaseStatus { get; set; }

        [Column(TypeName = "date")]
        public DateTime? StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EndDate { get; set; }

        public Patient Patient { get; set; }

        [ForeignKey("DiagnoseID")]
        public Diagnose Diagnose { get; set; }

        [ForeignKey("DoctorID")]
        public virtual Doctor? Doctor { get; set; }

        [ForeignKey("ConsID")]
        public Consultant? Consultant { get; set; }

        [ForeignKey("ClinicID")]
        public Clinic Clinic { get; set; }

        public PatientCase() { }

        public PatientCase(int patientID, int diagnoseID, int doctorID, string caseStatus, int consID)
        {
            PatientID = patientID;
            DiagnoseID = diagnoseID;
            DoctorID = doctorID;
            CaseStatus = caseStatus;
            ConsID = consID;
        }
    }
}