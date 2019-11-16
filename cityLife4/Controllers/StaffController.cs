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
    public class LoginFormData : formData
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
    /// it adds 2 properties:
    /// days - that may be different than the "nights" property of OrderData, because if the report starts after the beginning or ends before the
    /// end of the order - the "days" property will reflect the number of days the order will be shown in the report.
    /// firstDate - the first date in the report. May not be the same as checkIn property
    ///  
    /// </summary>
    public class DayBlock : OrderData
    {
        public DayBlock()
        { }
        public DayBlock(Order anOrder) : base(anOrder)
        { }
        public int days;
        public string firstDate;
    }




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
        /// <param name="wideDashboard">When "on" means that the user wants to get the wide version of the dashboard.
        /// In this mode, each column is wider (about 4 normal columns), we show only 3 days, and we put more data into\
        /// each order element.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult s21Dashboard(DateTime? fromDate, string wideDashboard = "off")
        {
            DateTime startDate;
            if (fromDate == null && Session["fromDate"] == null)
            {
                //Take yesterda's date as default
                startDate = FakeDateTime.DateNow.AddDays(-1);
                Session["fromDate"] = startDate;
            }
            else if (fromDate == null)
            {
                //fromDate is null but we have a date in the session - take it
                startDate = (DateTime)Session["fromDate"];
            }
            else
            {
                //We have a date in "fromDate" - save it in the session variable
                startDate = (DateTime)fromDate;
                Session["fromDate"] = startDate;
            }
            //At this point startDate contains a date (and also Session[fromDate"] - contains the same date

            //Currntly for both normal mode and wide mode we dislay 31 days. The number of days can be set here.
            //We display one month. The calculation is: the number of days displayed are equal to the month length of the starting month. 
            //In most cases this will yield an end date which is one less that the starting date. The only exception is January 30 and 31, which will 
            //end at 1 or 2 of March.
            //For example: start date    end date
            //             10/11/2019    9/12/2019
            //             31/12/2019    30/1/2020
            //             30/1/2019     1/3/2019 (except for leap year, where it will be 29/2/2020)
            //             1/11/2019     30/11/2019
            //             1/12/2019     31/12/2019
            //             1/2/2020      29/2/2020 (2020 is leap year)
            int dashboardDays = FakeDateTime.monthLength(startDate);
            Employee theEmployee = (Employee)Session["loggedinUser"];

            if (theEmployee == null)
            {
                //No user is logged in - go to login screen
                return s10login();
            }
            else
            {
                List<Money> revenuePerDay = null;
                EmployeeWorkDay[] empWorkDaysArray = null;
                List<Employee> maidList = null;
                List<Money> revenuePerApartment = null;
                List<int> percentOccupancyPerApartment = null;
                var apartmentDayBlocks = s21dashboardPreparation(
                    startDate, 
                    dashboardDays, 
                    ref revenuePerDay,
                    ref revenuePerApartment,
                    ref percentOccupancyPerApartment,
                    ref empWorkDaysArray, 
                    ref maidList);
                ViewBag.apartmentDayBlocks = apartmentDayBlocks;
                ViewBag.revenuePerDay = revenuePerDay;
                ViewBag.revenuPerApartment = revenuePerApartment;
                ViewBag.percentOccupancyPerApartment = percentOccupancyPerApartment;
                ViewBag.empWorkDaysArray = empWorkDaysArray;
                TranslateBox tBox = this.setTbox("RU");
                ViewBag.tBox = tBox;
                ViewBag.fromDate = startDate;
                ViewBag.today = FakeDateTime.Now;
                ViewBag.wideDashboard = wideDashboard == "on" ? "checked" : "";
                ViewBag.dashboardDays = dashboardDays;
                ViewBag.maidList = maidList;

                ViewBag.employee = theEmployee;
                if (Session["lastOrderDetails"] != null)
                {
                    ViewBag.highlightOrderId = (int)Session["lastOrderDetails"];
                }
                else
                {
                    ViewBag.highlightOrderId = 0;
                }
                return View("s21Dashboard");
            }



        }

        /// <summary>
        /// The function reads the orders for all apartments starting from the date set by the user and for a full month (30 or 31 days,
        /// and for February 28 or 29 days)
        /// </summary>
        ///<param name="fromDate">starting date of the dashboard</param>
        ///<param name="days">the number of days we wish to display.(depends on the starting month)</param>
        ///<param name="revenuePerDay">An output parameter - will contain the list of revenues per day</param>
        ///<param name="percentOccupancyPerApartment">Number of days the apartment is occupied divided by total number of days (rounded to 
        ///whole percent)</param>
        ///<param name="revenuePerApartment">Total revenue per apartment for that month</param>
        ///<param name="empWorkDaysArray">An array containing an employeeWorkDay record for each day in the month.
        ///Days for which no record found - will be null. Days for which more than one recrod found - will contain
        ///the last record. </param>
        ///<param name="maidList">A list of maids (employees of role="maid") - ouitput parameter</param>
        /// <returns>List of apartment orders. For each apartment:
        /// list of DayBlocks.
        /// A dayBlock is either a single free day, or an order which can span 1 or more days. Note that a day block may not 
        /// be identical to the corresponding order because the order may start before the "fromDate" or end after the "fromDate+31".</returns>
        public List<List<DayBlock>> s21dashboardPreparation(DateTime fromDate,
            int days,
            ref List<Money> revenuePerDay,
            ref List<Money> revenuePerApartment,
            ref List<int> percentOccupancyPerApartment,
            ref EmployeeWorkDay[] empWorkDaysArray,
            ref List<Employee> maidList)
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
            revenuePerDay = new List<Money>();
            for (int i = 0; i < days; i++)
            {
                revenuePerDay.Add(new Money(0m, "UAH"));
            }

            var lastDate = fromDate.AddDays(days - 1);
            revenuePerApartment = new List<Money>();
            percentOccupancyPerApartment = new List<int>();
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            foreach (var anApartment in db.Apartments)
            {
                var dayBlocks = new List<DayBlock>();
                DayBlock aDayBlock = null;
                int dayNumber = 0;
                Money apartmentRevenue = new Money(0m, "UAH");
                double apartmentOccupiedDays = 0;   //Use float for the percentage calculation later
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
                        aDayBlock = new DayBlock() { apartmentNumber = anApartment.number, status = OrderStatus.Free, firstDate = aDate.ToString("yyyy-MM-dd") };//"Free" status denotes a free day
                        dayBlocks.Add(aDayBlock);
                        aDayBlock = null;
                    }
                    else
                    {
                        //this is a busy day. Read the order record
                        var anOrder = db.Orders.Single(record => record.Id == anApartmentDay.Order.Id);
                        //Add the revenue of that day to the total revenu per day
                        revenuePerDay[dayNumber] += anApartmentDay.revenueAsMoney();
                        apartmentRevenue += anApartmentDay.revenueAsMoney();
                        apartmentOccupiedDays++;
                        if (aDayBlock == null)
                        {
                            //This is the first time we see this order - open a new DayBlock object. Note that if the report starts from 
                            //1/12/2019 and we have an order that started on 39/11/2019 and continued to 4/12/2019 - then the first time we 
                            //encounted this order is not in the checkin date of it.
                            aDayBlock = new DayBlock(anOrder)
                            {
                                days = 1,
                                firstDate = aDate.ToString("yyyy-MM-dd"),
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
                    dayNumber++;
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
                //Add the apartment revenue and apartment occupacy percentage
                revenuePerApartment.Add(apartmentRevenue);
                double apartmentOccupancyPercent = apartmentOccupiedDays / days * 100.0;
                int apartmentOccupancyPercentRounded = (int)Math.Round(apartmentOccupancyPercent);
                percentOccupancyPerApartment.Add(apartmentOccupancyPercentRounded);
            }
            //At this point the apartmentDayBlocks variable contaiins a list of list of day blocks 

            //Calculate the list of employee work days. The list contains a single record for each day (or null, if no employee is assigned
            //for that day). If 2 employees are assigned for the same day - only one is taken (the last one)
            //empWorkDaysList = new List<EmployeeWorkDay>();
            empWorkDaysArray = new EmployeeWorkDay[days + 1];
            var empWorkDays = from anEmpWorkDay in db.EmployeeWorkDays
                              where anEmpWorkDay.dateAndTime >= fromDate && anEmpWorkDay.dateAndTime <= lastDate
                              orderby anEmpWorkDay.dateAndTime
                              select anEmpWorkDay;
            foreach (var anEmpWorkDays in empWorkDays)
            {
                int dayNumber = (int)Math.Round((anEmpWorkDays.dateAndTime.Date - fromDate).TotalDays, 0);
                empWorkDaysArray[dayNumber] = anEmpWorkDays;
            }
            maidList = db.Employees.Where(emp => emp.role == "maid").ToList();  //Add all maids to the maid list

            return apartmentDayBlocks;

        }
        [HttpGet]
        public PartialViewResult s22OrderDetails(int orderId)
        {
            Session["lastOrderDetails"] = orderId;  //Keep the data so we know which order to highlight
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            var theOrder = db.Orders.Single(a => a.Id == orderId);
            OrderData theOrderData = new OrderData(theOrder);

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
        public ActionResult s23updateOrder(int orderId)
        {


            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            Order theOrder = db.Orders.Single(a => a.Id == orderId);
            OrderData theOrderData = new OrderData(theOrder);
            ViewBag.orderData = theOrderData;
            var apartmentNumbers = from anApartment in db.Apartments
                                   select anApartment.number;
            ViewBag.apartmentNumbers = apartmentNumbers;

            TranslateBox tBox = this.setTbox("RU");
            ViewBag.tBox = tBox;
            ViewBag.countries = db.Countries;
            var theEmployee = (Employee)Session["loggedinUser"];
            ViewBag.employee = theEmployee;
            ViewBag.action = "Update";
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
                nights = Order.dateDifference(checkout, checkin),
                adults = 2,
                children = 0,
                country = ""
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
                theOrderData.price = "0";
            }
            else
            {
                theOrderData.price = thePrice.pricePerStay.toMoneyString(showCents: false);
            }

            theOrderData.paid = "";
            var apartmentNumbers = from anApartment in db.Apartments
                                   select anApartment.number;
            ViewBag.apartmentNumbers = apartmentNumbers;

            TranslateBox tBox = this.setTbox("RU");
            ViewBag.tBox = tBox;
            ViewBag.countries = db.Countries;
            ViewBag.orderData = theOrderData;
            ViewBag.action = "Add";
            return View("s23addUpdateOrder");
        }

        /// <summary>
        /// This method is called when the user finished adding or updating an order. This method performs validity checks and if OK - 
        /// updates or adds the Order. Otherwise - sends error messages.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult s25addUpdateOrder(int orderId, int apartmentNumber, string Email, string Name, string Country, string Phone, string ArrivalTime,
            string SpecialRequest, DateTime CheckinDate, DateTime CheckoutDate, int Adults, int Children, string Price, string Paid,
            string BookedBy, string confirmationNumber, OrderStatus status, Color orderColor, string staffComments)
        {
            //Perform validity checks on the input
            TranslateBox tBox = new TranslateBox("RU", "RU", "dontShowAsterisks");
            ViewBag.tBox = tBox;
            if (orderId == 0)
            {
                //This is a new order
                ViewBag.action = "Add";
            }
            else
            {
                ViewBag.action = "Update";
            }

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

            int nights = (int)Math.Round((CheckoutDate - CheckinDate).TotalDays, 0);
            //perform validity chekcs on the input
            OrderData theOrderData = new OrderData()
            {
                name = Name,
                country = Country,
                email = Email,
                phone = Phone,
                expectedArrival = ArrivalTime,
                comments = SpecialRequest,
                adults = Adults,
                children = Children,
                checkin = CheckinDate,
                checkout = CheckoutDate,
                apartmentNumber = apartmentNumber,
                bookedBy = BookedBy,
                confirmationNumber = confirmationNumber,
                nights = nights,
                orderId = orderId,
                paid = Paid,
                price = Price,
                status = status,
                orderColor = orderColor,
                staffComments = staffComments
            };

            var apartmentNumbers = from anApartment in db.Apartments
                                   select anApartment.number;
            ViewBag.apartmentNumbers = apartmentNumbers;
            ViewBag.orderData = theOrderData;
            if (!theOrderData.isValid())
            {
                return View("s23addUpdateOrder");
            }
            else
            {
                //The form is valid - create the booking order
                //Check if the booking request is still valid.
                Money priceM = new Money(Price, "UAH");  //Default currency is UAH, if the currency symbol does not exist.
                Money paidAmountM = new Money(Paid, "UAH");

                Currency currentCurrency = db.Currencies.Single(a => a.currencyCode == "UAH");   //The staff application works currently only with UAH
                ApartmentPrice apartmentAndPrice = PublicController.calculatePricePerStayForApartment(theAparatment, db, theBookingRequest, currentCurrency, orderId);
                Country theCountry = db.Countries.SingleOrDefault(a => a.name == Country);
                if (apartmentAndPrice.availability == Availability.OCCUPIED)
                {
                    //the apartment is not available, although it seemed to be available. Perhaps it was taken in the last minutes
                    theOrderData.setErrorMessageFor("comments", "These dates are not available for that apartment");
                    return View("s23addUpdateOrder");
                }
                else
                {
                    //Complete the booking
                    apartmentAndPrice.pricePerStay = priceM;  //enter the actual price to be paid, rather than the calculated price.
                    Order theOrder = PublicController.p27createUpdateOrder(db, theBookingRequest, apartmentAndPrice, theOrderData, theCountry, tBox);

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
                        mailToCustomer.sendAsync();
                    }

                    if (orderId == 0)
                    {
                        //This is a new order - display the "booking success" screen
                        return View("s28bookingSuccess");
                    }
                    else
                    {
                        //This is an order update - show the grid, where this order will be highlighted. (TBD)
                        string fromDate = ((DateTime)Session["fromDate"]).ToString("yyyy/MM/dd");
                        Response.Redirect("/staff/s21Dashboard?fromDate=" + fromDate);
                        return View();   //fictitious, as the server.transfer moves control to another controller
                    }

                }
            }
        }

        [HttpPost]
        public string s26deleteOrder(int orderId)
        {
            var tBox = setTbox("RU");
            try
            {
                cityLifeDBContainer1 db = new cityLifeDBContainer1();

                var theApartmentDays = from anApartmentDays in db.ApartmentDays
                                       where anApartmentDays.Order.Id == orderId
                                       select anApartmentDays;
                foreach (var anApartmentDays in theApartmentDays)
                {
                    anApartmentDays.Order = null;
                    anApartmentDays.status = ApartOccuStatus.Free;
                }

                //Delete the order record
                var theOrder = db.Orders.Single(a => a.Id == orderId);
                db.Orders.Remove(theOrder);
                db.SaveChanges();

                return tBox.translate("order") + " " + orderId + " " + tBox.translate("deleted");
            }
            catch (Exception e)
            {
                AppException.writeException(119, e, e.StackTrace, orderId);
                return tBox.translate("order") + " " + orderId + " " + tBox.translate("could not be deleted");
            }

        }
        [HttpGet]
        public JsonResult s27checkBookingPrice(DateTime checkinDate, DateTime checkoutDate, int adults, int children, int apartmentNumber, int orderId)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            Apartment theApartment = db.Apartments.Single(a => a.number == apartmentNumber);

            BookingRequest theBookingRequest = new BookingRequest()
            {
                checkinDate = checkinDate,
                checkoutDate = checkoutDate,
                adults = adults,
                children = children
            };
            Currency theCrrency = db.Currencies.Single(a => a.currencyCode == "UAH");
            ApartmentPrice apartmentAndPrice = PublicController.calculatePricePerStayForApartment(theApartment, db, theBookingRequest, theCrrency, orderId);
            string availabilityName = Enum.GetName(typeof(Availability), apartmentAndPrice.availability);

            //If the apartment is occupied, the price will be null. Replace it with 0
            Money thePrice = apartmentAndPrice.pricePerStay == null ? new Money(0m, "UAH") : apartmentAndPrice.pricePerStay;
            var priceInfo = new { price = thePrice.toMoneyString(showCents: false), availability = availabilityName, nights = apartmentAndPrice.nightCount };
            JsonResult jResult = Json(priceInfo, JsonRequestBehavior.AllowGet);
            return jResult;


        }

        [HttpPost]
        public void s28AddUpdateMaid(int maidId, DateTime date)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            EmployeeWorkDay empWorkDay = db.EmployeeWorkDays.SingleOrDefault(a => a.dateAndTime == date);
            Employee theMaid = db.Employees.SingleOrDefault(a => a.Id == maidId);

            if (maidId == 0 && empWorkDay == null)
            {
                //no maid for assigned to this day. The day was not assigned before. do nothing
            }
            else if (maidId == 0)
            {
                //the day is unassigned from a previously assigned maid - delete the employeeWorkDay record
                db.EmployeeWorkDays.Remove(empWorkDay);
            }
            else if (empWorkDay == null)
            {
                //no employee was previously assigned to this day - add the new employee
                Currency theCurrency = db.Currencies.Single(a => a.currencyCode == "UAH");
                empWorkDay = new EmployeeWorkDay()
                {
                    dateAndTime = date,
                    Employee = theMaid,
                    Currency = theCurrency,
                    hours = 0,
                    isSalaryDay = false,
                    salaryCents = 0
                };
                db.EmployeeWorkDays.Add(empWorkDay);
            }
            else
            {
                //Another employee was assigned to this day - replace him by the new employee
                empWorkDay.Employee = theMaid;
            }
            db.SaveChanges();
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
                                        defaultLanguageCode: "EN",
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