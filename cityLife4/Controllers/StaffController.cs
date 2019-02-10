using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using cityLife;
using cityLife4;
using System.Text.RegularExpressions;

namespace cityLife.Controllers
{
    public class LoginFormData:formData
    {
        public string userName;
        public string password;

        public override bool isValid()
        {
            errorMessage.Clear();
            if (userName == "")
            {
                errorMessage.Add("userName", "Please enter your user name");
            }
            if (password == "")
            {
                errorMessage.Add("password", "Please enter the password");
            }
            if (errorMessage.Count() == 0)
            {
                //No error so far - check if such user exist and password matches
                cityLifeDBContainer1 db = new cityLifeDBContainer1();
                Employee theEmployee = db.Employees.FirstOrDefault(anEmp => anEmp.userName == userName);
                if (theEmployee == null)
                {
                    errorMessage.Add("userName", "Username was not found");
                }
                else if (theEmployee.passwordHash != password)
                {
                    errorMessage.Add("password", "The password does not match our records");
                }
            }
            return errorMessage.Count() == 0;
        }
    }
    /// <summary>
    /// A day block is either a single free day, or a group of occupied days for a single apartment and a single guest.
    /// OrderStatus - Created (guest did not arrive yet), checkedIn (guest is now in the apartment), checkedOut (guest left the apartment), null (free)
    /// name - gueset name or empty (if free)
    /// days - number of days of that order.
    /// orderId - 
    /// </summary>
    public class DayBlock
    {
        public int apartmentNumber;
        public OrderStatus? orderStatus;
        public string name;
        public int days;
        public int orderId;
        public string firstDate;
    }
   

    
    /// <summary>
    /// The class contains additional fields that do not exist in the booking form data
    /// </summary>
    //public class StaffBookingFormData : BookingFormData
    //{
    //    public FieldData checkinDate;
    //    public FieldData checkoutDate;
    //    public FieldData nights;
    //    public FieldData adults;
    //    public FieldData children;
    //    public FieldData price;
    //    public FieldData paidAmount;
    //    public FieldData apartmentNumber;
    //    public FieldData confirmationNumber;
    //    public FieldData orderId;

    //    public StaffBookingFormData():base()
    //    {
    //         this.checkinDate = new FieldData("Checkin Date");
    //    this.checkoutDate = new FieldData("Checkout Date");
    //    this.nights = new FieldData("Nights");
    //    this.adults = new FieldData("Adults");
    //    this.children = new FieldData("Children");
    //    this.price = new FieldData("Price");
    //    this.paidAmount = new FieldData("Paid Amount");
    //    this.apartmentNumber = new FieldData("Apartment Number");
    //    this.confirmationNumber = new FieldData("Confirmation Number");
    //    this.orderId = new FieldData("Order ID");
    //    }
    //    public StaffBookingFormData(string name, string country, string email, string phone, string arrivalTime, 
    //        string specialRequest, string bookedBy,
    //        string checkin, string checkout, string nights, string adults, 
    //        string children, string price, string paidAmount, string apartmentNumber, string orderId):base(name,country,email,phone,arrivalTime,specialRequest,bookedBy)
    //    {
    //        this.checkinDate = new FieldData("Checkin Date", checkin);
    //        this.checkoutDate = new FieldData("Checkout Date", checkout);
    //        this.nights = new FieldData("Nights", nights);
    //        this.adults = new FieldData("Adults", adults);
    //        this.children = new FieldData("Children", children);
    //        this.price = new FieldData("Price", price);
    //        this.paidAmount = new FieldData("Paid Amount", paidAmount);
    //        this.apartmentNumber = new FieldData("Apartment Number", apartmentNumber);
    //       // this.confirmationNumber = new FieldData("Confirmation Number", confirmationNumber);
    //        this.orderId = new FieldData("Order Id", orderId);
    //    }
    //    public override bool isValid()  
    //    {
    //        if (base.isValid())   //TBD - should be changted
    //            return true;

    //        if (checkinDate.errorMessage != "" ||
    //            checkoutDate.errorMessage != "" ||
    //            nights.errorMessage != "" ||
    //            adults.errorMessage != "" ||
    //            children.errorMessage != "" ||
    //            price.errorMessage != "" ||
    //            paidAmount.errorMessage != "" ||
    //            apartmentNumber.errorMessage != "" ||
    //            confirmationNumber.errorMessage != "" ||
    //            orderId.errorMessage != "")
    //            return true;

    //        return false;
    //    }
    //}
    public class StaffController : Controller
    {
        // GET: Staff
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult s10login()
        {
            TranslateBox tBox = this.setTbox("RU");
            LoginFormData theLoginFormData = new LoginFormData();
            ViewBag.tBox = tBox;
            ViewBag.theLoginFormData = theLoginFormData;
            return View("s10login");
        }

