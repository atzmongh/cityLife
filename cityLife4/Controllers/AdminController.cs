using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace cityLife4.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string recreateDB)
        {
            var a = recreateDB;
           
            FileStream sqlFile = new FileStream(Server.MapPath("/cityLifeDB.csv"),FileMode.Open);
            bool b = sqlFile.CanRead;

            return View();
        }
    }
}