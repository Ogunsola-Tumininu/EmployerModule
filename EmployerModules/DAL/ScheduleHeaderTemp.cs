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
    
    public partial class ScheduleHeaderTemp
    {
        public int Id { get; set; }
        public string PFA { get; set; }
        public Nullable<decimal> TotalAmount { get; set; }
        public Nullable<int> TotalEmployee { get; set; }
        public string TransactionId { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentState { get; set; }
        public Nullable<System.DateTime> SchedulePeriod { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.DateTime> Paymentdate { get; set; }
        public Nullable<System.DateTime> UploadAdded { get; set; }
        public string EmployerId { get; set; }
        public string PFACode { get; set; }
    }
}
