using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Donor.Models;

namespace Donor.Controllers
{
    public class DonorDetailsController : Controller
    {
        private DonorEntities db = new DonorEntities();

        // GET: DonorDetails
        [Authorize]
        public ActionResult ReadOnly()
        {
            return View();
        }
        public ActionResult Index()
        {
            return PartialView();
        }

        // GET: DonorDetails/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DonorDetail donorDetail = db.DonorDetails.Find(id);
            if (donorDetail == null)
            {
                return HttpNotFound();
            }
            return View(donorDetail);
        }

        // GET: DonorDetails/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: DonorDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude  = "IsHospitalAuthorized")] DonorDetail donorDetail)
        {
            bool Status = false;
            string message = "";

            if (ModelState.IsValid)
            {
                donorDetail.IsHospitalAuthorized = false;
                var isExist = IsEmailExist(donorDetail.DonorEmail);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "You are already an donor");
                    return View(donorDetail);
                }
                else
                {
                    db.DonorDetails.Add(donorDetail);
                    db.SaveChanges();
                    sendEmail(donorDetail.DonorEmail);
                    message = "Registration successfully done for LifeLinker please complete the check up process in any one of the hospital registered under lifelinker.You will receive an Email about the checkup " +
                       " has been sent to your email id:" + donorDetail.DonorEmail;
                    Status = true;
                }
                
                
                
            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(donorDetail);
        }

        // GET: DonorDetails/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DonorDetail donorDetail = db.DonorDetails.Find(id);
            if (donorDetail == null)
            {
                return HttpNotFound();
            }
            return View(donorDetail);
        }

        // POST: DonorDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
       
        public ActionResult Edit([Bind(Include = "DonorId,DonorName,DonorAddress,DonorEmail,DonorMobile,DonorGender,OrganName,IsHospitalAuthorized")] DonorDetail donorDetail)
        {
            
                db.Entry(donorDetail).State = EntityState.Modified;
                db.SaveChanges();
            return RedirectToAction("Index");
            
            
        }

        // GET: DonorDetails/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    DonorDetail donorDetail = db.DonorDetails.Find(id);
        //    if (donorDetail == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(donorDetail);
        //}

        // POST: DonorDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        
        public ActionResult DeleteConfirmed(int id)
        {
            DonorDetail donorDetail = db.DonorDetails.Find(id);
            db.DonorDetails.Remove(donorDetail);
            db.SaveChanges();
            return Json(new { success = true, message = "Deleted Successfully" ,JsonRequestBehavior.AllowGet});
        }

        public ActionResult GetData()
        {
            List<DonorDetail> donorList = db.DonorDetails.ToList<DonorDetail>();
            return Json(new { data = donorList }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDataForUsersView()
        {
            
            return Json(new { data = db.DonorDetails.Where(a=>(Boolean)a.IsHospitalAuthorized==true) }, JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (DonorEntities dc = new DonorEntities())
            {
                var v = dc.DonorDetails.Where(a => a.DonorEmail == emailID).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void sendEmail(string emailID)
        {
            var verifyUrl = "/CheckUps/Create";
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("organdonante@gmail.com", "Organ Donante");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "helloworld123#"; // Replace with actual password
            string subject = "";
            string body = "";
            
                subject = "Hurray You have Successfully Registered Life Linker";

                body = "<br/><br/>We are excited to tell you that your Application for life linker is received and it is under process you will become a successfull life linker after the successfull completion of hospital checkup with in 1 month other wise your account will be deleted" +
                    " Please Click on the below link for the hospital checkup" +
                    " <br/><br/><a href='" + link + "'>" + link + "</a> ";
          


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

    }
}
