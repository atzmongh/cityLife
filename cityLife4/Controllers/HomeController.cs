using cityLife4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;

namespace cityLife.Controllers
{
    /// <summary>
    /// contains an apartment with 2 possible prices:
    /// - price per night (which is actually the minimum price per night)
    /// / price per stay (exact price one we know the number of adults, children and dates)
    /// </summary>
    public class ApartmentPrice
    {
        public Apartment theApartment;
        public int minPricePerNight;  //if not calculated - will be 0
        public int pricePerStay;   //if not calculated - will be 0
        public int nightCount;     //for how many nights is the price per stay. 0 if unknown
    }
    public class HomeController : Controller
    {
        /// <summary>
        /// The action calculates all data rquired to display the home screen
        /// </summary>
        /// <param name="language">when the user selcets a language drop-down - we get the language in the parameter
        /// We will change the language accordingly</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index(string language, string currency)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();


            if (language == null)
            {
                //No language has been selected by the user - set English as the initial target language
                language = "EN";
            }

            TranslateBox tBox;
            if (Session["tBox"] == null)
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
            tBox.setTargetLanguage(language);  //either EN as default or what has been set by the user

            if (currency == null && Session["currency"] == null)
            {
                //use default currency
                currency = "UAH";
                Session["currency"] = currency;
            }
            else if (currency == null)
            {
                //the session contains a currency - use it
                currency = (string)Session["currency"];
            }
            else
            {
                //currency contains a new currency selected by the user
                Session["currency"] = currency;
            }
            Currency theCurrency = db.Currencies.Single(aCurrency => aCurrency.currencyCode == currency);

            //get all apartments and for each apartment - its price.
            //Price can either be:
            //- minimum price per night (if we do not know the dates of stay)
            //- exact price per stay (if we know number of adults, children and dates)
            //CURRENTLY ONLY MINIMUM PER NIGHT IS CALCULATED
            List<ApartmentPrice> apartmentList = new List<ApartmentPrice>();
            foreach (var anApartment in db.Apartments)
            {
                int minPrice = anApartment.PricePerNight(adults: 1, children: 0, weekend: false, currencyCode: currency, atDate: FakeDateTime.Now);
                apartmentList.Add(new ApartmentPrice { theApartment = anApartment, minPricePerNight = minPrice, pricePerStay = 0, nightCount = 0 });
            }


            ViewBag.apartments = apartmentList;   
            ViewBag.tBox = tBox;
            ViewBag.languages = db.Languages;
            ViewBag.pageURL = "home";
            ViewBag.currentLanguage = language;
            ViewBag.currencies = db.Currencies;
            ViewBag.currentCurrency = theCurrency;

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