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

        public ActionResult Feedbacks()
        {
            return View(db.Feedbacks.ToList());
        }
        public ActionResult CompletedFeedbacks()
        {
            return View(db.Feedbacks.Where(f => f.Status == "complete").ToList());
        }
        public ActionResult InProgressFeedbacks()
        {
            return View(db.Feedbacks.Where(f => f.Status == "in-progress").ToList());
        }
        public ActionResult PendingFeedbacks()
        {
            return View(db.Feedbacks.Where(f => f.Status == "pending").ToList());
        }

        public ActionResult Reply(int Id)
        {
            var feedback = db.Feedbacks.Find(Id);
            var remarks = db.Remarks.Where(r => r.FeedbackId == Id).OrderByDescending(r => r.Id).ToList();
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
            getFeedBack.Createdby = feeback.Createdby;
            //getFeedBack.Remarks = feeback.Remarks;
            getFeedBack.Modifiedby = email;
            getFeedBack.Status = feeback.Status;
            //string dtNow = DateTime.Now.ToShortDateString();
            getFeedBack.DateUpdated = DateTime.Now;

            var remark = new Remark();

            remark.FeedbackId = getFeedBack.Id;
            remark.Comment = Comment;
            remark.CreatedDate = DateTime.Now;
            remark.IsAdminRemark = true;
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

            }

            return RedirectToAction("Index");
        }
    }
}