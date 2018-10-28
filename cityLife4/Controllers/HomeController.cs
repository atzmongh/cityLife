using cityLife4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;

namespace cityLife.Controllers
{
    public enum Availability { UNKNOWN, AVAILABLE, OCCUPIED, NOT_SUITABLE};
    /// <summary>
    /// contains an apartment with 2 possible prices:
    /// - price per night (which is actually the minimum price per night) or
    /// - price per stay (exact price one we know the number of adults, children and dates)
    /// </summary>
    public class ApartmentPrice
    {
        public Apartment theApartment;
        public Money minPricePerNight;  //if not calculated - will be 0
        public Money pricePerStay;   //if not calculated - will be 0
        public int nightCount;     //for how many nights is the price per stay. 0 if unknown
        public Availability availability = Availability.UNKNOWN;
    }
    public class BookingRequest
    {
        public DateTime? checkinDate = null;
        public DateTime? checkoutDate = null;
        public int? adults = null;
        public int? children = null;

        /// <summary>
        /// returns true if the record is not empty  - denoted by having checkin date
        /// </summary>
        /// <returns></returns>
        public bool bookingRequestExists()
        {
            return checkinDate != null;
        }
    }
    public class HomeController : Controller
    {
        /// <summary>
        /// The action calculates all data rquired to display the home screen
        /// </summary>
        /// <param name="language">when the user selcets a language drop-down - we get the language in the parameter
        /// We will change the language accordingly</param>
        /// <param name="currency">when the user selects the currency drtop-down - we get the currency in this parameter</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index(string language, string currency, DateTime? fromDate, DateTime? toDate, int? adultsCount, int? childrenCount)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            TranslateBox tBox = this.setTbox(language);
            Currency theCurrency = this.setCurrency(currency);
            BookingRequest theBookingRequest = this.setBookingRequest(fromDate, toDate, adultsCount, childrenCount);


            //get all apartments and for each apartment - its price.
            //Price can either be:
            //- minimum price per night (if we do not know the dates of stay)
            //- exact price per stay (if we know number of adults, children and dates)
            //CURRENTLY ONLY MINIMUM PER NIGHT IS CALCULATED
            List<ApartmentPrice> apartmentList = new List<ApartmentPrice>();
            if (theBookingRequest.bookingRequestExists())
            {
                //We have a booking request - we need to show the information about price per stay for each apartment 
                //and also availabiltiy information for each apartment
                apartmentList = this.calculatePricePerStay(theBookingRequest, theCurrency);
            }
            else
            {
                //get all apartments and for each apartment - its price.
                //calculate minimum price per night (since we do not know the length of stay)
                foreach (var anApartment in db.Apartments)
                {
                    Money minPrice = anApartment.PricePerNight(adults: 1, children: 0, weekend: false, currencyCode: theCurrency.currencyCode, atDate: FakeDateTime.Now);
                    apartmentList.Add(new ApartmentPrice { theApartment = anApartment, minPricePerNight = minPrice, pricePerStay = null, nightCount = 0 });
                }
            }

            ViewBag.apartments = apartmentList;
            ViewBag.tBox = tBox;
            ViewBag.languages = db.Languages;
            ViewBag.pageURL = "/home";
            ViewBag.currentLanguage = tBox.targetLanguage;
            ViewBag.currencies = db.Currencies;
            ViewBag.currentCurrency = theCurrency;
            ViewBag.bookingRequest = theBookingRequest;


            return View();
        }

