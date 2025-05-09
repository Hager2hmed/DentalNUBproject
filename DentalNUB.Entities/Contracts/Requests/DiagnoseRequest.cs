using System.ComponentModel.DataAnnotations;

namespace DentalNUB.Entities

{
    public record DiagnoseRequest
    {
        [Required]
        public int AppointID { get; set; }

        [Required]
        [MaxLength(255)]
        public string AssignedClinic { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FinalDiagnose { get; set; } = string.Empty;
    }
}