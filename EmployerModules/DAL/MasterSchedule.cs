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
    using System.ComponentModel.DataAnnotations;

    public partial class MasterSchedule
    {
        public long Id { get; set; }
        [Display(Name = "Employer Code")]
        public string EmployerCode { get; set; }
        public Nullable<System.DateTime> Period { get; set; }
        public string Pin { get; set; }
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; }
        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string OtherName { get; set; }
        [Display(Name = "Employee Contribution")]
        public Nullable<decimal> EmployeeContribution { get; set; }
        [Display(Name = "Employer Contribution")]
        public Nullable<decimal> EmployerContribution { get; set; }
        [Display(Name = "Voluntary Contribution")]
        public Nullable<decimal> VoluntaryContribution { get; set; }
        [Display(Name = "Total Contribution")]
        public Nullable<decimal> TotalContribution { get; set; }
        public string FileName { get; set; }
        [Display(Name = "PFACode")]
        public Nullable<bool> PinValid { get; set; }
        [Display(Name = "Payment Type")]
        public string PaymentType { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<long> PaymentId { get; set; }
        [Display(Name = "PFA Code")]
        public string PFACode { get; set; }
    }
}
