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
    
    public partial class PaymentProve
    {
        public int Id { get; set; }
        public Nullable<int> SchedulerHeaderId { get; set; }
        public string FilePath { get; set; }
        public Nullable<System.DateTime> UploadedDate { get; set; }
        public string FileExt { get; set; }
    }
}