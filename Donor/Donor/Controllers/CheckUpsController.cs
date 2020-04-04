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
    public class CheckUpsController : Controller
    {
        private DonorEntities db = new DonorEntities();

        // GET: CheckUps
        
        public ActionResult Index()
        {
            //var checkUps = db.CheckUps.Include(c => c.Hospital);
            string email = User.Identity.Name;
            //List<Hospital>  h = db.Hospitals.Where(a => a.EmailID.Equals(email)).ToList<Hospital>();
            return View(db.CheckUps.Where(a=>a.Hospital.EmailID.Equals(email)).ToList());
            
        }

        // GET: CheckUps/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CheckUp checkUp = db.CheckUps.Find(id);
            if (checkUp == null)
            {
                return HttpNotFound();
            }
            return View(checkUp);
        }

        // GET: CheckUps/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.HospitalId = new SelectList(db.Hospitals, "HospitalID", "HospitalName");
            return View();
        }

        // POST: CheckUps/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public ActionResult Create([Bind(Include = "Id,Email,HospitalId,State,City")] CheckUp checkUp)
        {
            bool Status = false;
            string message = "";
            if (ModelState.IsValid)
            {
                var isExist = IsEmailExist(checkUp.Email);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "You Already Applied for Checkup");
                    ViewBag.HospitalId = new SelectList(db.Hospitals, "HospitalID", "HospitalName", checkUp.HospitalId);
                    return View(checkUp);
                }
                else
                {
                    db.CheckUps.Add(checkUp);
                    db.SaveChanges();
                    Status = true;
                    ViewBag.HospitalId = new SelectList(db.Hospitals, "HospitalID", "HospitalName", checkUp.HospitalId);
                    message = "You have successfully applied for Checkup and You will receive a Email from the corresponding hospital to your registered mail id"+checkUp.Email;
                }
                ViewBag.Message = message;
                ViewBag.Status = Status;
                return View(checkUp);
            }

           
            return View(checkUp);
        }

        // GET: CheckUps/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CheckUp checkUp = db.CheckUps.Find(id);
            if (checkUp == null)
            {
                return HttpNotFound();
            }
            ViewBag.HospitalId = new SelectList(db.Hospitals, "HospitalID", "HospitalName", checkUp.HospitalId);
            return View(checkUp);
        }

        // POST: CheckUps/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Email,HospitalId,State,City")] CheckUp checkUp)
        {
            if (ModelState.IsValid)
            {
                db.Entry(checkUp).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.HospitalId = new SelectList(db.Hospitals, "HospitalID", "HospitalName", checkUp.HospitalId);
            return View(checkUp);
        }

        // GET: CheckUps/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    CheckUp checkUp = db.CheckUps.Find(id);
        //    if (checkUp == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(checkUp);
        //}

        // POST: CheckUps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CheckUp checkUp = db.CheckUps.Find(id);
            db.CheckUps.Remove(checkUp);
            db.SaveChanges();
            return Json(new { success = true, message = "Deleted Successfully", JsonRequestBehavior.AllowGet });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public bool IsEmailExist(string emailID)
        {
            using (DonorEntities dc = new DonorEntities())
            {
                var v = dc.CheckUps.Where(a => a.Email == emailID).FirstOrDefault();
                return v != null;
            }
        }

    [HttpPost]
        public ActionResult SendEmail(int id)
        {
            string emailID = "";
            var email = db.CheckUps.Find(id);
            emailID = email.Email;
            string location = email.City;
            var fromEmail = new MailAddress("organdonante@gmail.com", "Organ Donante");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "helloworld123#"; // Replace with actual password
            string subject = "";
            string body = "";
           
                subject = "Your Appointment Scheduled for Master Health Checkup";

                body = " You can visit hospital at 9Am at "+location;
            
           


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
            return Json(new { sucess = emailID }, JsonRequestBehavior.AllowGet);
        }
        
    }
}
