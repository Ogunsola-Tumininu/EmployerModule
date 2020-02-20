using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmployerModules.Models
{
    public class DataClass
    {
        public string txid { set; get; }
        public string txref { set; get; }
        public string amount { set; get; }
        public string currency { set; get; }
        public string chargedamount { set; get; }
        public string chargecode { set; get; }
        public string chargemessage { set; get; }
        public string status { set; get; }
        public string custname { set; get; }
        public string custemail { set; get; }
        public string custphone { set; get; }
        public string customerid { set; get; }
    }
}