        [HttpPost]
        public ActionResult s10login(string userName, string password)
        {
            LoginFormData theLoginFormData = new LoginFormData()
            {
                userName = userName,
                password = password
            };

            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            TranslateBox tBox = this.setTbox("RU");
            ViewBag.tBox = tBox;
            ViewBag.theLoginFormData = theLoginFormData;

            if (!theLoginFormData.isValid())
            {
                return View("s10login");
            }
            else
            {
                Employee theEmployee = db.Employees.Single(anEmp => anEmp.userName == userName);
                Session["loggedinUser"] = theEmployee;

                Response.Redirect("/Staff/s21Dashboard");

                return null;  //Fake return - as the Response.redirect activates a new request from the browser. 
            }
        }

        /// <summary>
        /// This is the vertical version of the dashboard. Currently not in use.
        /// </summary>
        /// <returns></returns>
        public ActionResult s20dashboard()
        {
            return View("s20dashboard");
        }
        
        /// <summary>
        /// This is the horizontal version of the dashboard. It shows the orders of all apartments for 31 days, since the date entered by the user, 
        /// or since today. (the default)
        /// </summary>
        /// <param name="fromDate">Date entered in the date picker, or null</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult s21Dashboard(DateTime? fromDate)
        {
            Employee theEmployee = (Employee)Session["loggedinUser"];
            if (theEmployee == null)
            {
                //No user is logged in - go to login screen
                return s10login();
            }
            else
            {
                FakeDateTime.SetFakeTime(new DateTime(2018, 09, 20));  //DEBUGGING!!!!!!
                DateTime fromDateOrNow = fromDate ?? FakeDateTime.Now;
                var apartmentDayBlocks = s21dashboardPreparation(fromDateOrNow);
                ViewBag.apartmentDayBlocks = apartmentDayBlocks;
                TranslateBox tBox = this.setTbox("RU");
                ViewBag.tBox = tBox;
                ViewBag.fromDate = fromDateOrNow;
                ViewBag.today = FakeDateTime.Now;

                ViewBag.employee = theEmployee;
                return View("s21Dashboard");
            }
            
        }

