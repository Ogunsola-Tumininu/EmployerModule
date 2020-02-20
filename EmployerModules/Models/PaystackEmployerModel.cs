
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmployerModules.Models
{
    public class PaystackEmployerModel
    {
        public string EmployerName { get; set; }
        public string EmployerId { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public int amount { get; set; }
    }
}