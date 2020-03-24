using EmployerModules.DAL;
using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EmployerModules.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using System.Data;
using ClosedXML.Excel;
using System.IO;

namespace EmployerModules.Controllers
{
    [Authorize (Roles = "Admin")]
    public class AdminController : Controller
    {
        private PALSiteDBEntities db = new PALSiteDBEntities();

        // GET: Admin
        public ActionResult Index()
        {
            return View(db.ScheduleHeaders.ToList());
        }
        public FileResult Export(string employerCode)
        {
            var employerName = db.EmployerDetails.Where(s => s.Recno == employerCode).FirstOrDefault().EmployerName;
            var period = db.ScheduleUploads.Where(s => s.EmployerCode == employerCode && s.PFACode == "25").OrderByDescending(s => s.Period).FirstOrDefault().Period;
            var employeeList = db.ScheduleUploads.Where(s => s.EmployerCode == employerCode && s.PFACode == "25" && s.Period == period).ToList();
            var cashId= db.ScheduleHeaders.Where(s => s.EmployerId == employerCode && s.PFACode == "25" && s.SchedulePeriod==period).First().TransactionId;
            string PFAName = db.Pfas.Where(p => p.PFACode == "25").FirstOrDefault().Description;

            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[11] {
                                            new DataColumn("PIN"),
                                            new DataColumn("surname"),
                                            new DataColumn("firstname"),
                                            new DataColumn("othername"),
                                            new DataColumn("employer code"),
                                            new DataColumn("ee"),
                                            new DataColumn("er"),
                                            new DataColumn("vc"),
                                            new DataColumn("cont. Period"),
                                            new DataColumn("cash ID"),
                                            new DataColumn("Description"),
                                           
                                             });


            
            foreach (var employee in employeeList)
            {
                dt.Rows.Add(employee.Pin, employee.Surname, employee.FirstName,
                    employee.OtherName, employee.EmployerCode, employee.EmployeeContribution, employee.EmployerContribution,
                    employee.VoluntaryContribution, String.Format("{0:MM/dd/yyyy}", employee.Period), cashId, employerName + " " + String.Format("{0: MMMM yyyy}", employee.Period));
                
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
        public ActionResult Feedbacks()
        {
            return View(db.Feedbacks.OrderByDescending(f => f.DateUpdated).ToList());
        }
        public ActionResult CompletedFeedbacks()
        {
            return View(db.Feedbacks.Where(f => f.Status == "complete").OrderByDescending(f => f.DateUpdated).ToList());
        }
        public ActionResult InProgressFeedbacks()
        {
            return View(db.Feedbacks.Where(f => f.Status == "in-progress").OrderByDescending(f => f.DateUpdated).ToList());
        }
        public ActionResult PendingFeedbacks()
        {
            return View(db.Feedbacks.Where(f => f.Status == "pending").OrderByDescending(f => f.DateUpdated).ToList());
        }

        public ActionResult Reply(int Id)
        {
            var feedback = db.Feedbacks.Find(Id);
            feedback.Viewed = true;
            var checkRemarks = db.Remarks.Where(r => r.FeedbackId == Id && r.IsAdminRemark == false).OrderByDescending(r => r.Id).ToList();
            var remarks = db.Remarks.Where(r => r.FeedbackId == Id).OrderByDescending(r => r.Id).ToList();
            if (checkRemarks.Count > 0)
            {
                foreach (var remark in checkRemarks)
                {
                    remark.Viewed = true;
                }
            }
            db.SaveChanges();
            ViewBag.Remarks = remarks;
            return View(feedback);
        }
        [HttpPost]
        public ActionResult Reply(Feedback feeback, String Comment)
        {
            if(Comment == null)
            {
                ViewBag.ErrorMessage = "Oops, Error sending message. Please enter your Remark.";
                ModelState.AddModelError("", "Please enter your remarks");
            }

            var email = User.Identity.GetUserName();
            var getFeedBack = db.Feedbacks.Find(feeback.Id);
            getFeedBack.Remarks = Comment;
            getFeedBack.Modifiedby = email;
            getFeedBack.NewRemark = false;
            getFeedBack.Status = feeback.Status;
            //string dtNow = DateTime.Now.ToShortDateString();
            getFeedBack.DateUpdated = DateTime.Now;

            var remark = new Remark();

            remark.FeedbackId = getFeedBack.Id;
            remark.Comment = Comment;
            remark.CreatedDate = DateTime.Now;
            remark.IsAdminRemark = true;
            remark.EmployerCode = getFeedBack.EmployerCode;
            remark.Viewed = false;
            remark.FeedbackType = getFeedBack.FeedbackType;
            db.Remarks.Add(remark);
            
            db.SaveChanges();
            return RedirectToAction("Feedbacks");
        }

        // GET: Employers/SearchEmployer?employercode= PEN176383939
        public ActionResult SearchEmployer(string employerCode)
        {
            var emp = db.EmployerDetails.Where(c => c.Recno == employerCode).FirstOrDefault(); ;
            if (emp != null)
            {
                return RedirectToAction("Register", "Account", new { employerCode = employerCode });
            }

            if (employerCode != null && emp == null)
                ViewBag.ErrorMessage = "Employer does not exist";

            return View();
        }

        public ActionResult AddAdmin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddAdmin(string Email, string Password,  string ConfirmPassword)
        {
            var check = db.AspNetUsers.Where(u => u.Email == Email).FirstOrDefault();
            if(check != null)
            {
                ViewBag.ErrorMessage = " Email already Exist.";
                return View();
            }
            if (Password != ConfirmPassword)
            {
                ViewBag.ErrorMessage = "Oops, Password does not match.";
                return View();
            }
            ApplicationDbContext context = new Models.ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                        

            var user = new ApplicationUser();
            user.UserName = Email;
            user.Email = Email;

            string userPWD = Password;

             var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role Admin    
             if (chkUser.Succeeded)
             {
                var result1 = UserManager.AddToRole(user.Id, "Admin");
                UserManager.AddClaim(user.Id, new Claim(ClaimTypes.GivenName, Email));
                var newAdminUser = db.AspNetUsers.Find(user.Id);

                newAdminUser.EmailConfirmed = true;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Notifications()
        {
            var feedbacks = db.Feedbacks.Where(f =>( f.Viewed == false || f.Viewed == null|| f.NewRemark == true) ).OrderByDescending(f => f.DateUpdated).Count();
          
            return Json( feedbacks , JsonRequestBehavior.AllowGet);
        }

        public ActionResult NotificationsList()
        {
            var feedbacks = db.Feedbacks.Where(f => f.Viewed == false || f.Viewed == null || f.NewRemark==true).OrderByDescending(f => f.DateUpdated).ToList();
           
            return View(feedbacks);
        }

        public ActionResult EmployeeList(string pfa, string employerCode)
        {
            var employeeList = db.ScheduleUploads.Where(s => s.EmployerCode == employerCode && s.PFACode == pfa).ToList();
            return View(employeeList);
        }

        public ActionResult ValidateSchedule(int Id)
        {
            var recentSchedule = db.ScheduleHeaders.Find(Id);
            return View(recentSchedule);
        }

        [HttpPost]
        public ActionResult ValidateSchedule(int Id, string State)
        {
            var recentSchedule = db.ScheduleHeaders.Find(Id);
            recentSchedule.PaymentState = "closed";
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DownloadFile(string filePath, string fileName)
        {
            byte[] filebyte = System.IO.File.ReadAllBytes(filePath);
            return File(filebyte, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}