        /// <summary>
        /// The function reads the orders for all apartments starting from the date set by the user and for 31 days.
        /// </summary>
        /// <param name="fromDateOrNull"></param>
        /// <returns>List of apartment orders. For each apartment:
        /// list of DayBlocks.
        /// A dayBlock is either a single free day, or an order which can span 1 or more days. Note that a day block may not 
        /// be identical to the corresponding order because the order may start before the "fromDate" or end after the "fromDate+31".</returns>
        public List<List<DayBlock>> s21dashboardPreparation(DateTime fromDate)
        {
            //for each apartment
            //for each day from from_date to from_date+3
            //lastOrderId = 0;
            //orderLength = 0;
            //apartmentDay = read apartmentDay record for apartment and day
            //if record does not exist or record status == free - this is a free day - add a free block
            //order = apartmentDay->order
            //if order.isFirstDay - create a new busy block
            //add the day to the busy block
            //if order.isLastDay - write busy block
            //write last busy block

            //a 2 dimensional array - a list of apartments, and for each apartment - a list of day blocks
            //where each day block is either a free day or an order.

            var apartmentDayBlocks = new List<List<DayBlock>>();
            var lastDate = fromDate.AddDays(31);
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            foreach (var anApartment in db.Apartments)
            {
                var dayBlocks = new List<DayBlock>();
                DayBlock aDayBlock = null;
                for (var aDate = fromDate; aDate <= lastDate; aDate = aDate.AddDays(1))
                {
                    var anApartmentDay = db.ApartmentDays.SingleOrDefault(record => record.Apartment.Id == anApartment.Id && record.date == aDate);
                    if (anApartmentDay == null || anApartmentDay.status == ApartOccuStatus.Free)
                    {
                        //This is a free day
                        if (aDayBlock != null)
                        {
                            //Although this should not occur (assuming that the apartmentDays table matches the orders checkin and checkout dates)
                            //But anyway - we will write the dayBlock to the list
                            dayBlocks.Add(aDayBlock);
                        }
                        aDayBlock = new DayBlock() { apartmentNumber = anApartment.number, orderStatus = null, firstDate = aDate.ToString("yyyy-MM-dd") };//null order status denotes a free day
                        dayBlocks.Add(aDayBlock);
                        aDayBlock = null;
                    }
                    else
                    {
                        //this is a busy day. Read the order record
                        var anOrder = db.Orders.Single(record => record.Id == anApartmentDay.Order.Id);
                        if (aDayBlock == null)
                        {
                            //This is the first time we see this order - open a new DayBlock object. Note that if the report starts from 
                            //1/12/2019 and we have an order that started on 39/11/2019 and continued to 4/12/2019 - then the first time we 
                            //encounted this order is not in the checkin date of it.
                            aDayBlock = new DayBlock()
                            {
                                apartmentNumber = anApartment.number,
                                days = 1,
                                name = anOrder.Guest.name,
                                orderId = anOrder.Id,
                                orderStatus = anOrder.status,
                                firstDate = aDate.ToString("yyyy-MM-dd")
                            };
                        }
                        else
                        {
                            //This is a continuation of the order - increment the number of days
                            aDayBlock.days++;
                        }
                        if (anOrder.isLastDay(aDate))
                        {
                            //This is the last day of the order - write the day block
                            dayBlocks.Add(aDayBlock);
                            aDayBlock = null;
                        }
                    }
                }
                //At this point we finished going on all dates for a specific apartment. It is possible that the last DayBlock was not yet written
                //if its checkout date is beyond the last day of the report (our report is from 1/12/2019 till 31/12/2019, but the checkout date
                //of the last order is 2/1/2020)
                if (aDayBlock != null)
                {
                    dayBlocks.Add(aDayBlock);
                    aDayBlock = null;
                }
                //Add the dayBlocks list into the apartmentDayBlocks
                apartmentDayBlocks.Add(dayBlocks);
            }
            //At this point the apartmentDayBlocks variable contaiins a list of list of day blocks 
            return apartmentDayBlocks;

        }
        [HttpGet]
        public PartialViewResult s22OrderDetails(int orderId)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            var theOrder = db.Orders.Single(a => a.Id == orderId);
            OrderData theOrderData = new OrderData()
            {
                orderId = theOrder.Id,
                name = theOrder.Guest.name,
                phone = theOrder.Guest.phone,
                email = theOrder.Guest.email,
                adults = theOrder.adultCount,
                children = theOrder.childrenCount,
                checkin = theOrder.checkinDate,
                checkout = theOrder.checkoutDate,
                country = theOrder.Guest.Country.name,
                expectedArrival = theOrder.expectedArrival,
                price = theOrder.priceAsMoney(),
                paid = theOrder.amountPaidAsMoney(),
                comments = theOrder.specialRequest,
                nights = theOrder.nights,
                bookedBy = theOrder.bookedBy,
                apartmentNumber = theOrder.Apartment.number
            };
            TranslateBox tBox = this.setTbox("RU");
            ViewBag.tBox = tBox;
            ViewBag.orderData = theOrderData;
            PartialViewResult result = PartialView("s22OrderDetails");
            return result;
            //JsonResult jResult = Json(theOrderData, JsonRequestBehavior.AllowGet);
            //return jResult;
        }

        /// <summary>
        /// Called when the user wants to update an existing order (after pressing an order on the dashboard)
        /// This is the get request part which displays the order form. The actual update is done by s25addUpdateOrder()
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>update order form</returns>
        [HttpGet]
        public ActionResult s23updateOrder(int? orderId)
        {

            if (orderId != null)
            {
                cityLifeDBContainer1 db = new cityLifeDBContainer1();
                var theOrder = db.Orders.Single(a => a.Id == orderId);
                ViewBag.order = theOrder;
            }
            TranslateBox tBox = this.setTbox("RU");
            ViewBag.tBox = tBox;
            return View("s23addUpdateOrder");
        }

        /// <summary>
        /// Called when the user wants to add a new Order. (when he presses an empty cell on the dashboard).
        /// This is the get request part which displays the order form. The actual update is done by s25addUpdateOrder()
        /// </summary>
        /// <param name="checkin">checkin date (yyyy--mm-dd - converted to DateTime</param>
        /// <param name="checkout">checkout date</param>
        /// <param name="nights">number of nights</param>
        /// <param name="apartmentNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult s24addOrder(DateTime checkin, DateTime checkout, int nights, int apartmentNumber)
        {
            Employee theEmployee = (Employee)Session["loggedinUser"];
            if (theEmployee == null)
            {
                //No user is logged in - go to login page
                return s10login();
            }
            ViewBag.employee = theEmployee;
            OrderData theOrderData = new OrderData()
            {
                checkin = checkin,
                checkout = checkout,
                apartmentNumber = apartmentNumber,
                nights = Order.dateDifference(checkout,checkin),
                adults = 2,
                children = 0
            };
            cityLifeDBContainer1 db = new cityLifeDBContainer1();

            //Calculate expected price, assuming 2 adults and 0 children. Calculate the price in UAH
            BookingRequest theBookingRequest = new BookingRequest()
            {
                 checkinDate = checkin,
                 checkoutDate = checkout,
                 adults = 2,
                 children = 0
            };
            Currency theCurrency = db.Currencies.Single(a => a.currencyCode == "UAH");
            Apartment theApartment = db.Apartments.Single(a => a.number == apartmentNumber);
            var thePrice = PublicController.calculatePricePerStayForApartment(theApartment, db, theBookingRequest, theCurrency);
            if (thePrice.availability != Availability.AVAILABLE)
            {
                //The apartment is not available for that period. 
                theOrderData.price = new Money(0m, "UAH");
            }
            else
            {
                theOrderData.price = thePrice.pricePerStay;
            }
           
            theOrderData.paid = new Money(0m, "UAH");

            TranslateBox tBox = this.setTbox("RU");
            ViewBag.tBox = tBox;
            ViewBag.countries = db.Countries;
            ViewBag.orderData = theOrderData;
            return View("s23addUpdateOrder");
        }

