using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DentalNUB.Entities.Models
{
    public class Admin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdminID { get; set; }

        
        [MaxLength(100)]
        public string? AdName { get; set; }

       
        [MaxLength(100)]
        [EmailAddress]
        public string ? AdEmail { get; set; }

      
        [MaxLength(150)]
        public string ? AdPassword { get; set; }

       
        [MaxLength(20)]
        public string ? AdPhone { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }



    }


}
