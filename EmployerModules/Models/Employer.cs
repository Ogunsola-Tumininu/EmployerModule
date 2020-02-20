using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EmployerModules.Models
{
    public class Employer
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string EmployerId { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Name { get; set; }

        //remember to add this public DbSet<CheckingAccount> CheckingAccounts { get; set; } in Identity model

        // foreign key
        public virtual ApplicationUser User { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }


        // This is added because Checking Account is a Foreign Key in transaction

        public virtual ICollection<Member> Members { get; set; }
    }
}