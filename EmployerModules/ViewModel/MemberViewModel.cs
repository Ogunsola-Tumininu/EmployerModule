using EmployerModules.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmployerModules.ViewModel
{
    public class MemberViewModel
    {
        public Employee memberVm { get; set; }
        public Contribution contributionsVm { get; set; }
    }
}