        /// <summary>
        /// the action is called when the user entered booking information and pressed the search button.
        /// </summary>
        /// <param name="fromDate">check in date</param>
        /// <param name="toDate">checkout date</param>
        /// <param name="adultsCount">number of adults</param>
        /// <param name="childrenCount">number of children</param>
        /// <returns></returns>
        public List<ApartmentPrice> calculatePricePerStay(BookingRequest theBookingRequest, Currency theCurrency)
        {

            cityLifeDBContainer1 db = new cityLifeDBContainer1();

            List<ApartmentPrice> apartmentList = new List<ApartmentPrice>();
            //Check apartment availability for the given dates and for the given occupation
            foreach (var anApartment in db.Apartments)
            {
                var apartmentDays = from anApartmentDay in db.ApartmentDays
                                    where anApartmentDay.Apartment.Id == anApartment.Id &&
                                    anApartmentDay.date >= theBookingRequest.checkinDate && 
                                    anApartmentDay.date < theBookingRequest.checkoutDate
                                    select anApartmentDay;

                var nonFreeDays = from anApartmentDay in apartmentDays
                                  where anApartmentDay.status != ApartOccuStatus.Free
                                  select anApartmentDay;
                if (nonFreeDays.Count() == 0) 
                {
                    //The apartment is free for the requested period - check number of adults and children
                    var pricing = from aPricing in db.Pricings
                                  where aPricing.Apartment.Id == anApartment.Id &&
                                        aPricing.adults == theBookingRequest.adults &&
                                        aPricing.children == theBookingRequest.children
                                  select aPricing;
                    if (pricing.Count() > 0)
                    {
                        //The apartment is suitable for that number of people. Calculate the price per stay and
                        //add the apartment to the list of available apartments
                        Pricing thePricing = pricing.First();  //Anyway we assume that only a single record will be found.

                        Money pricePerDay;
                        Money pricePerStay = new Money(0M, thePricing.Currency.currencyCode);
                        for (DateTime aDate = (DateTime)theBookingRequest.checkinDate; aDate < theBookingRequest.checkoutDate; aDate = aDate.AddDays(1))
                        {
                            if (aDate.IsWeekend())
                            {
                                pricePerDay = new Money(thePricing.priceWeekendAsMoney());
                            }
                            else
                            {
                                pricePerDay = new Money(thePricing.priceWeekdayAsMoney());
                            }
                            //Check if there is a discount for that day
                            ApartmentDay anApartmentDay = apartmentDays.SingleOrDefault(record => record.date == aDate);
                            if (anApartmentDay != null)
                            {
                                //There is an apartmentDay record for that day - it should contain a price factor. 
                                pricePerDay *= anApartmentDay.priceFactor;
                            }
                            pricePerStay += pricePerDay;
                        }
                        //At this point pricePerStay contains the total price for staying in the apartment. 
                        //COnvert it to the target currency
                        Money pricePerStayTargetCurrency = pricePerStay.converTo(theCurrency.currencyCode, FakeDateTime.Now);
                        int nightCount = ((TimeSpan)(theBookingRequest.checkoutDate - theBookingRequest.checkinDate)).Days;
                        ApartmentPrice apartmentAndPrice = new ApartmentPrice()
                        {
                            theApartment = anApartment,
                            pricePerStay = pricePerStayTargetCurrency,
                            minPricePerNight = null,
                            nightCount = nightCount,
                            availability = Availability.AVAILABLE
                        };
                        apartmentList.Add(apartmentAndPrice);
                    }
                    else
                    {
                        //The apartment is not available because of number of people
                        ApartmentPrice apartmentAndPrice = new ApartmentPrice()
                        {
                            theApartment = anApartment, availability = Availability.NOT_SUITABLE
                        };
                        apartmentList.Add(apartmentAndPrice);
                    }
                }
                else
                {
                    //The apartment is not free at that period
                    ApartmentPrice apartmentAndPrice = new ApartmentPrice()
                    {
                        theApartment = anApartment,
                        availability = Availability.OCCUPIED
                    };
                    apartmentList.Add(apartmentAndPrice);

                }
            }
            //At this point the apartment list contains the list of apartments plus the price per stay for each and whether it is suitable or not
            return apartmentList;
        }

        /// <summary>
        /// This method is an auxiliary method to create the translation box and to insert it if needed to the Session variable
        /// </summary>
        /// <param name="language">language code, if set by the user</param>
        /// <returns>the translation box</returns>
        private TranslateBox setTbox(string language)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();


            TranslateBox tBox;
            if (Session["tBox"] == null)
            {
                //Create a new translateBox object and save in session
                tBox = new TranslateBox(targetLanguageCode: "EN",
                                        defaultLanguageCode: "RU",
                                        noTranslation: WebConfigurationManager.AppSettings["noTranslations"]);
                Session["tBox"] = tBox;
            }
            else
            {
                tBox = (TranslateBox)Session["tBox"];
            }
            if (language != null)
            {
                tBox.targetLanguage = language;  //set language to what has been set by the user

            }
            return tBox;


        }

        /// <summary>
        /// Auxiliary method to set the currency.
        /// </summary>
        /// <param name="currency">Currency code if entered by the user or null</param>
        /// <returns>the currency currency object</returns>
        private Currency setCurrency(string currency)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();

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
            Currency theCurrency = db.Currencies.SingleOrDefault(aCurrency => aCurrency.currencyCode == currency);
            if (theCurrency == null)
            {
                throw new AppException(105, "Currency not found in DB:" + currency);
            }
            return theCurrency;
        }

        private BookingRequest setBookingRequest(DateTime? checkinDate, DateTime? checkoutDate, int? adults, int? children)
        {
            BookingRequest theBookingRequest;
            if (Session["bookingRequest"] == null)
            {
                theBookingRequest = new BookingRequest();
            }
            else
            {
                theBookingRequest = (BookingRequest)Session["bookingRequest"];
            }
            //At this point we have a booking request object
            if (checkinDate == null)
            {
                //checkin date is null - we assume no data was entered by the user. - do nothing
            }
            else
            {
                theBookingRequest.checkinDate = checkinDate;
                if (checkoutDate == null)
                {
                    //although should not happen - but we will take it as one day after checkin day
                    checkoutDate = checkinDate + new TimeSpan(1, 0, 0, 0);
                }
                if (adults == null)
                {
                    //use 1 adult as default
                    adults = 1;
                }
                if (children == null)
                {
                    children = 0;
                }
                theBookingRequest = new BookingRequest { checkinDate = checkinDate, checkoutDate = checkoutDate, adults = adults, children = children };
                Session["bookingRequest"] = theBookingRequest;
            }
            return theBookingRequest;
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