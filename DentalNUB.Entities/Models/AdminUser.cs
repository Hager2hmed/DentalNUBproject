using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalNUB.Entities.Models
{

    public class AdminUser : IdentityUser
    {
        public string FullName { get; set; }
        public int BranchId { get; set; }
        public int JobId { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public int UserType { get; set; }
    }
}


