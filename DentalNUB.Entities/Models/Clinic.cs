//using DocumentFormat.OpenXml.Bibliography;
//using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Security.AccessControl;
namespace DentalNUB.Entities.Models
{
    public class Clinic
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int ClinicID { get; set; }


        [MaxLength(255)]
        public string ClinicName { get; set; } = string.Empty;

        public int? AllowedYear { get; set; }  // مثلاً 3، 4، أو 5

        [Required]
        public int MaxStudent { get; set; }

        public string Schedule { get; set; } = string.Empty;

        public ICollection<ClinicSection> ClinicSections { get; set; } = new List<ClinicSection>();

        public ICollection<PatientCase>? PatientCases { get; set; }
        public ICollection<Doctor>? Doctors { get; set; }

    }
}
