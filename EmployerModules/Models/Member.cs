using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EmployerModules.Models
{
    public class Member
    {
        [Required]
        public int Id { get; set; }

        public string Pin { get; set; }

       
        [Display(Name = "Employer Name")]
        public string EmployerName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        public string Email { get; set; }
      
        public virtual Employer Employer { get; set; }
        [Required]
        public int EmployerId { get; set; }
    }
}