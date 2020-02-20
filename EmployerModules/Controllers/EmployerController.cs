﻿using EmployerModules.DAL;
using EmployerModules.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using System.Globalization;
using EmployerModules.ViewModel;
using Newtonsoft.Json;

using System.IO;
using System.Data;
using ClosedXML.Excel;
using System.Security.Claims;
using System.Configuration;
using Paystack.Net.SDK.Transactions;
using System.Threading.Tasks;

using System.Net.Http;
using System.Net.Http.Headers;


namespace EmployerModules.Controllers
{
    [Authorize(Roles = "EmpAdmin,Employer")]
    public class EmployerController : Controller
    {
        //private ApplicationDbContext db = new ApplicationDbContext();
        private PALSiteDBEntities db = new PALSiteDBEntities();

        // GET: Employers
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var emp = db.EmployerDetails.Where(c => c.Recno == employerCode).FirstOrDefault();
            var members = db.Employees.Where(e => e.Employer_Recno == employerCode).ToList().Count();
            var transactions = db.ScheduleHeaders.Where(e => e.EmployerId == employerCode).ToList();
            ViewBag.Transactions = transactions;
            ViewBag.Members = members;
            return View(emp);
        }


        // GET: Employers/Member
        public ActionResult Member()
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;

            var members = db.Employees.Where(e => e.Employer_Recno == employerCode && e.Pin.Contains("PEN")).ToList();
             
