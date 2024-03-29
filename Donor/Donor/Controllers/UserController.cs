﻿using Donor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Donor.Controllers
{
    public class UserController : Controller
    {
        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }
        //Registration POST action 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] User user)
        {
            bool Status = false;
            string message = "";
            
            //
            // Model Validation 
            if (ModelState.IsValid)
            {

                #region //Email is already Exist 
                var isExist = IsEmailExist(user.EmailID);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code 
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region  Password Hashing 
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); //
                #endregion
                user.IsEmailVerified = false;

                #region Save to Database
                using (DonorEntities dc = new DonorEntities())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();

                    //Send Email to User
                    SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString());
                    message = "Registration successfully done. Account activation link " +
                        " has been sent to your email id:" + user.EmailID;
                    Status = true;
                }
                #endregion
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }
        //Verify Account  

        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (DonorEntities dc = new DonorEntities())
            {
               
                dc.Configuration.ValidateOnSaveEnabled = false; 
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        //Login 
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login,string ReturnUrl)
        {
            string message = "";
            using (DonorEntities dc=new DonorEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == login.EmailId).FirstOrDefault();
                
                    if (v != null)
                    {
                        if (v.IsEmailVerified==true)
                        {
                             if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                             {
                            int timeout = login.RememberMe ? 525600 : 20;
                            var ticket = new FormsAuthenticationTicket(login.EmailId, login.RememberMe, timeout);
                            string encrypted = FormsAuthentication.Encrypt(ticket);
                            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                            cookie.Expires = DateTime.Now.AddMinutes(timeout);
                            cookie.HttpOnly = true;
                            Response.Cookies.Add(cookie);

                            if (Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            message = "Invalid Password";
                        }
                    }
                    else
                    {
                        message = "Account Not Verified";
                    }

                       
                    }
                    else
                    {
                        message = "Invalid Credentials";
                    }
               
               
            }
                ViewBag.Message = message;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        public bool IsEmailExist(string emailID)
        {
            using (DonorEntities dc = new DonorEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode,string emailFor="VerifyAccount")
        {
            var verifyUrl = "/User/"+emailFor+"/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("organdonante@gmail.com", "Organ Donante");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "helloworld123#"; // Replace with actual password
            string subject = "";
            string body = "";
            if(emailFor== "VerifyAccount")
            {
                subject = "Your account is successfully created!";

                body = "<br/><br/>We are excited to tell you that your LifeLink account is" +
                    " successfully created. Please click on the below link to verify your account" +
                    " <br/><br/><a href='" + link + "'>" + link + "</a> ";
            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "Hi,<br/><br/>We got request for reset your account password.Please Click on the below link to reset your password"+
                    "<br/><br/><a href="+link+">"+link+"</a>";


            }
           

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

        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(string EmailId)
        {
            string message = "";
           // bool status = false;

            using (DonorEntities dc=new DonorEntities())
            {
                var acc = dc.Users.Where(a => a.EmailID == EmailId).FirstOrDefault();
                if (acc != null)
                {
                    string resetCode = Guid.NewGuid().ToString();
                    SendVerificationLinkEmail(acc.EmailID, resetCode, "ResetPassword");
                    message = "Password Reset Link Successfully Sent to Your Mail Id";
                    acc.ResetPasswordCode = resetCode;
                    dc.Configuration.ValidateOnSaveEnabled = false;
                    dc.SaveChanges();

                }
                else
                {
                    ModelState.AddModelError("EmailExist", "Account Not Found with Provided mail Id Please Check the Mail Id and Try again");
                   
                }
                ViewBag.Message = message;
            }
                return View();
        }
        public ActionResult ResetPassword(string id)
        {
            using (DonorEntities dc=new DonorEntities())
            {
                var user = dc.Users.Where(a => a.ResetPasswordCode == id).FirstOrDefault();
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (DonorEntities dc=new DonorEntities())
                {
                    var user = dc.Users.Where(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        user.Password = Crypto.Hash(model.NewPassword);
                        user.ResetPasswordCode = "";
                        dc.Configuration.ValidateOnSaveEnabled = false;
                        dc.SaveChanges();
                        message = "New Password has been successfully updated";
                    }
                }
            }
            else
            {
                message = "Something went wrong";
            }
            ViewBag.Message = message;
            return View(model);
        }

    }
}