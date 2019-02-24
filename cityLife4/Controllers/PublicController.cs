using cityLife;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using cityLife4;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Net;

namespace cityLife.Controllers
{
    public enum Availability { UNKNOWN, AVAILABLE, OCCUPIED, NOT_SUITABLE };
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

        public string checkInOutDates()
        {
            string checkInOut = "";
            if (checkinDate != null)
            {
                checkInOut = ((DateTime)checkinDate).ToShortDateString();
            }
            checkInOut += '-';
            if (checkoutDate != null)
            {
                checkInOut += ((DateTime)checkoutDate).ToShortDateString();
            }
            return checkInOut;
        }
    }

    /// <summary>
    /// This is a base class for classes that define the fields of a form. They should contain all properties of the form, plus an "isValid"
    /// method that checks the form for validity. Each invalid property will get an error message in the errorMessage dictionary.
    /// where the key is the property name. For example, if the address field is empty, we will add a <key,value> pair to the dictioanry:
    /// key="address", value="address cannot be empty"
    /// </summary>
    public abstract class formData
    {
        protected Dictionary<string, string> errorMessage = new Dictionary<string, string>();
        /// <summary>
        /// Returns true if the form is valid. If not valid - adds error messages to the dictionary.
        /// </summary>
        /// <returns></returns>
        public abstract bool isValid();
        /// <summary>
        /// Returns the translated error messages for a specific property, or empty string if the field was valid
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="tBox"></param>
        /// <returns></returns>
        public string errorMessageOf(string fieldName, TranslateBox tBox)
        {
            try
            {
                return tBox.translate(this.errorMessage[fieldName]);
            }
            catch (Exception)
            {
                return "";
            }
        }
        public void setErrorMessageFor(string fieldName, string message)
        {
            errorMessage.Add(fieldName, message);
        }
        /// <summary>
        /// The method returns the word "error" if the field has an error. This will create the CSS class "error"
        /// It returns empty string otherwise
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string errorOf(string fieldName)
        {
            return this.errorMessage.ContainsKey(fieldName) ? "error" : "";
        }

    }
    public class FieldData
    {
        public string fieldName;
        public string content = "";
        public string errorMessage = "";
        public string errorOrNot
        {
            get
            {
                return errorMessage != "" ? "error" : "";
            }
        }
        public FieldData(string theName)
        {
            this.fieldName = theName;
        }
        public FieldData(string theName, string theContent)
        {
            this.fieldName = theName;
            this.content = theContent;
        }
        /// <summary>
        /// The method returns the field name without spaces. For examle, the name: Arrival Time will be returned as
        /// ArrivalTime (Camel Case). It does not change the case of the first letter of each word, so the name:
        /// Special request will be returned as Specialrequest
        /// </summary>
        public string fieldCamelName
        {
            get
            {
                string camelName = this.fieldName.Replace(" ", "");
                return camelName;
            }
        }


    }



    /// <summary>
    /// Note that the class inherits methods errorMessageOf and errorOf
    /// </summary>
    public class OrderData : formData
    {
        public int orderId;
        public string name;
        public string phone;
        public string email;
        public string country;
        public int adults;
        public int children;
        public DateTime checkin;
        public DateTime checkout;
        public int nights;
        public Money price;
        public Money paid;
        public string expectedArrival;
        public string comments;
        public string bookedBy;
        public int apartmentNumber;
        public string confirmationNumber;
        public OrderStatus status;
        public Color orderColor;
        public string staffComments;

        public OrderData()
        {
            status = OrderStatus.Created;
            paid = new Money(0m,"UAH");
            confirmationNumber = "0";
            orderColor = Color.Red;
        }
        public OrderData(Order anOrder)
        {
            orderId = anOrder.Id;
            name = anOrder.Guest.name;
            phone = anOrder.Guest.phone;
            email = anOrder.Guest.email;
            country = anOrder.Guest.Country.name;
            adults = anOrder.adultCount;
            children = anOrder.childrenCount;
            checkin = anOrder.checkinDate;
            checkout = anOrder.checkoutDate;
            nights = anOrder.nights;
            price = anOrder.priceAsMoney();
            paid = anOrder.amountPaidAsMoney();
            expectedArrival = anOrder.expectedArrival;
            comments = anOrder.specialRequest;
            bookedBy = anOrder.bookedBy;
            apartmentNumber = anOrder.Apartment.number;
            confirmationNumber = anOrder.confirmationNumber;
            status = anOrder.status;
            orderColor = anOrder.OrderColor;
            staffComments = anOrder.staffComments;
        }
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public override bool isValid()
        {
            this.errorMessage.Clear();
            if (this.name == null || this.name == "")
            {
                this.errorMessage.Add("name", "Please enter name");
            }

            if (this.country == null || this.country == "")
            {
                this.errorMessage.Add("country", "Please select country");
            }
            else
            {
                cityLifeDBContainer1 db = new cityLifeDBContainer1();
                Country theCountry = db.Countries.SingleOrDefault(a => a.name == this.country);
                if (theCountry == null)
                {
                    this.errorMessage.Add("country", "This country does not exist in our country list");
                }
            }
            if (!OrderData.IsValidEmail(this.email))
            {
                this.errorMessage.Add("email", "Please enter a valid email address");
            }
            if (!Regex.Match(this.phone, @"^(\+?[0-9]{10,13})$").Success)
            {
                this.errorMessage.Add("phone", "Please enter a valid phone number");
            }
            if (adults <= 1)
            {
                this.errorMessage.Add("adults", "Number of adults must be at least 1");
            }
            if (children < 0)
            {
                this.errorMessage.Add("children", "number of children cannot be negative");
            }
            if (checkout <= checkin)
            {
                this.errorMessage.Add("checkin", "Checkout date cannot be before or equal to checkin date");
            }
            return errorMessage.Count() == 0;
        }
    }


    public class PublicController : Controller
    {
        /// <summary>
        /// The action calculates all data rquired to display the home screen
        /// </summary>
        /// <param name="language">when the user selcets a language drop-down - we get the language in the parameter
        /// We will change the language accordingly</param>
        /// <param name="currency">when the user selects the currency drtop-down - we get the currency in this parameter</param>
        /// <param name="fromDate">Date of checkin</param>
        /// <param name="toDate">Date of checkout</param>
        /// <param name="adultsCount">number of adults</param>
        /// <param name="childrenCount">number of children</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult p10home(string language, string currency, DateTime? fromDate, DateTime? toDate, int? adultsCount, int? childrenCount)
        {
            this.prepareApartmentData(language, currency, fromDate, toDate, adultsCount, childrenCount);

            ViewBag.pageURL = "/public/p10home";
            ViewBag.preferredDevice = this.preferredDevice();

            return View("p10home");
        }

        /// <summary>
        /// Displays the list of apartments and their availability, if known. If not known (because the user did not enter checkin-checkout
        /// information - show just a list of apartments. This controller is also used for the "apartment" option in the main menu
        /// where it will show the list of apartments and minimum price per night for each
        /// </summary>
        /// <param name="language"></param>
        /// <param name="currency"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="adultsCount"></param>
        /// <param name="childrenCount"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult p20checkAvailability(string language, string currency, DateTime? fromDate, DateTime? toDate, int? adultsCount, int? childrenCount)
        {
            this.prepareApartmentData(language, currency, fromDate, toDate, adultsCount, childrenCount);

            ViewBag.pageURL = "/public/p20checkAvailability";

            return View("p20availableAppartments");
        }

        [HttpGet]
        public ActionResult p25apartmentDetails(int apartmentId)
        {
            //Look for the apartment for which we need to book
            ApartmentPrice theApartmentAndPrice;
            try
            {
                List<ApartmentPrice> apartments = (List<ApartmentPrice>)Session["apartments"];
                string currentLanguage = (string)Session["currentLanguage"];
                Currency currentCurrency = (Currency)Session["currentCurrency"];
                BookingRequest theBookingRequest = (BookingRequest)Session["bookingRequest"];
                theApartmentAndPrice = apartments.Single(a => a.theApartment.Id == apartmentId);

                cityLifeDBContainer1 db = new cityLifeDBContainer1();
                ViewBag.apartmentAndPrice = theApartmentAndPrice;
                ViewBag.tBox = this.setTbox(currentLanguage);
                ViewBag.languages = db.Languages;
                ViewBag.currentLanguage = currentLanguage;
                ViewBag.currencies = db.Currencies;
                ViewBag.currentCurrency = currentCurrency;
                ViewBag.bookingRequest = theBookingRequest;
                ViewBag.pageURL = "/public/p25apartmentDetails";
                ViewBag.Title = "apartment details";
                ViewBag.preferredDevice = this.preferredDevice();

            }
            catch (Exception e)
            {
                //We could not find the requested apartment in the List<apartmentPrice>, although it should have been there.
                //Write an error to the log and continue
                AppException.writeException(112, e, e.StackTrace, apartmentId);
                Server.Transfer("/home");
            }

            return View("p25apartmentDetails");
        }
        [HttpGet]
        public PartialViewResult p26bookingForm(int apartmentId)
        {
            this.prepareDataForp26p27(apartmentId);
            ViewBag.orderData = new OrderData();   //Empty order data, as this is a new form with no data


            return PartialView("p26bookingForm");

        }


        [HttpPost]
        public PartialViewResult p27bookingOrder(int apartmentId, string Name, string Country, string Email, string Phone, string ArrivalTime, string SpecialRequest)
        {
            prepareDataForp26p27(apartmentId);
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            Apartment theAparatment = db.Apartments.Single(anApartment => anApartment.Id == apartmentId);
            BookingRequest theBookingRequest = (BookingRequest)Session["bookingRequest"];

            //perform validity chekcs on the input
            OrderData theOrderData = new OrderData()
            {
                name = Name,
                country = Country,
                email = Email,
                phone = Phone,
                expectedArrival = ArrivalTime,
                comments = SpecialRequest,
                adults = (int)theBookingRequest.adults,
                children = (int)theBookingRequest.children,
                checkin = (DateTime)theBookingRequest.checkinDate,
                checkout = (DateTime)theBookingRequest.checkoutDate
            };

            ViewBag.orderData = theOrderData;
            if (!theOrderData.isValid())
            {
                return PartialView("p26bookingForm");
            }
            else
            {
                //The form is valid - create the booking order
                //Check if the booking request is still valid.

                Currency currentCurrency = (Currency)Session["currentCurrency"];
                ApartmentPrice apartmentAndPrice = calculatePricePerStayForApartment(theAparatment, db, theBookingRequest, currentCurrency);
                theOrderData.price = apartmentAndPrice.pricePerStay;
                theOrderData.apartmentNumber = apartmentAndPrice.theApartment.number;

                Country theCountry = db.Countries.Single(a => a.name == Country);
                if (apartmentAndPrice.availability != Availability.AVAILABLE)
                {
                    //the apartment is not available, although it seemed to be available. Perhaps it was taken in the last minutes
                    return PartialView("p27bookingFailure");
                }
                else
                {
                    //Complete the booking
                    TranslateBox tBox = ViewBag.tBox;
                    Order theOrder = PublicController.p27createUpdateOrder(db, theBookingRequest, apartmentAndPrice, theOrderData, theCountry, tBox);

                    ViewBag.theOrder = theOrder;
                    ViewBag.apartmentAndPrice = apartmentAndPrice;

                    //if (theOrder.Guest.email != null && theOrder.Guest.email != "")
                    //{
                    //    EmailMessage mailToCustomer = new EmailMessage(
                    //    to: theOrder.Guest.email,
                    //    subject: tBox.translate("Welcome to Kharkov Apartments City Life"),
                    //    mailName: "t10welcomeMail",
                    //    theController: this
                    //    );
                    //    mailToCustomer.send();
                    //}
                    return PartialView("p28bookingSuccess");
                }
            }
        }

        public static Order p27createUpdateOrder(cityLifeDBContainer1 db, BookingRequest theBookingRequest, ApartmentPrice apartmentAndPrice,
            OrderData theOrderData, Country theCountry, TranslateBox tBox)
        {
            DateTime checkIn = (DateTime)theBookingRequest.checkinDate;
            DateTime checkOut = (DateTime)theBookingRequest.checkoutDate;
            var dayCount = (checkOut - checkIn).Days;
            Currency theCurrency = db.Currencies.Single(a => a.currencyCode == apartmentAndPrice.pricePerStay.currency);

            Guest theGuest = db.Guests.FirstOrDefault
                (aGuest => aGuest.email == theOrderData.email &&
                aGuest.name == theOrderData.name &&
                aGuest.phone == theOrderData.phone &&
                aGuest.Country.name == theOrderData.country);

            if (theGuest == null)
            {
                //there is no such guest - create new
                theGuest = new Guest()
                {
                    name = theOrderData.name,
                    phone = theOrderData.phone,
                    email = theOrderData.email,
                    Country = theCountry
                };
                db.Guests.Add(theGuest);
            }

            Order theOrder;
            if (theOrderData.orderId == 0)
            {
                //This is a new order that should be added
                theOrder = new Order();
            }
            else
            {
                //This is an existing order that should be updated. 
                theOrder = db.Orders.Single(a => a.Id == theOrderData.orderId);

                //Change all existing ApartmentDays records to be "free"
                var theApartmentDays = from anApartmentDays in db.ApartmentDays
                                       where anApartmentDays.Order.Id == theOrderData.orderId
                                       select anApartmentDays;
                foreach(var anApartmentDays in theApartmentDays)
                {
                    anApartmentDays.status = ApartOccuStatus.Free;
                    anApartmentDays.revenue = 0;
                }
            }
            //At this point we have an order record (either new or existing) and apartment days records, if already exist - are changed to be 
            //in "free" status. Now we can add/update the order record and add/update the apartment days records.

            theOrder.adultCount = (int)theBookingRequest.adults;
            theOrder.amountPaid = theOrderData.paid.toCents();
            theOrder.Apartment = apartmentAndPrice.theApartment;
            theOrder.bookedBy = theOrderData.bookedBy;
            theOrder.checkinDate = (DateTime)theBookingRequest.checkinDate;
            theOrder.checkoutDate = (DateTime)theBookingRequest.checkoutDate;
            theOrder.childrenCount = theBookingRequest.children ?? 0;
            theOrder.confirmationNumber = theOrderData.confirmationNumber;
            theOrder.expectedArrival = theOrderData.expectedArrival;
            theOrder.specialRequest = theOrderData.comments;
            theOrder.status = theOrderData.status;
            theOrder.dayCount = dayCount;
            theOrder.price = theOrderData.price.toCents();   //Note that we take the price from orderData and not from apartmentAndPrice. IN the staff app - the 
                                                             //price is determined by the admin and it can be different than the automatic calculation
            theOrder.Guest = theGuest;
            theOrder.Currency = theCurrency;
            theOrder.OrderColor = theOrderData.orderColor;
            theOrder.staffComments = theOrderData.staffComments;
            

            if (theOrderData.orderId == 0)
            {
                db.Orders.Add(theOrder);
            }

            //Create or update the apartment day records (a record for each apartment-day)
            for (var aDay = checkIn; aDay < checkOut; aDay = aDay.AddDays(1))
            {
                ApartmentDay anApartmentDay = db.ApartmentDays.SingleOrDefault(a => a.date == aDay && a.Apartment.Id == apartmentAndPrice.theApartment.Id);
                if (anApartmentDay == null)
                {
                    //record does not exist - create it
                    anApartmentDay = new ApartmentDay()
                    {
                        Order = theOrder,
                        date = aDay,
                        Apartment = apartmentAndPrice.theApartment,
                        Currency = theCurrency,
                        priceFactor = 1.0m,
                        isCleaned = false,
                        revenue = 0,
                        status = ApartOccuStatus.Reserved
                    };
                    if (aDay == checkIn)
                    {
                        //this is the first day of the booking - add the revenue to this day
                        anApartmentDay.revenue = apartmentAndPrice.pricePerStay.toCents();
                    }
                    db.ApartmentDays.Add(anApartmentDay);
                }
                else
                {
                    //a apartment day record exists.
                    if (anApartmentDay.status == ApartOccuStatus.Free)
                    {
                        anApartmentDay.Order = theOrder;
                        anApartmentDay.Apartment = apartmentAndPrice.theApartment;
                        anApartmentDay.Currency = theCurrency;
                        if (aDay == checkIn)
                        {
                            anApartmentDay.revenue = apartmentAndPrice.pricePerStay.toCents();
                        }
                        
                        anApartmentDay.status = ApartOccuStatus.Reserved;
                    }
                    else
                    {
                        //The record exists but it is not free - raise an exception
                        throw new AppException(113, null, apartmentAndPrice.theApartment.number, aDay.ToShortDateString(), theGuest.name);
                    }
                }
            }
            db.SaveChanges();

            return theOrder;
        }

        private void sendMail(Order theOrder, ApartmentPrice apartmentAndPrice)
        {
            //var message = new MailMessage();
            //message.To.Add(new MailAddress(theOrder.Guest.email));  // replace with valid value 
            //message.From = new MailAddress("apart.citylife@gmail.com");  // replace with valid value
            //message.Subject = "Your email subject";
            //message.Body = "hello world";
            //message.IsBodyHtml = true;

            //using (var smtp = new SmtpClient())
            //{
            //    var credential = new NetworkCredential
            //    {
            //        UserName = "apart.citylife@gmail.com",  // replace with valid value
            //        Password = "456ertksenia"  // replace with valid value
            //    };
            //    smtp.Credentials = credential;
            //    smtp.Host = "smtp.gmail.com";
            //    smtp.Port = 587;
            //    smtp.EnableSsl = true;
            //    smtp.Send(message);
            //}
        }

        public JsonResult p29checkEmailExists(string email)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            var theGuests = from aGuest in db.Guests
                            where aGuest.email == email
                            select new { aGuest.name, aGuest.phone, country = aGuest.Country.name };
            JsonResult jResult;
            if (theGuests.Count() == 1)
            {
                //There is exactly 1 guest with that email - send his personal details as default
                jResult = Json(theGuests.First(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                jResult = Json(new { name = "", phone = "", country = "" }, JsonRequestBehavior.AllowGet);
            }
            return jResult;
        }
        [HttpGet]
        public ActionResult p30apartments(string language, string currency, DateTime? fromDate, DateTime? toDate, int? adultsCount, int? childrenCount)
        {
            this.prepareApartmentData(language, currency, fromDate, toDate, adultsCount, childrenCount);

            ViewBag.pageURL = "/public/p30apartments";


            return View("p30apartments");
        }

        [HttpGet]
        public ActionResult p50OurRules(string language, string currency, DateTime? fromDate, DateTime? toDate, int? adultsCount, int? childrenCount)
        {
            this.prepareApartmentData(language, currency, fromDate, toDate, adultsCount, childrenCount);

            ViewBag.pageURL = "/public/p50ourRules";
            ViewBag.preferredDevice = this.preferredDevice();
            return View("p50ourRulesEnglish");
        }
        private void prepareApartmentData(string language, string currency, DateTime? fromDate, DateTime? toDate, int? adultsCount, int? childrenCount)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            TranslateBox tBox = this.setTbox(language);
            Currency theCurrency = this.setCurrency(currency);
            BookingRequest theBookingRequest = this.setBookingRequest(fromDate, toDate, adultsCount, childrenCount);


            //get all apartments and for each apartment - its price.
            //Price can either be:
            //- minimum price per night (if we do not know the dates of stay)
            //- exact price per stay (if we know number of adults, children and dates)
            List<ApartmentPrice> apartmentList = new List<ApartmentPrice>();
            if (theBookingRequest.bookingRequestExists())
            {
                //We have a booking request - we need to show the information about price per stay for each apartment 
                //and also availabiltiy information for each apartment
                apartmentList = calculatePricePerStay(theBookingRequest, theCurrency);
            }
            else
            {
                //get all apartments and for each apartment - its price.
                //calculate minimum price per night (since we do not know the length of stay)
                foreach (var anApartment in db.Apartments)
                {
                    Money minPrice = anApartment.PricePerNight(adults: 1, children: 0, weekend: false, currencyCode: theCurrency.currencyCode, atDate: FakeDateTime.Now);
                    apartmentList.Add(new ApartmentPrice { theApartment = anApartment, availability = Availability.UNKNOWN, minPricePerNight = minPrice, pricePerStay = null, nightCount = 0 });
                }
            }
            //Keep the data in the session variable for next iteration
            Session["apartments"] = apartmentList;
            Session["currentLanguage"] = tBox.targetLanguage;
            Session["currentCurrency"] = theCurrency;
            Session["bookingRequest"] = theBookingRequest;

            ViewBag.apartments = apartmentList;
            ViewBag.tBox = tBox;
            ViewBag.languages = db.Languages;
            ViewBag.currentLanguage = tBox.targetLanguage;
            ViewBag.currencies = db.Currencies;
            ViewBag.currentCurrency = theCurrency;
            ViewBag.bookingRequest = theBookingRequest;

        }

        private void prepareDataForp26p27(int apartmentId)
        {
            ApartmentPrice theApartmentAndPrice;
            try
            {
                List<ApartmentPrice> apartments = (List<ApartmentPrice>)Session["apartments"];
                BookingRequest theBookingRequest = (BookingRequest)Session["bookingRequest"];
                theApartmentAndPrice = apartments.Single(a => a.theApartment.Id == apartmentId);
                string currentLanguage = (string)Session["currentLanguage"];

                cityLifeDBContainer1 db = new cityLifeDBContainer1();
                ViewBag.apartmentAndPrice = theApartmentAndPrice;
                ViewBag.tBox = this.setTbox(currentLanguage);
                ViewBag.bookingRequest = theBookingRequest;
                ViewBag.countries = db.Countries;
            }
            catch (Exception e)
            {
                //We could not find the requested apartment in the List<apartmentPrice>, although it should have been there.
                //Write an error to the log and continue
                AppException.writeException(112, e, e.StackTrace, apartmentId);
                Server.Transfer("/home");
            }
        }
        /// <summary>
        /// the method  is called when the user entered booking information and pressed the search button. 
        /// The method calculates the price per each apartment and whether it is available and suitable for
        /// the group size
        /// </summary>
        /// <param name="fromDate">check in date</param>
        /// <param name="toDate">checkout date</param>
        /// <param name="adultsCount">number of adults</param>
        /// <param name="childrenCount">number of children</param>
        /// <returns>list of apartments. For each apartment - the price per stay (if available)
        /// and the availability (available, not available, not suitable)</returns>
        public static List<ApartmentPrice> calculatePricePerStay(BookingRequest theBookingRequest, Currency theCurrency)
        {

            cityLifeDBContainer1 db = new cityLifeDBContainer1();

            List<ApartmentPrice> apartmentList = new List<ApartmentPrice>();
            //Check apartment availability for the given dates and for the given occupation
            foreach (var anApartment in db.Apartments)
            {
                ApartmentPrice apartmentAndPrice = calculatePricePerStayForApartment(anApartment, db, theBookingRequest, theCurrency);
                apartmentList.Add(apartmentAndPrice);
            }

            //At this point the apartment list contains the list of apartments plus the price per stay for each and whether it is suitable or not
            return apartmentList;
        }

        /// <summary>
        /// the method calculates the availability and price per stay for a specific apartment
        /// </summary>
        /// <param name="anApartment">the apartment for which we need to calculate availability</param>
        /// <param name="db"></param>
        /// <param name="theBookingRequest"></param>
        /// <param name="theCurrency"></param>
        /// <param name="originalOrderId">In case we update an existing order - gives the original order ID which we wish to update. Otherwise
        /// it is set to 0</param>
        /// <returns>apartment availability and price information</returns>
        public static ApartmentPrice calculatePricePerStayForApartment(Apartment anApartment, cityLifeDBContainer1 db, BookingRequest theBookingRequest, Currency theCurrency, int originalOrderId = 0)
        {

            ApartmentPrice apartmentAndPrice;
            //Look for apartment days for that apartment and these dates. Note that we exclude from the list records belonging to the same order ID (in case of update)
            //For example: we had an order for 1/12 until 3/12. If we added another day - we are only interestsed to know if the new day is free or not.
            var apartmentDays = from anApartmentDay in db.ApartmentDays
                                where anApartmentDay.Apartment.Id == anApartment.Id &&
                                anApartmentDay.date >= theBookingRequest.checkinDate &&
                                anApartmentDay.date < theBookingRequest.checkoutDate &&
                                anApartmentDay.Order.Id != originalOrderId   //if the apartment day is for the same order ID - it is not "blocking" us from updating the order
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
                    apartmentAndPrice = new ApartmentPrice()
                    {
                        theApartment = anApartment,
                        pricePerStay = pricePerStayTargetCurrency,
                        minPricePerNight = null,
                        nightCount = nightCount,
                        availability = Availability.AVAILABLE
                    };
                }
                else
                {
                    //The apartment is not available because of number of people
                    apartmentAndPrice = new ApartmentPrice()
                    {
                        theApartment = anApartment,
                        availability = Availability.NOT_SUITABLE
                    };
                }
            }
            else
            {
                //The apartment is not free at that period
                apartmentAndPrice = new ApartmentPrice()
                {
                    theApartment = anApartment,
                    availability = Availability.OCCUPIED
                };
            }
            return apartmentAndPrice;

        }

        /// <summary>
        /// This method is an auxiliary method to create the translation box and to insert it if needed to the Session variable
        /// </summary>
        /// <param name="language">language code, if set by the user</param>
        /// <returns>the translation box</returns>
        public TranslateBox setTbox(string language)
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
                throw new AppException(105, null, "Currency not found in DB:" + currency);
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

        private DisplayDevice preferredDevice()
        {
            if (Request.Browser.IsMobileDevice)
            {
                return DisplayDevice.MOBILE;
            }
            else
            {
                return DisplayDevice.DESKTOP;
            }
        }







        public String RenderViewToString(ControllerContext context, String viewPath, object model = null)
        {
            context.Controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindView(context, viewPath, null);
                var viewContext = new ViewContext(context, viewResult.View, context.Controller.ViewData, context.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(context, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }



    }
}