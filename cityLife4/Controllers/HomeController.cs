using cityLife4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;

namespace cityLife.Controllers
{
    public class ApartmentMainPhoto
    {
        public Apartment apart;
        public ApartmentPhoto photo;
    }
    public class HomeController : Controller
    {
       
        [HttpGet]
        public ActionResult Index(string language)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            //get all apartments and for each apartment - its main photo.
            ViewBag.apartments = (from apartment in db.Apartments
                                join mainPhoto in db.ApartmentPhotoes on apartment.Id equals mainPhoto.Apartment.Id
                                where mainPhoto.type == PhotoType.Main
                                  select new ApartmentMainPhoto { apart = apartment, photo = mainPhoto });
            if (language==null)
            {
                //No language has been selected by the user - set English as the initial target language
                language = "EN";
            }

            TranslateBox tBox;
            if (Session["tBox"]== null)
            {
                //Create a new translateBox object and save in session
                tBox = new TranslateBox(targetLanguageCode: language,
                                        defaultLanguageCode: "RU",
                                        noTranslation: WebConfigurationManager.AppSettings["noTranslations"]);
                Session["tBox"] = tBox;
            }
            else
            {
                tBox = (TranslateBox)Session["tBox"];
            }
            tBox.setTargetLanguage(language);
            if (Session["currency"] == null)
            {

            }

            ViewBag.tBox = tBox;
            ViewBag.languages = db.Languages;
            ViewBag.pageURL = "home";
            ViewBag.currentLanguage = language;
           



            return View();
        }

        [HttpPost]
        public ActionResult Index(DateTime fromDate, DateTime toDate, int adultsCount, int childrenCount)
        {
          

            return View();
        }

        public ActionResult language()
        {
            return View("index");
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Home()
        {
            ViewBag.Message = "Your home page.";

            return View();
        }
    }
}