            return View(members);
        }


        // GET: Employers/MemberDetails/PEN1000004848
        [Route("Employer/MemberDetails/{pin}")]
        public ActionResult MemberDetails(string pin)
        {
            var employerDetails = db.Employees.Find(pin);
            return View(employerDetails);
        }

        // GET: Employers/Transaction

        public ActionResult Transaction(string pin)
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var transactions = db.SCHEDULES.Where(s => s.EMPLOYER_CODE == employerCode).Take(12);
            return View(transactions);
        }

        // GET: Employers/Verifypin?searchPin= PEN176383939
        public ActionResult Verifypin(string searchedPin)
        {
            var pin = db.Employees.Where(s => s.Pin.Contains(searchedPin) && s.Pin.Contains("PEN")).ToList();
            if(pin.Count == 0 && searchedPin != null)
                ViewBag.ErrorMessage = "Pin does not exist.";
            return View(pin);
        }

        // GET: Employers/Feedback
        public ActionResult Feedback()
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var feedbacks = db.Feedbacks.Where(f =>f.FeedbackType == "feedback"  && f.EmployerCode == employerCode).ToList();
            var complaints = db.Feedbacks.Where(f => f.FeedbackType == "complaints" && f.EmployerCode == employerCode).ToList();
            var enquiries = db.Feedbacks.Where(f => f.FeedbackType == "enquiry" && f.EmployerCode == employerCode).ToList();
            ViewBag.Feedbacks = feedbacks.Count;
            ViewBag.Complaints = complaints.Count;
            ViewBag.Enquiries = enquiries.Count;
            return View();
        }

        // GET: Employers/Feedback
        public ActionResult FeedbackForm(string Type)
        {
            return View();
        }

        [HttpPost]
        public ActionResult FeedbackForm(Feedback feedback)
        {
            var userId = User.Identity.GetUserId();
            var employer = db.AspNetUsers.Find(userId);
            string employerName = db.EmployerDetails.Where(e => e.Recno == employer.EmployerCode).FirstOrDefault().EmployerName;
            if (feedback.Message == null || feedback.Email == null)
            {
                ViewBag.ErrorMessage = "Oops, Error sending message. Please enter your email and message.";
                ModelState.AddModelError("", "Please enter your message and email");
            }
            if (ModelState.IsValid)
            {
                feedback.EmployerCode = employer.EmployerCode;
                feedback.FeedbackType = "feedback";
                feedback.Status = "pending";
                string dtNow = DateTime.Now.ToShortDateString();
                feedback.DateCreated = Convert.ToDateTime(dtNow);
                feedback.DateUpdated = Convert.ToDateTime(dtNow);
                feedback.Createdby = employerName;
                db.Feedbacks.Add(feedback);
                db.SaveChanges();
                ModelState.Clear();
                ViewBag.SuccessMessage = "Thanks for your feedback. We will get back to you soonest.";


                var msg = PalLibrary.Messaging.SendEmail("info@palpensions.com", "helpdesk@palpensions.com", $"Feedback ({feedback.EmployerCode}) Notification - {feedback.FeedbackType}", $@"

                    <p>Hello Info,
 
                    <p>Your client ({feedback.EmployerCode}) wrote at {DateTime.Now.ToString("MMM dd yyyy hh:mm:ss tt")}: <br />
                    <strong>Status:</strong> {feedback.Status}<br/>
                    <strong>Message:</strong> {feedback.Message}<br/></p>
                  
                <hr />   
                     
                <p>For a list of feedback,request or complaints, please visit https://pallite.palpensions.com/Staff/SupportLogs </p> 
   
            <p>         Regards<br/>
 
                    PAL Pensions</p>");


                //return View("Feedback");
                return RedirectToAction("FeedbacksList");
            }

            ViewBag.ErrorMessage = "Oops, Error sending message. Please enter your email and message.";
            return View("Feedback", feedback);
        }

        [HttpPost]
        public ActionResult EnquiryForm(Feedback feedback)
        {
            var userId = User.Identity.GetUserId();
            var employer = db.AspNetUsers.Find(userId);
            string employerName = db.EmployerDetails.Where(e => e.Recno == employer.EmployerCode).FirstOrDefault().EmployerName;
            if (feedback.Message == null || feedback.Email == null)
            {
                ViewBag.ErrorMessage = "Oops, Error sending message. Please enter your email and message.";
                ModelState.AddModelError("", "Please enter your message and email");
            }
            if (ModelState.IsValid)
            {
                feedback.EmployerCode = employer.EmployerCode;
                feedback.FeedbackType = "enquiry";
                feedback.Status = "pending";
                string dtNow = DateTime.Now.ToShortDateString();
                feedback.DateCreated = Convert.ToDateTime(dtNow);
                feedback.DateUpdated = Convert.ToDateTime(dtNow);
                feedback.Createdby = employerName;
                db.Feedbacks.Add(feedback);
                db.SaveChanges();
                ModelState.Clear();
                ViewBag.SuccessMessage = "Thanks for your enquiry. We will get back to you soonest.";
                //return View("Feedback");
                var msg = PalLibrary.Messaging.SendEmail("info@palpensions.com", "helpdesk@palpensions.com", $"Feedback ({feedback.EmployerCode}) Notification - {feedback.FeedbackType}", $@"

                    <p>Hello Info,
 
                    <p>Your client ({feedback.EmployerCode}) wrote at {DateTime.Now.ToString("MMM dd yyyy hh:mm:ss tt")}: <br />
                    <strong>Status:</strong> {feedback.Status}<br/>
                    <strong>Message:</strong> {feedback.Message}<br/></p>
                  
                    <hr />   
                     
                    <p>For a list of feedback,request or complaints, please visit https://pallite.palpensions.com/Staff/SupportLogs </p> 
   
                    <p>Regards<br/>
 
                    PAL Pensions</p>");
                return RedirectToAction("EnquiryList");
            }

            ViewBag.ErrorMessage = "Oops, Error sending message. Please enter your email and message.";
            return View("Feedback", feedback);
        }

        [HttpPost]
        public ActionResult ComplaintsForm(Feedback feedback)
        {
            var userId = User.Identity.GetUserId();
            var employer = db.AspNetUsers.Find(userId);
            string employerName = db.EmployerDetails.Where(e => e.Recno == employer.EmployerCode).FirstOrDefault().EmployerName;
            
            if (feedback.Message == null || feedback.Email == null)
            {
                ViewBag.ErrorMessage = "Oops, Error sending message. Please enter your email and message.";
                ModelState.AddModelError("", "Please enter your message and email");
            }
            if (ModelState.IsValid)
            {
                feedback.EmployerCode = employer.EmployerCode;
                feedback.FeedbackType = "complaints";
                feedback.Status = "pending";
                string dtNow = DateTime.Now.ToShortDateString();
                feedback.DateCreated = Convert.ToDateTime(dtNow);
                feedback.DateUpdated = Convert.ToDateTime(dtNow);
                feedback.Createdby = employerName;
                db.Feedbacks.Add(feedback);
                db.SaveChanges();
                ModelState.Clear();
                ViewBag.SuccessMessage = "Thanks for taking your time to contact us. We will get back to you soonest.";
                var msg = PalLibrary.Messaging.SendEmail("info@palpensions.com", "helpdesk@palpensions.com", $"Feedback ({feedback.EmployerCode}) Notification - {feedback.FeedbackType}", $@"

                    <p>Hello Info,
 
                    <p>Your client ({feedback.EmployerCode}) wrote at {DateTime.Now.ToString("MMM dd yyyy hh:mm:ss tt")}: <br />
                    <strong>Status:</strong> {feedback.Status}<br/>
                    <strong>Message:</strong> {feedback.Message}<br/></p>
                  
                    <hr />   
                     
                    <p>For a list of feedback,request or complaints, please visit https://pallite.palpensions.com/Staff/SupportLogs </p> 
   
                    <p>Regards<br/>
 
                    PAL Pensions</p>");
                //return View("Feedback");
                return RedirectToAction("ComplaintsList");
            }

            ViewBag.ErrorMessage = "Oops, Error sending message. Please enter your email and message.";
            return View("Feedback", feedback);
        }

        public ActionResult FeedbacksList()
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var feedbacks = db.Feedbacks.Where(f => f.FeedbackType == "feedback" && f.EmployerCode == employerCode).ToList();
            return View(feedbacks);
        }

        public ActionResult ComplaintsList()
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var complaints = db.Feedbacks.Where(f => f.FeedbackType == "complaints" && f.EmployerCode == employerCode).ToList();
            return View(complaints);
        }

        public ActionResult EnquiryList()
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var enquiries = db.Feedbacks.Where(f => f.FeedbackType == "enquiry" && f.EmployerCode == employerCode).ToList();
            return View(enquiries);
        }

        public ActionResult ViewMessage(int Id)
        {
            var feedbackDetails = db.Feedbacks.Find(Id);
            var remarks = db.Remarks.Where(r => r.FeedbackId == Id).OrderByDescending(r => r.Id).ToList();
            ViewBag.Remarks = remarks;
            return View(feedbackDetails);
        }

        [HttpPost]
        public ActionResult ViewMessage(int Id, string Comment)
        {
            if (Comment == null)
            {
                ViewBag.ErrorMessage = "Oops, Error sending message. Please enter your Remark.";
                ModelState.AddModelError("", "Please enter your remarks");
            }

            var getFeedback  = db.Feedbacks.Find(Id);

            var remark = new Remark();

            remark.FeedbackId = getFeedback.Id;
            remark.Comment = Comment;
            remark.CreatedDate = DateTime.Now;
            remark.IsAdminRemark = false;
            db.Remarks.Add(remark);

            db.SaveChanges();
            return RedirectToAction("ViewMessage");
        }

        public ActionResult UploadSchedule()
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var latestSchedule = db.ScheduleHeaderTemps.Where(s => s.EmployerId == employerCode).ToList();
           
            return View(latestSchedule);
        }

        [HttpPost]
        public ActionResult UploadSchedule(FormCollection formCollection, string total, string u_period)
        {
            decimal submitedTotal =  0;
            if(total != "")
            {
                submitedTotal = Convert.ToDecimal(total);
            }
            
            string submitPeriod = formCollection["period"];
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            decimal totalAmount = 0;
            string period = "";
            string expirydate = "";
            var uploadList = new List<ScheduleUploadTemp>();

            var schedules = db.ScheduleHeaderTemps.Where(s => s.EmployerId == employerCode).Take(12);

            if(u_period == "")
            {
                ViewBag.ErrorMessage = "Please enter the period field";
                return View(schedules);
            }

            HttpPostedFileBase file = Request.Files["UploadedFile"];
            if ((file == null) || (file.ContentLength == 0))
                {
                    ViewBag.ErrorMessage = "Please select an Excel file in order to upload." ;
                    return View(schedules);
                }

                else
                {
                    if (file.FileName.EndsWith("xls") || file.FileName.EndsWith("xlsx"))
                    {
                        
                        var members = db.Employees.Where(e => e.Employer_Recno == employerCode && e.Pin.Contains("PEN")).ToList();

                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                        //Enter in Console "Install-Package EPPlus"
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            for (int rowIterator = 1; rowIterator <= noOfRow; rowIterator++)
                            {
                                if(rowIterator == 1)
                                {
                                    employerCode = (string)workSheet.Cells[rowIterator, 7].Value;
                                }
                                if (rowIterator == 2)
                                {
                                    totalAmount = Convert.ToDecimal(workSheet.Cells[rowIterator, 2].Value.ToString());
                                }

                                if (rowIterator == 4)
                                {
                                    string periodMonth = workSheet.Cells[rowIterator, 2].Value.ToString().ToLower();
                                    string periodYear = workSheet.Cells[rowIterator, 7].Value.ToString();
                                    string monthNumber = "";
                                    switch (periodMonth)
                                    {
                                        case "january":
                                            period = "1/01/" + periodYear;
                                            monthNumber = "01";
                                            break;

                                        case "february":
                                            period = "1/02/" + periodYear;
                                            monthNumber = "02";
                                            break;

                                        case "march":
                                            period = "1/03/" + periodYear;
                                            monthNumber = "03";
                                            break;

                                        case "april":
                                            period = "1/04/" + periodYear;
                                            monthNumber = "04";
                                            break;
                                        case "may":
                                            period = "1/05/" + periodYear;
                                            monthNumber = "05";
                                            break;
                                        case "june":
                                            period = "1/06/" + periodYear;
                                            monthNumber = "06";
                                            break;
                                        case "july":
                                            period = "1/07/" + periodYear;
                                            monthNumber = "07";
                                            break;
                                        case "august":
                                            period = "1/08/" + periodYear;
                                            monthNumber = "08";
                                            break;
                                        case "september":
                                            period = "1/09/" + periodYear;
                                            monthNumber = "08";
                                            break;
                                        case "october":
                                            period = "1/10/" + periodYear;
                                            monthNumber = "10";
                                            break;
                                        case "november":
                                            period = "1/11/" + periodYear;
                                            monthNumber = "11";
                                            break;
                                        case "december":
                                            period = "1/12/" + periodYear;
                                            monthNumber = "12";
                                            break;
                                        default:
                                        break;
                                    }
                                    
                                    // Check if the uploaded period is equal to inputted period
                                    if (u_period.Substring(u_period.Length - 2) != monthNumber || u_period.Substring(0, 4) != periodYear)
                                    {
                                        ViewBag.ErrorMessage = "The uploaded period is not equal to the entered period. Please check again.";
                                        return View(schedules);
                                    }
                                    
                                }

                            if (rowIterator == 3 )
                            {
                                //7035796.88
                                string uploadedValuedate = workSheet.Cells[rowIterator, 2].Value.ToString();
                                expirydate =workSheet.Cells[rowIterator, 2].Value.ToString();
                            }

                            if ( rowIterator == 5 || rowIterator == 6)
                            {
                                continue;
                            }

                            if(rowIterator >=7 )
                            {
                                if (workSheet.Cells[rowIterator, 2].Value == null && workSheet.Cells[rowIterator, 3].Value == null && workSheet.Cells[rowIterator, 4].Value == null)
                                {
                                    break;
                                }
                            
                                var schedule = new ScheduleUploadTemp();
                                    schedule.EmployerCode = employerCode;
                                    DateTime dateValue;
                                    //string period = (string) workSheet.Cells[rowIterator, 1].Value;
                                    DateTime.TryParse(period, out dateValue);
                                    schedule.Period = dateValue;
                                    schedule.Pin = (string) workSheet.Cells[rowIterator, 2].Value;
                                    schedule.Surname = (string) workSheet.Cells[rowIterator, 3].Value;
                                    schedule.FirstName = (string) workSheet.Cells[rowIterator, 4].Value;
                                    schedule.OtherName = (string) workSheet.Cells[rowIterator, 5].Value;
                                    
                                    schedule.EmployeeContribution = Convert.ToDecimal( workSheet.Cells[rowIterator, 6].Value.ToString());
                                    schedule.EmployerContribution = Convert.ToDecimal( workSheet.Cells[rowIterator, 7].Value.ToString());
                                    schedule.VoluntaryContribution = Convert.ToDecimal(workSheet.Cells[rowIterator, 8].Value.ToString()) + Convert.ToDecimal(workSheet.Cells[rowIterator, 9].Value.ToString());
                                    schedule.TotalContribution = Convert.ToDecimal(workSheet.Cells[rowIterator, 10].Value.ToString());
                                    schedule.PFACode = (string) workSheet.Cells[rowIterator, 11].Value.ToString();
                                    schedule.CreatedBy = employerCode;
                                    string dtNow = DateTime.Now.ToShortDateString();
                                    schedule.CreatedOn = Convert.ToDateTime(dtNow);
                                    schedule.PinValid = false;
                                    if (members.Any(m => m.Pin == schedule.Pin))
                                    {
                                        schedule.PinValid = true;
                                    }

                                    uploadList.Add(schedule);
                                    
                                }
                            }
                        }

                        decimal TotalPFATotalContribution = (decimal)uploadList.Sum(x => x.TotalContribution);

                        if (submitedTotal != TotalPFATotalContribution)
                        {
                        ViewBag.ErrorMessage = "Sum of the Total Contribution of the uploaded excel is not equal to the total entered." +
                        "It has a difference of " + (TotalPFATotalContribution - submitedTotal).ToString() + "NGR. Please check again and resend.";
                            return View(schedules);
                        }
                        else
                        {
                            foreach (var item in uploadList)
                            {
                                db.ScheduleUploadTemps.Add(item);

                            }
                            db.SaveChanges();

                            var latestSchedule = db.ScheduleUploadTemps.Where(s => s.EmployerCode == employerCode).ToList();
                            //var PFAList =totalSchedules.GroupBy(p => p.PFACode).Select(p => p.First());
                            var PFAList = db.Pfas.ToList();

                        
                            foreach (var PFA in PFAList)
                            {

                                var groupSchedules = latestSchedule.Where(s => s.EmployerCode == employerCode && s.PFACode == PFA.PFACode).ToList();
        
                                if (groupSchedules.Count > 0)
                                {
                                    var scheduleSummary = new ScheduleHeaderTemp();
                                    scheduleSummary.TotalAmount = (decimal) groupSchedules.Sum(x => x.TotalContribution);
                                    scheduleSummary.TotalEmployee = groupSchedules.Count;
                                    scheduleSummary.PFA = PFA.Description;
                                    scheduleSummary.PFACode = PFA.PFACode;
                                    scheduleSummary.PaymentState = "open";
                                    scheduleSummary.PaymentStatus = "unpaid";
                                    //scheduleSummary.ExpiryDate = Convert.ToDateTime(expirydate);
                                    DateTime dateValue;
                                    DateTime.TryParse(period, out dateValue);
                                    scheduleSummary.SchedulePeriod = dateValue;
                                    string dtNow = DateTime.Now.ToShortDateString();
                                    //scheduleSummary.UploadAdded = Convert.ToDateTime(dtNow);
                                    scheduleSummary.UploadAdded = DateTime.Now;
                                    scheduleSummary.EmployerId = employerCode;
                                    db.ScheduleHeaderTemps.Add(scheduleSummary);
                                    db.SaveChanges();
                                 
                                }
                            }

                            db.SaveChanges();
                        
                            var latestSummary = db.ScheduleHeaderTemps.Where(s => s.EmployerId == employerCode).ToList();

                            ViewBag.Success = "Excel Uploaded Successfully";
                            ViewBag.UploadList = uploadList;
                            return View(latestSummary);
                        }
                        
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "The file extension must be excel ie .xls or .xlsx extension";
                        return View(schedules);
                    }
                }
           
        }

        public ActionResult RecentTransaction()
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var latestSummary = db.ScheduleHeaders.Where(s => s.EmployerId == employerCode).OrderByDescending(m=> m.PaymentDate).ToList();
            return View(latestSummary);
        }

        public ActionResult DownloadFile()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Content/template/";
            byte[] filebyte = System.IO.File.ReadAllBytes(path + "schedulesTemplate.xlsx");
            string fileName = "schedulesTemplate.xlsx";
            return File(filebyte, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult ClearUpload()
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var schedules = db.ScheduleUploadTemps.Where(s => s.EmployerCode == employerCode).ToList();
            var scheduleSummary = db.ScheduleHeaderTemps.Where(s => s.EmployerId == employerCode).ToList();
            foreach (var schedule in schedules)
            {
                db.ScheduleUploadTemps.Remove(schedule);
            }

            foreach (var schedule in scheduleSummary)
            {
                db.ScheduleHeaderTemps.Remove(schedule);
            }

            db.SaveChanges();
            return RedirectToAction("UploadSchedule");
        }

        public ActionResult PaymentSummary()
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;

            var totalSchedules = db.ScheduleUploadTemps.Where(s => s.EmployerCode == employerCode).ToList();
            //var PFAList =totalSchedules.GroupBy(p => p.PFACode).Select(p => p.First());
            var PFAList = db.Pfas.ToList();

            var pfaModel = new PFAList();

            
            foreach (var PFA in PFAList)
            {
                var schedules = db.ScheduleUploadTemps.Where(s => s.EmployerCode == employerCode && s.PFACode == PFA.PFACode).ToList();
                if(schedules != null)
                {
                    var pfaCode = PFA.PFACode;
                    switch (pfaCode)
                    {
                        case "26":
                            pfaModel.ARM = schedules;
                            break;
                        case "33":
                            pfaModel.AIICO = schedules;
                            break;
                        case "37":
                            pfaModel.APT = schedules;
                            break;
                        case "32":
                            pfaModel.CrusaderSterling = schedules;
                            break;
                        case "43":
                            pfaModel.Fidelity = schedules;
                            break;
                        case "29":
                            pfaModel.FirstGuarantee = schedules;
                            break;
                        case "40":
                            pfaModel.InvestmentOne = schedules;
                            break;
                        case "36":
                            pfaModel.IEIAnchor = schedules;
                            break;
                        case "23":
                            pfaModel.Leadway = schedules;
                            break;
                        case "31":
                            pfaModel.NPLC = schedules;
                            break;
                        case "47":
                            pfaModel.NPF = schedules;
                            break;
                        case "34":
                            pfaModel.OAK = schedules;
                            break;
                        case "25":
                            pfaModel.PAL = schedules;
                            break;
                        case "22":
                            pfaModel.Premium = schedules;
                            break;
                        case "24":
                            pfaModel.Sigma = schedules;
                            break;
                        case "21":
                            pfaModel.StanbicIBTC = schedules;
                            break;
                        case "28":
                            pfaModel.TrustFund = schedules;
                            break;
                        case "35":
                            pfaModel.AXA = schedules;
                            break;
                        case "30":
                            pfaModel.FCMB = schedules;
                            break;
                        case "49":
                            pfaModel.NigeriaUniversity = schedules;
                            break;
                        case "46":
                            pfaModel.Radix = schedules;
                            break;
                        case "42":
                            pfaModel.Veritias = schedules;
                            break;
                       
                        default:
                           
                            break;
                    }
                }
                
            }
            pfaModel.Total = totalSchedules;
            return View(pfaModel);
        }

        public ActionResult EmployeeList(string pfa)
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var employeeList = db.ScheduleUploadTemps.Where(s => s.EmployerCode == employerCode && s.PFACode == pfa).ToList();
           
            return View(employeeList);
        }

        public ActionResult PaidEmployeeList(string pfa)
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var employeeList = db.ScheduleUploads.Where(s => s.EmployerCode == employerCode && s.PFACode == pfa).ToList();

            return View(employeeList);
        }

        [AllowAnonymous]
        public FileResult Export(string pfa)
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var employeeList = db.ScheduleUploadTemps.Where(s => s.EmployerCode == employerCode && s.PFACode == pfa).ToList();
            string PFAName = db.Pfas.Where(p => p.PFACode == pfa).FirstOrDefault().Description;

            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[12] {
                                             new DataColumn("S/N"),
                                            new DataColumn("Period"),
                                            new DataColumn("Pin"),
                                            new DataColumn("PinValid"),
                                            new DataColumn("Surname"),
                                            new DataColumn("FirstName"),
                                            new DataColumn("OtherName"),
                                            new DataColumn("PFACode"),
                                            new DataColumn("EmployeeContribution"),
                                            new DataColumn("EmployerContribution"),
                                            new DataColumn("VoluntaryContribution"),
                                            new DataColumn("TotalContribution"),
                                             });


            int i = 1;
            foreach (var employee in employeeList)
            {
                dt.Rows.Add(i,employee.Period, employee.Pin, employee.PinValid, 
                    employee.Surname, employee.FirstName, employee.OtherName, employee.PFACode, 
                    employee.EmployeeContribution, employee.EmployerContribution, employee.VoluntaryContribution,
                    employee.TotalContribution);
                i++;
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", PFAName + "_EmployeeList.xlsx");
                }
            }
        }

        public ActionResult Payment()
        {
            var userId = User.Identity.GetUserId();
            var emp = db.AspNetUsers.Find(userId);
            string empName = db.EmployerDetails.Where(e => e.Recno == emp.EmployerCode).FirstOrDefault().EmployerName;
            var totalSchedules = db.ScheduleUploadTemps.Where(s => s.EmployerCode == emp.EmployerCode).ToList();
            ViewBag.Paystack_Pk = "pk_test_e0d7d4d6fd56f59e254a03634d3e2970bcf78933";
            ViewBag.Paystack_Sk = "sk_test_283ffe5211516393d9812e240e6c86110ef7fd89";

            Random random = new Random();
            int randNum = random.Next(100000, 999999);
            ViewBag.Flutterwave_Pk = "FLWPUBK_TEST-8ad783b86d69923e8937667de0a86cbc-X";
            ViewBag.Flutterwave_Sk = "FLWSECK_TEST-bd7e15166caee0fab2b2e735d6986d18-X";
            ViewBag.FlutterwaveRefId = "PAL" + DateTime.Now.ToString("ddMMyyyy") + randNum.ToString();

            ViewBag.TotalPayment = totalSchedules.Sum(t => t.TotalContribution);
            ViewBag.EmployerId = emp.EmployerCode;
            ViewBag.EmployerEmail = emp.Email;
            ViewBag.EmployerName = empName;
            ViewBag.PhoneNo = emp.PhoneNumber;
            return View();
        }
        
        public ActionResult PaymentConfirmed(string paymentType)
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var employerDetail = db.EmployerDetails.Where(e => e.Recno == employerCode).FirstOrDefault();
            var uploadedSchedules = db.ScheduleUploadTemps.Where(s => s.EmployerCode == employerCode).ToList();
            var period = uploadedSchedules.FirstOrDefault().Period;
            var TotalPaidAmount =uploadedSchedules.Sum(t => t.TotalContribution);
            var scheduleHeader = db.ScheduleHeaderTemps.Where(s => s.EmployerId == employerCode).ToList();
            

            foreach (var schedule in uploadedSchedules)
            {
                var moveSchedule = new ScheduleUpload();
                moveSchedule.EmployerCode = schedule.EmployerCode;
                moveSchedule.Period = schedule.Period;
                moveSchedule.Pin = schedule.Pin;
                moveSchedule.Surname = schedule.Surname;
                moveSchedule.FirstName = schedule.FirstName;
                moveSchedule.OtherName = schedule.OtherName;
                moveSchedule.EmployeeContribution = schedule.EmployeeContribution;
                moveSchedule.EmployerContribution = schedule.EmployerContribution;
                moveSchedule.VoluntaryContribution = schedule.VoluntaryContribution;
                moveSchedule.TotalContribution = schedule.TotalContribution;
                moveSchedule.FileName = schedule.FileName;
                moveSchedule.PinValid = schedule.PinValid;
                moveSchedule.PaymentType = paymentType;
                moveSchedule.CreatedOn = schedule.CreatedOn;
                moveSchedule.CreatedBy = schedule.CreatedBy;
                //moveSchedule.PaymentId = 67;  // Payment Id to be generated
                moveSchedule.PFACode = schedule.PFACode;
                db.ScheduleUploads.Add(moveSchedule);
                db.ScheduleUploadTemps.Remove(schedule);
            }

            Random random = new Random();
            foreach (var header in scheduleHeader)
            {
                int randNum = random.Next(100000, 999999);

                var moveHeader = new ScheduleHeader();
                moveHeader.PFA = header.PFA;
                moveHeader.TotalAmount = header.TotalAmount;
                moveHeader.TotalEmployee = header.TotalEmployee;               
                moveHeader.TransactionId = "TR" + DateTime.Now.ToString("ddMMyyyy") + randNum.ToString();
                moveHeader.PaymentStatus = "paid";
                moveHeader.PaymentState = "closed";
                moveHeader.SchedulePeriod = header.SchedulePeriod;
                string dtNow = DateTime.Now.ToShortDateString();
                moveHeader.PaymentDate = DateTime.Now;
                dtNow = DateTime.Now.AddDays(30).ToShortDateString();
                moveHeader.ExpiryDate = Convert.ToDateTime(dtNow);
                moveHeader.UploadAdded = header.UploadAdded;
                moveHeader.EmployerId = header.EmployerId;
                moveHeader.PFACode = header.PFACode;
                db.ScheduleHeaders.Add(moveHeader);
                db.ScheduleHeaderTemps.Remove(header);
            }
            db.SaveChanges();

            var msg = PalLibrary.Messaging.SendEmail("info@palpensions.com", "helpdesk@palpensions.com", $"A payment of N{TotalPaidAmount.ToString()} was made by {employerDetail.EmployerName} - {employerCode}", $@"

                    <p>Hello Info,
 
                    <p>Your client {employerDetail.EmployerName} with Employer Code {employerCode} made payment of N{TotalPaidAmount.ToString()} 
                        on {DateTime.Now.ToString("MMM dd yyyy hh:mm:ss tt")}: <br />
                    <strong>Period:</strong> {period.ToString()}<br/>
                    
                  
                <hr />   
                     
                <p>For a list of feedback,request or complaints, please visit https://pallite.palpensions.com/Staff/SupportLogs </p> 
   
            <p>         Regards<br/>
 
                    PAL Pensions</p>");

           // return Redirect("RecentTransaction");
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PaymentServerConfirmed(string paymentType)
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var employerDetail = db.EmployerDetails.Where(e => e.Recno == employerCode).FirstOrDefault();
            var uploadedSchedules = db.ScheduleUploadTemps.Where(s => s.EmployerCode == employerCode).ToList();
            var period = uploadedSchedules.FirstOrDefault().Period;
            var TotalPaidAmount = uploadedSchedules.Sum(t => t.TotalContribution);
            var scheduleHeader = db.ScheduleHeaderTemps.Where(s => s.EmployerId == employerCode).ToList();


            foreach (var schedule in uploadedSchedules)
            {
                var moveSchedule = new ScheduleUpload();
                moveSchedule.EmployerCode = schedule.EmployerCode;
                moveSchedule.Period = schedule.Period;
                moveSchedule.Pin = schedule.Pin;
                moveSchedule.Surname = schedule.Surname;
                moveSchedule.FirstName = schedule.FirstName;
                moveSchedule.OtherName = schedule.OtherName;
                moveSchedule.EmployeeContribution = schedule.EmployeeContribution;
                moveSchedule.EmployerContribution = schedule.EmployerContribution;
                moveSchedule.VoluntaryContribution = schedule.VoluntaryContribution;
                moveSchedule.TotalContribution = schedule.TotalContribution;
                moveSchedule.FileName = schedule.FileName;
                moveSchedule.PinValid = schedule.PinValid;
                moveSchedule.PaymentType = paymentType;
                moveSchedule.CreatedOn = schedule.CreatedOn;
                moveSchedule.CreatedBy = schedule.CreatedBy;
                //moveSchedule.PaymentId = 67;  // Payment Id to be generated
                moveSchedule.PFACode = schedule.PFACode;
                db.ScheduleUploads.Add(moveSchedule);
                db.ScheduleUploadTemps.Remove(schedule);
            }

            Random random = new Random();
            foreach (var header in scheduleHeader)
            {
                int randNum = random.Next(100000, 999999);

                var moveHeader = new ScheduleHeader();
                moveHeader.PFA = header.PFA;
                moveHeader.TotalAmount = header.TotalAmount;
                moveHeader.TotalEmployee = header.TotalEmployee;
                moveHeader.TransactionId = "TR" + DateTime.Now.ToString("ddMMyyyy") + randNum.ToString();
                moveHeader.PaymentStatus = "paid";
                moveHeader.PaymentState = "closed";
                moveHeader.SchedulePeriod = header.SchedulePeriod;
                string dtNow = DateTime.Now.ToShortDateString();
                moveHeader.PaymentDate = DateTime.Now;
                dtNow = DateTime.Now.AddDays(30).ToShortDateString();
                moveHeader.ExpiryDate = Convert.ToDateTime(dtNow);
                moveHeader.UploadAdded = header.UploadAdded;
                moveHeader.EmployerId = header.EmployerId;
                moveHeader.PFACode = header.PFACode;
                db.ScheduleHeaders.Add(moveHeader);
                db.ScheduleHeaderTemps.Remove(header);
            }
            db.SaveChanges();

            var msg = PalLibrary.Messaging.SendEmail("info@palpensions.com", "helpdesk@palpensions.com", $"A payment of N{TotalPaidAmount.ToString()} was made by {employerDetail.EmployerName} - {employerCode}", $@"

                    <p>Hello Info,
 
                    <p>Your client {employerDetail.EmployerName} with Employer Code {employerCode} made payment of N{TotalPaidAmount.ToString()} 
                        on {DateTime.Now.ToString("MMM dd yyyy hh:mm:ss tt")}: <br />
                    <strong>Period:</strong> {period.ToString()}<br/>
                    
                  
                <hr />   
                     
                <p>For a list of feedback,request or complaints, please visit https://pallite.palpensions.com/Staff/SupportLogs </p> 
   
            <p>         Regards<br/>
 
                    PAL Pensions</p>");

            return Redirect("RecentTransaction");
            //return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddEmployer( )
        {
            var identity = (ClaimsIdentity)User.Identity;
            var name = identity.FindFirstValue(ClaimTypes.GivenName);
            var userId = User.Identity.GetUserId();
            var emp = db.AspNetUsers.Find(userId);
            ViewBag.EmployerCode = emp.EmployerCode;
            ViewBag.EmployerName = name;
            return View();
        }

        public ActionResult AllEmployerUsers()
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var users = db.AspNetUsers.Where(u => u.EmployerCode == employerCode).ToList();
            return View(users);
        }

        public async Task<JsonResult> InitializePayment(PaystackEmployerModel model)
        {
            string secretKey = ConfigurationManager.AppSettings["PaystackSecret"];
            var paystackTransactionAPI = new PaystackTransaction(secretKey);
            var response = await paystackTransactionAPI.InitializeTransaction(model.email, model.amount, model.EmployerName, model.EmployerId, "http://localhost:61383/Employer/VerifyTransaction");
            //Note that callback url is optional
            if (response.status == true)
            {
                return Json(new { error = false, result = response }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { error = true, result = response }, JsonRequestBehavior.AllowGet);

        }

        public async Task<ActionResult> VerifyTransaction()
        {
            string secretKey = ConfigurationManager.AppSettings["PaystackSecret"];
            var paystackTransactionAPI = new PaystackTransaction(secretKey);
            var tranxRef = HttpContext.Request.QueryString["reference"];
            if (tranxRef != null)
            {
                var response = await paystackTransactionAPI.VerifyTransaction(tranxRef);
                if (response.status)
                {
                    return RedirectToAction("PaymentServerConfirmed", "Employer", new { paymentType = "Paystack"});
                    //return View(response);
                }
            }

            return View("PaymentError");
        }

        public ActionResult Validate(string TransactionId)
        {
            var userId = User.Identity.GetUserId();
            string employerCode = db.AspNetUsers.Find(userId).EmployerCode;
            var uploadedSchedules = db.ScheduleUploadTemps.Where(s => s.EmployerCode == employerCode).ToList();
            var amount = uploadedSchedules.Sum(t => t.TotalContribution);

            var data = new { txref = TransactionId, SECKEY = "FLWSECK_TEST-bd7e15166caee0fab2b2e735d6986d18-X", include_payment_entity = 1 };
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var responseMessage = client.PutAsJsonAsync("https://ravesandboxapi.flutterwave.com/flwv3-pug/getpaidx/api/xrequery", data).Result;

            var responseStr = responseMessage.Content.ReadAsStringAsync().Result;
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseData>(responseStr);

            if (response.data.status == "successful" && response.data.amount == amount.ToString() && response.data.chargecode == "00")
            {
                return RedirectToAction("PaymentServerConfirmed", "Employer", new { paymentType = "Flutterwave" });
            }
            return RedirectToAction("PaymentSummary", "Employer");
        }
    }
    
}