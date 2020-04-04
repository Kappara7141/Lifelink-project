using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Donor.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        
        public ActionResult StartPage()
        {
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
        public ActionResult HospitalIndex()
        {
            return View();
        }
    }
}