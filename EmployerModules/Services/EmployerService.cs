using EmployerModules.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmployerModules.Services
{
    public class EmployerService
    {
        private ApplicationDbContext db;

        public EmployerService(ApplicationDbContext dbContext)
        {
            db = dbContext;
        }

        public void CreateEmployer( string name, string address, string userId)
        {
            var db = new ApplicationDbContext();
            var employeeID = "";

            if (db.Employers.Count() == 0)
            {
                employeeID = "PR" + "1".PadLeft(10, '0');
            }
            else
            {
                employeeID = "PR" + (1 + db.Employers.Count()).ToString().PadLeft(10, '0');
            }
            var employer = new Employer
            {
                Address = address,
                Name = name,
                EmployerId = employeeID,
                ApplicationUserId = userId
            };
            db.Employers.Add(employer);
            db.SaveChanges();
        }
    }
}