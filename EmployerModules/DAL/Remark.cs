//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EmployerModules.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Remark
    {
        public long Id { get; set; }
        public Nullable<int> FeedbackId { get; set; }
        public string Comment { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<bool> IsAdminRemark { get; set; }
    }
}