        /// <summary>
        /// This method is called when the user finished adding or updating an order. This method performs validity checks and if OK - 
        /// updates or adds the Order. Otherwise - sends error messages.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult s25addUpdateOrder(int orderId, int apartmentNumber, string Email, string Name, string Country, string Phone, string ArrivalTime, 
            string SpecialRequest, DateTime CheckinDate, DateTime CheckoutDate, int Adults, int Children, int Nights, string Price, string Paid, 
            string BookedBy, int confirmationNumber)
        {
            //Perform validity checks on the input
           // StaffBookingFormData theBookingForm = new StaffBookingFormData(
                
                
            Money priceM = new Money(Price,"UAH");  //Default currency is UAH, if the currency symbol does not exist.
            Money paidAmountM = new Money(Paid,"UAH");

            //prepareDataForp26p27(apartmentId);
            TranslateBox tBox = new TranslateBox("UAH", "UAH", "dontShowAsterisks");
            ViewBag.tBox = tBox;
            
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            Apartment theAparatment = db.Apartments.Single(anApartment => anApartment.number == apartmentNumber);
            BookingRequest theBookingRequest = new BookingRequest()
            {
                checkinDate = CheckinDate,
                checkoutDate = CheckoutDate,
                adults = Adults,
                children = Children
            };
            ViewBag.countries = db.Countries;
            ViewBag.employee = (Employee)Session["loggedinUser"];

            //perform validity chekcs on the input
            OrderData theOrderData = new OrderData()
            {
                name = Name,
                country = Country,
                email = Email,
                phone = Phone,
                expectedArrival = ArrivalTime,
                comments = SpecialRequest,
                adults =  Adults,
                children = Children,
                checkin = CheckinDate,
                checkout = CheckoutDate,
                 apartmentNumber = apartmentNumber,
                 bookedBy = BookedBy,
                 confirmationNumber = confirmationNumber,
                 nights = Nights,
                 orderId = orderId,
                 paid = paidAmountM,
                 price = priceM
            };

            ViewBag.orderData = theOrderData;
            if (!theOrderData.isValid())
            {
                return View("s23addUpdateOrder");
            }
            else
            {
                //The form is valid - create the booking order
                //Check if the booking request is still valid.

                Currency currentCurrency = db.Currencies.Single(a => a.currencyCode == "UAH");   //The staff application works currently only with UAH
                ApartmentPrice apartmentAndPrice = PublicController.calculatePricePerStayForApartment(theAparatment, db, theBookingRequest, currentCurrency);
                Country theCountry = db.Countries.Single(a => a.name == Country);
                if (apartmentAndPrice.availability == Availability.OCCUPIED)
                {
                    //the apartment is not available, although it seemed to be available. Perhaps it was taken in the last minutes
                    theOrderData.setErrorMessageFor("comments", "These dates are not available for that apartment");
                   return View("s23addUpdateOrder");
                }
                else
                {
                    //Complete the booking
                    Order theOrder = PublicController.p27createOrder(db, theBookingRequest, apartmentAndPrice, theOrderData, theCountry, tBox);

                    ViewBag.theOrder = theOrder;
                    ViewBag.apartmentAndPrice = apartmentAndPrice;

                    if (theOrder.Guest.email != null && theOrder.Guest.email != "")
                    {
                        EmailMessage mailToCustomer = new EmailMessage(
                        to: theOrder.Guest.email,
                        subject: tBox.translate("Welcome to Kharkov Apartments City Life"),
                        mailName: "t10welcomeMail",
                        theController: this
                        );
                        mailToCustomer.send();
                    }
                    return View("s28bookingSuccess");
                }
            }



            return View();
        }
        /// <summary>
        /// This method is an auxiliary method to create the translation box and to insert it if needed to the Session variable
        /// It was copied from the public controller. A more elegant solution would be to create one method that will get the 
        /// controller as a pararmeter.
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
    }
}