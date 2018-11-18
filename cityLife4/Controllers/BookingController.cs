using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace cityLife4.Controllers
{
    public class BookingController : Controller
    {
        // GET: Booking
        public ActionResult Index(int apartmentId)
        {
            throw new Exception("got " + apartmentId);
            return View();
        }
    }
}