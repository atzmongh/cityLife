using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using cityLife;
using cityLife4;


using System.Text.RegularExpressions;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data;
using System.Web.WebSockets;

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

    public class RevenueAndOccupancy
    {
        public Money revenue { get; private set; } = new Money(0m, "UAH");
        public int occupiedDays { get; private set; } = 0;

        private int totalDaysInMonth = 30;
        /// <summary>
        /// number of days in the month represented by this 
        /// </summary>
        /// <param name="totalDays"></param>
        public RevenueAndOccupancy(int totalDays)
        {
            if (totalDays > 0)
            {
                totalDaysInMonth = totalDays;
            }
        }
        public double percentOccupancy()
        {
            return (double)occupiedDays / totalDaysInMonth;
        }
        /// <summary>
        /// Add revenue for a single day. We assume that each day where we had a revenue - is also an occupied
        /// day, so we increase the occupied days count.
        /// </summary>
        /// <param name="amount"></param>
        public void addDaysRevenue(Money amount)
        {
            revenue += amount;
            occupiedDays++;
        }
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
        /// <param name="hotel1">Currently we assume that there are exactly 3 hotels with IDs 1,2,3
        /// when "on" - this hotel should be displayed. When "off" or null - this hotel should not be 
        /// displayed</param>
        /// <param name="wideDashboard">When "on" means that the user wants to get the wide version of the dashboard.
        /// In this mode, each column is wider (about 4 normal columns), we show only 3 days, and we put more data into\
        /// each order element.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult s21Dashboard(DateTime? fromDate, 
            string hotel1, string hotel2, string hotel3,
            string wideDashboard = "off")
        {
            //if this is an initial request for the dashboard (or the user 
            //unchecked all hotels) - check if a cooky exists with the 
            //default value for hotel information
            //if the cookie exists - insert its value to the variable. Otherwise - insert "off"
            if (hotel1 == null && hotel2 == null && hotel3 == null)
            {
                hotel1 = (HttpContext.Request.Cookies["hotel1"]?.Value) ?? "off";
                hotel2 = (HttpContext.Request.Cookies["hotel2"]?.Value) ?? "off";
                hotel3 = HttpContext.Request.Cookies["hotel3"]?.Value ?? "off";
            }
            //At this point hotel1 will contain either "on" or "off". Same for hotel2 and hotel3
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

            //Currntly for both normal mode and wide mode we dislay a full month. The number of days can be set here.
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
                List<Money> expensePerDay = null;
                List<string> expenseTypes = null;
                EmployeeWorkDay[] empWorkDaysArray = null;
                List<Employee> maidList = null;
                List<Money> revenuePerApartment = null;
                List<double> aveargeDaysPerApartment = null;
                List<int> percentOccupancyPerApartment = null;
                List<Hotel> hotels = null;
                List<int> hotelIds = new List<int>();
                //here again we assume 3 hotels, with IDs 1,2,3
                if (hotel1 == "on")
                    hotelIds.Add(1);
                if (hotel2 == "on")
                    hotelIds.Add(2);
                if (hotel3 == "on")
                    hotelIds.Add(3);

                var apartmentDayBlocks = s21dashboardPreparation(
                    startDate,
                    dashboardDays,
                    hotelIds,
                    ref revenuePerDay,
                    ref expensePerDay,
                    ref expenseTypes,
                    ref revenuePerApartment,
                    ref aveargeDaysPerApartment,
                    ref percentOccupancyPerApartment,
                    ref empWorkDaysArray,
                    ref maidList,
                    ref hotels);
                ViewBag.apartmentDayBlocks = apartmentDayBlocks;
                ViewBag.hotelIds = hotelIds;
                ViewBag.revenuePerDay = revenuePerDay;
                ViewBag.expensePerDay = expensePerDay;
                ViewBag.expenseTypes = expenseTypes;
                ViewBag.revenuPerApartment = revenuePerApartment;
                ViewBag.aveargeDaysPerApartment = aveargeDaysPerApartment;
                ViewBag.percentOccupancyPerApartment = percentOccupancyPerApartment;
                ViewBag.empWorkDaysArray = empWorkDaysArray;
                TranslateBox tBox = this.setTbox("RU");
                ViewBag.tBox = tBox;
                ViewBag.fromDate = startDate;
                ViewBag.today = FakeDateTime.Now;
                ViewBag.wideDashboard = wideDashboard == "on" ? "checked" : "";
                ViewBag.dashboardDays = dashboardDays;
                ViewBag.maidList = maidList;
                ViewBag.hotels = hotels;

                ViewBag.employee = theEmployee;
                if (Session["lastOrderDetails"] != null)
                {
                    ViewBag.highlightOrderId = (int)Session["lastOrderDetails"];
                }
                else
                {
                    ViewBag.highlightOrderId = 0;
                }
                HttpCookie hotel1Cookie = new HttpCookie("hotel1", hotel1);
                hotel1Cookie.Expires = FakeDateTime.Now.AddDays(120);
                Response.Cookies.Add(hotel1Cookie);
                HttpCookie hotel2Cookie = new HttpCookie("hotel2", hotel2);
                hotel2Cookie.Expires = FakeDateTime.Now.AddDays(120);
                Response.Cookies.Add(hotel2Cookie);
                HttpCookie hotel3Cookie = new HttpCookie("hotel3", hotel3);
                hotel3Cookie.Expires = FakeDateTime.Now.AddDays(120);
                Response.Cookies.Add(hotel3Cookie);
                return View("s21Dashboard");
            }



        }

        /// <summary>
        /// The function reads the orders for all apartments starting from the date set by the user and for a full month (30 or 31 days,
        /// and for February 28 or 29 days)
        /// </summary>
        ///<param name="fromDate">starting date of the dashboard</param>
        ///<param name="days">the number of days we wish to display.(depends on the starting month)</param>
        ///<param name="hotelIds">list of hotel IDs in the current filter. Either what the user has clicked
        ///or what has been kept in the cookie.</param>
        ///<param name="revenuePerDay">An output parameter - will contain the list of revenues per day</param>
        ///<param name="expensePerDay">total expenses for each day</param>
        ///<param name="expenseTypes">list of expense types</param>
        ///<param name="percentOccupancyPerApartment">Number of days the apartment is occupied divided by total number of days (rounded to 
        ///whole percent)</param>
        ///<param name="revenuePerApartment">Total revenue per apartment for that month</param>
        ///<param name="averageDaysPerApartment">contains the average number of days per rent for each apartment. We include in this average
        ///only rents that have started in the displayed month. IF a rent started in that month and spans to the next month
        ///the average takes into account the total time for that rent - not only the portion will falls in this month</param>
        ///<param name="empWorkDaysArray">An array containing an employeeWorkDay record for each day in the month.
        ///Days for which no record found - will be null. Days for which more than one recrod found - will contain
        ///the last record. </param>
        ///<param name="maidList">A list of maids (employees of role="maid") - ouitput parameter</param>
        ///<param name="hotels">list of all hotels in the system</param>
        /// <returns>List of apartment orders. For each apartment:
        /// list of DayBlocks.
        /// A dayBlock is either a single free day, or an order which can span 1 or more days. Note that a day block may not 
        /// be identical to the corresponding order because the order may start before the "fromDate" or end after the "fromDate+31".
        /// Note  that the list contains first all real apartments, then "waiting" apartments</returns>
        public List<List<DayBlock>> s21dashboardPreparation(DateTime fromDate,
            int days,
            List<int> hotelIds,
            ref List<Money> revenuePerDay,
            ref List<Money> expensePerDay,
            ref List<string> expenseTypes,
            ref List<Money> revenuePerApartment,
            ref List<double> averageDaysPerApartment,
            ref List<int> percentOccupancyPerApartment,
            ref EmployeeWorkDay[] empWorkDaysArray,
            ref List<Employee> maidList,
            ref List<Hotel> hotels)
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
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            var apartmentDayBlocks = new List<List<DayBlock>>();
            revenuePerDay = new List<Money>();
            expensePerDay = new List<Money>();
            for (int i = 0; i < days; i++)
            {
                revenuePerDay.Add(new Money(0m, "UAH"));
            }


            var lastDate = fromDate.AddDays(days - 1);
            revenuePerApartment = new List<Money>();
            averageDaysPerApartment = new List<double>();
            percentOccupancyPerApartment = new List<int>();

            //Calculate the expenses for each date in the range
            for (DateTime aDate = fromDate; aDate <= lastDate; aDate = aDate.AddDays(1))
            {
                //Read all expenses for the date and sum them
                var expenseListCentsForDate = (from expense in db.Expenses
                                               where expense.date == aDate
                                               select expense.amount);   //The expenses are kept as cents in the DB
                int expensesCentsForDate = 0;
                if (expenseListCentsForDate.Count() > 0)
                {
                    expensesCentsForDate = expenseListCentsForDate.Sum();
                }
                Money expensesForDate = new Money(expensesCentsForDate, "UAH");
                expensePerDay.Add(expensesForDate);
            }

            //Sort apartments by type (first all "normal" apartments then the "waiting" apartments), 
            //then by hotel ID and then by their number
            var sortedApartments = from anApartment in db.Apartments
                                   orderby anApartment.type, anApartment.hotel_Id, anApartment.number
                                   where hotelIds.Contains(anApartment.hotel_Id)
                                   select anApartment;
            Order anOrder = new Order()   //create a fictitious order with id = 0
            {
                Id = 0
            };

            foreach (var anApartment in sortedApartments)
            {
                var dayBlocks = new List<DayBlock>();
                DayBlock aDayBlock = null;
                int dayNumber = 0;
                Money apartmentRevenue = new Money(0m, "UAH");
                double apartmentOccupiedDays = 0;   //Use float for the percentage calculation later
                double totalRents = 0.0;    //Counter for how many orders are during the month for that apartment.
                                            //We keep it as double in order to calculate the average (which should be in float)
                int totalRentDays = 0;      //the total amount of rented days for that apartment in that month. We take into account only rents that started
                                            //in the displayed month.
                                            //Get all apartment days of the current apartment for the desired month
                var apartmentDaysForMonth = (from theApartmentDay in db.ApartmentDays
                                             where theApartmentDay.Apartment.Id == anApartment.Id && theApartmentDay.date >= fromDate && theApartmentDay.date <= lastDate
                                             orderby theApartmentDay.date
                                             select theApartmentDay).ToList();
                int apartmentDaysI = 0;
                ApartmentDay anApartmentDay;
                int apartmentDaysCount = apartmentDaysForMonth.Count();
                for (var aDate = fromDate; aDate <= lastDate; aDate = aDate.AddDays(1))
                {
                    if (apartmentDaysCount > apartmentDaysI && apartmentDaysForMonth[apartmentDaysI].date == aDate)
                    {
                        //The current apartmentDays record matches the on-hand date - an apartmentDay exists
                        anApartmentDay = apartmentDaysForMonth[apartmentDaysI];
                        apartmentDaysI++;
                    }
                    else
                    {
                        //An apartment day does not exist - it will be null
                        anApartmentDay = null;
                    }

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
                        if (anOrder.Id != anApartmentDay.Order.Id)
                        {
                            //We did not read this order yet - read it
                            anOrder = db.Orders.Single(record => record.Id == anApartmentDay.Order.Id);
                            //Check if this order started today
                            if (anOrder.checkinDate == aDate)
                            {
                                //take this order into account for calculation of average days per rent
                                totalRents++;
                                totalRentDays += anOrder.dayCount;
                            }
                            else
                            {
                                //This is a new order but it started before the first day of the dispalyed month - do not take it into consideration
                                //for calculating average rent days.
                            }
                        }
                        else
                        {
                            //the order is for more than one day. We have already read this order in the previous cycle in the date loop
                        }
                        //At this point anOrder contains the order pointed by the on-hand apartmentDay record
                        //Add the revenue of that day to the total revenu per day
                        if (anApartmentDay.Apartment.number > 0)
                        {
                            //This is a real apartment (not a waiting list) - add its revenue to the day's revenue
                            revenuePerDay[dayNumber] += anApartmentDay.revenueAsMoney();
                        }
                        
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
                //Add the dayBlocks list into the apartmentDayBlocks. Check if it is a "waiting" apartment.
                apartmentDayBlocks.Add(dayBlocks);

                //Add the apartment revenue and apartment occupacy percentage - only for "normal" apartments
                revenuePerApartment.Add(apartmentRevenue);
                double apartmentOccupancyPercent = apartmentOccupiedDays / days * 100.0;
                int apartmentOccupancyPercentRounded = (int)Math.Round(apartmentOccupancyPercent);
                percentOccupancyPerApartment.Add(apartmentOccupancyPercentRounded);

                //calculate the average rent days per apartment
                double averageRentDays = 0.0;
                if (totalRents > 0)
                {
                    averageRentDays = totalRentDays / totalRents;
                }
                averageDaysPerApartment.Add(averageRentDays);
            }
            //At this point the apartmentDayBlocks variable contaiins a list of list of day blocks 

            //Calculate the list of employee work days. The list contains a single record for each day (or null, if no employee is assigned
            //for that day). If 2 employees are assigned for the same day - only one is taken (the last one)
            //empWorkDaysList = new List<EmployeeWorkDay>();
            empWorkDaysArray = new EmployeeWorkDay[days];
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

            expenseTypes = (from expenseType in db.ExpenseTypes
                            select expenseType.nameKey).ToList();
            hotels = db.Hotels.ToList();



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
                country = "",
                bookedBy = BookedBy.booking.ToString()
            };
            if (apartmentNumber <= 0)
            {
                //This is a waiting apartment - the default status should be "waiting list" and color orange (Otherwise it will stay as Created and red)
                theOrderData.status = OrderStatus.Waiting_list;
                theOrderData.orderColor = Color.Orange;
            }
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
            string bookedBy, string confirmationNumber, OrderStatus status, Color orderColor, string staffComments)
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
                bookedBy = bookedBy,
                confirmationNumber = confirmationNumber,
                nights = nights,
                orderId = orderId,
                paid = Paid,
                price = Price,
                status = status,
                orderColor = orderColor,
                staffComments = staffComments
            };
            if (apartmentNumber <= 0)
            {
                //This is a waiting apartment. Status must be either "waiting list" or "waiting deletion"
                if (status != OrderStatus.Waiting_list && status != OrderStatus.Waiting_deletion)
                {
                    //Change the status to be waiting deletion
                    theOrderData.status = OrderStatus.Waiting_deletion;
                    theOrderData.orderColor = Color.Gray;
                    theOrderData.price = "0";
                }
            }

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

                ApartmentPrice apartmentAndPrice = new ApartmentPrice()
                {
                    availability = Availability.OCCUPIED   //Set occupied as default value. If not replaced - will stay as occupied
                };
                if (theOrderData.status == OrderStatus.Waiting_list || theOrderData.status == OrderStatus.Waiting_deletion)
                {
                    //This is either a waiting list order or an order waiting deletion - Find the first "waiting apartment" that is free for these dates
                    //and assign the order to that fictitious apartment
                    //We sort the apartments in ascending order. That means that apartment -3 will be before -2, and before -1. This is done to be consistent 
                    //with what we do when displaying the real apartments and "waiting" apartments. (See s21dashBoardPreparation)
                    var waitingApartments = from anApartment in db.Apartments
                                            where anApartment.type == ApartmentIs.Waiting
                                            orderby anApartment.number
                                            select anApartment;
                    foreach (var anApartment in waitingApartments)
                    {
                        if (PublicController.calculateFreeDatesForApartment(anApartment, db, theBookingRequest, orderId) == true)
                        {
                            //The waiting apartment is free for these dates - assign that apartment to the order
                            apartmentAndPrice = new ApartmentPrice()
                            {
                                availability = Availability.AVAILABLE,
                                theApartment = anApartment,
                                nightCount = theOrderData.nights
                            };
                            theOrderData.apartmentNumber = anApartment.number;
                            break;
                        }
                    }
                    //At this point - if the loop was not "broken" - it means that no waiting apartment is free at these dates and the 
                    //apartmentAndPrice remains as "occupied"
                    //Anyway - change the default color - waiting list is orange and waiting deletion is gray
                    if (theOrderData.status == OrderStatus.Waiting_list)
                    {
                        theOrderData.orderColor = Color.Orange;
                    }
                    else if (theOrderData.status == OrderStatus.Waiting_deletion)
                    {
                        theOrderData.orderColor = Color.Gray;
                    }
                }
                else
                {
                    //This is a normal order - check apartment availability and price
                    apartmentAndPrice = PublicController.calculatePricePerStayForApartment(theAparatment, db, theBookingRequest, currentCurrency, orderId);
                }

                Country theCountry = db.Countries.SingleOrDefault(a => a.name == Country);
                if (apartmentAndPrice.availability == Availability.OCCUPIED)
                {
                    //the apartment is not available, although it seemed to be available. Perhaps it was taken in the last minutes
                    //If the order is a waiting order - no "waiting apartment" was free for these dastes.
                    theOrderData.setErrorMessageFor("comments", "These dates are not available for that apartment");
                    return View("s23addUpdateOrder");
                }
                else
                {
                    //Complete the booking
                    //At this point we do not care if this is a normal order or a waiting order. 
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
                    salaryCents = 0,
                    hotel_Id = 1
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
        /// The function adds an expense reported by the user in s21dashboard.
        /// </summary>
        /// <param name="expenseDate"></param>
        /// <param name="expenseType">either one of the existing expense types in expenseTypes table or a new expense type
        /// If new - that expense type will be added to the expenseTypes table</param>
        /// <param name="amount">expense amount (as integer)</param>
        /// <param name="currency">currency code</param>
        /// <param name="description"></param>
        /// <returns>the new expense amount for that date in UAH without currency symbol and cents</returns>
        [HttpPost]
        public string s29addExpense(string expenseDate, string expenseType, int amount, string currency, string description)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            //Check if that expense type exists
            var expenseTypeRec = db.ExpenseTypes.SingleOrDefault(rec => rec.nameKey == expenseType);
            if (expenseTypeRec == null)
            {
                //This expense type does not exist in the expenseTypes table - add it
                expenseTypeRec = new ExpenseType()
                {
                    nameKey = expenseType
                };
                db.ExpenseTypes.Add(expenseTypeRec);
            }
            //At this point the expense type exists in expenseTypeRec record
            Currency theCurrency = db.Currencies.Single(rec => rec.currencyCode == currency);   //We assume it must be in the DB
            DateTime theExpenseDate = DateTime.ParseExact(expenseDate, "dd/MM/yyyy", null);
            Expense theExpense = new Expense()
            {
                amount = amount * 100,   //The amount is kept in cents in the DB
                Currency = theCurrency,
                date = theExpenseDate,
                description = description,
                ExpenseType = expenseTypeRec
            };
            db.Expenses.Add(theExpense);
            db.SaveChanges();

            return calculateExpensesForDate(theExpenseDate);
        }

        /// <summary>
        /// Creates a list of expenses for the date selected by the user
        /// </summary>
        /// <param name="expenseDateSt">the date in </param>
        /// <returns></returns>
        [HttpGet]
        public PartialViewResult s30showExpensesForDate(string expenseDateSt)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            DateTime expenseDate = DateTime.ParseExact(expenseDateSt, "yyyy-MM-dd", null);
            var expenseList = from expense in db.Expenses
                              where expense.date == expenseDate
                              select expense;

            return PartialView("s30showExpenses", expenseList);
        }

        /// <summary>
        /// The method gets is called when the user presses an existing expense in the expense list. The method creates the 
        /// expense update form
        /// </summary>
        /// <param name="expenseId"></param>
        /// <returns>the form containing the expense fields: type, amount, description, and an update, delete and cancel buttons</returns>
        [HttpGet]
        public PartialViewResult s31updateExpense(int expenseId)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            Expense theExpense = db.Expenses.Single(rec => rec.Id == expenseId);
            TranslateBox tBox = this.setTbox("RU");
            ViewBag.tBox = tBox;
            ViewBag.expenseTypes = db.ExpenseTypes;
            return PartialView("s31updateExpense", theExpense);

        }
        /// <summary>
        /// gets the updated expense data and updates the DB
        /// </summary>
        /// <param name="expenseDate"></param>
        /// <param name="expenseType"></param>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="description"></param>
        /// <param name="expenseId">the id of the expense being updated</param>
        /// <returns>the new total expenses for that date in UAH, without cents and currency sign</returns>
        [HttpPost]
        public string s31updateExpense(string expenseDate, string expenseType, int amount, string currency, string description, int expenseId)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            //Check if that expense type exists
            var expenseTypeRec = db.ExpenseTypes.SingleOrDefault(rec => rec.nameKey == expenseType);
            if (expenseTypeRec == null)
            {
                //This expense type does not exist in the expenseTypes table - add it
                expenseTypeRec = new ExpenseType()
                {
                    nameKey = expenseType
                };
                db.ExpenseTypes.Add(expenseTypeRec);
            }
            //At this point the expense type exists in expenseTypeRec record
            Currency theCurrency = db.Currencies.Single(rec => rec.currencyCode == currency);   //We assume it must be in the DB
            DateTime theExpenseDate = DateTime.ParseExact(expenseDate, "dd/MM/yyyy", null);
            //Read the existing expense
            Expense theExpense = db.Expenses.Single(rec => rec.Id == expenseId);
            theExpense.amount = amount * 100;
            theExpense.Currency = theCurrency;
            theExpense.date = theExpenseDate;
            theExpense.description = description;
            theExpense.ExpenseType = expenseTypeRec;

            db.SaveChanges();

            return calculateExpensesForDate(theExpenseDate);
        }

        [HttpPost]
        public string s32deleteExpense(int expenseId)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();

            Expense theExpense = db.Expenses.Single(rec => rec.Id == expenseId);
            DateTime theExpenseDate = theExpense.date;
            db.Expenses.Remove(theExpense);
            db.SaveChanges();

            return calculateExpensesForDate(theExpenseDate);
        }

        /// <summary>
        /// The report shows 2 columns for each month: revenu for each apartment and occupancy percentage. Stargin at 
        /// 10/2019 until today
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult s33revenueReport(int? _fromMonth, int? _fromYear, int? _toMonth, int? _toYear)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            Employee theEmployee = (Employee)Session["loggedinUser"];

            if (theEmployee == null)
            {
                //No user is logged in - go to login screen
                return s10login();
            }
            
            int fromMonth = _fromMonth ?? 1;
            int fromYear = _fromYear?? FakeDateTime.DateNow.Year;
            int toMonth = _toMonth?? FakeDateTime.DateNow.Month;
            int toYear = _toYear ?? fromYear;
            DateTime fromDate = new DateTime(fromYear, fromMonth, 1);
            DateTime toDate = new DateTime(toYear, toMonth, 1);  //if toMonth = 12 and toYear = 2020, then
                                                                 //toYear will be 1/12/2020

            //fromDate is after toDate - reverse the 2 dates
            if (fromDate > toDate)
            {
                DateTime temp = fromDate;
                fromDate = toDate;
                toDate = temp;
            }

            //The key of the dictionary is the apartment number. The value is the revenue and occupancy 
            //statistics for each month in the year for that apartment
            //See: https://docs.google.com/spreadsheets/d/1-xa-eSMR8yv7E8f9GBTBTVFPfG-ko9Ask8akpfHB-4Q/edit#gid=0
            //for an example of the report
            //Note that for "toDate" we add 1 month - if "toDate" is 12/2020, then we need to read all revenue data 
            //until 31/12/2020. So the condition is date < toDate.AddMonths(1), which is 1/1/2021 00:00:00
            //We take only information about apartment with positive apartment numbers - apartments with 
            //negative numbers are "waiting list" apartments (not real)
            DateTime toDatePlus1 = toDate.AddMonths(1);
            var apartmentDays = from anApartmentDays in db.ApartmentDays
                                where anApartmentDays.status != ApartOccuStatus.Free 
                                && anApartmentDays.Apartment.number > 0
                                && anApartmentDays.date >= fromDate && anApartmentDays.date < toDatePlus1
                                select anApartmentDays;
            Dictionary2d<int, DateTime, RevenueAndOccupancy> revenueMatrix = new Dictionary2d<int, DateTime, RevenueAndOccupancy>();

            //Find all apartments which exist in the apartmentDays
            var apartmentList = (from anApartmentDays in apartmentDays
                                select anApartmentDays.Apartment.number).Distinct();
            
            //Populate all rows (apartments) and columns (months) of revenueMatrix
            foreach(int apartmentNumber in apartmentList)
            {
                for (DateTime aDate = fromDate; aDate <= toDate; aDate = aDate.AddMonths(1))
                {
                    int daysInMonth = DateTime.DaysInMonth(aDate.Year, aDate.Month);
                    revenueMatrix.Add(apartmentNumber, aDate, new RevenueAndOccupancy(daysInMonth));
                }
            }
            //At this point the revenue matrix contains empty RevenuyeAndOccupancy objects for all apartments and 
            //months.
            //Populate the revenueMatrix with real revenue data
            foreach (var anApartmentDays in apartmentDays)
            {
                int apartmentNumber = anApartmentDays.Apartment.number;
                //convert the date to be the first day of the relevant month
                DateTime yearMonth = new DateTime(anApartmentDays.date.Year, anApartmentDays.date.Month, 1);
               
                revenueMatrix[apartmentNumber, yearMonth].addDaysRevenue(anApartmentDays.revenueAsMoney());
            }

            //Calculate expenses
            var expenses = from anExpense in db.Expenses
                           where anExpense.date >= fromDate && anExpense.date < toDatePlus1
                           select anExpense;
            Dictionary<DateTime, Money> expensesPerMonth = new Dictionary<DateTime, Money>();
            //populate all months of expensesPerMonth with 0 values.
            for (DateTime aDate = fromDate; aDate <= toDate; aDate = aDate.AddMonths(1))
            {
                DateTime yearMonth = new DateTime(aDate.Year, aDate.Month, 1);  //such as 1/12/2020
                expensesPerMonth.Add(yearMonth, new Money(0m, "UAH"));
            }
            //At this point expensesPerMonth contain a Money record for each month in the report - with 0 value
            foreach (var anExpense in expenses)
            {
                DateTime yearMonth = new DateTime(anExpense.date.Year, anExpense.date.Month, 1);
                expensesPerMonth[yearMonth] += anExpense.expenseAsMoney();
            }
            ViewBag.revenueMatrix = revenueMatrix;
            ViewBag.expensesPerMonth = expensesPerMonth;
            ViewBag.apartmentList = revenueMatrix.getRowKeys();
            TranslateBox tBox = this.setTbox("RU");
            ViewBag.tBox = tBox;
            ViewBag.fromDate = fromDate;
            ViewBag.toDate = toDate;
            ViewBag.employee = theEmployee;
            return View("s33revenueReport");
        }

        /// <summary>
        /// Calculate the  amount of expenses for that date (as money string)
        /// </summary>
        /// <param name="theExpenseDate">the date for which we want to calcualte the expenses</param>
        /// <returns>total expenses as string (without currency and cents in UAH)</returns>
        private string calculateExpensesForDate(DateTime theExpenseDate)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            var expensesForDate = from expense in db.Expenses
                                  where expense.date == theExpenseDate
                                  select expense;
            Money totalExpensesForDate = new Money(0m, "UAH");
            foreach (var anExpense in expensesForDate)
            {
                totalExpensesForDate += anExpense.expenseAsMoney();
            }
            string totalExpensesSt = totalExpensesForDate.toMoneyString();
            if (totalExpensesSt == "0")
            {
                totalExpensesSt = "";   //show empty cell when 0
            }
            return totalExpensesSt;
        }


        /// <summary>
        /// This method is an auxiliary method to create the translation box and to insert it if needed to the Session variable
        /// It was copied from the public controller. A more elegant solution would be to create one method that will get the 
        /// controller as a parameter.
        /// </summary>
        /// <param name="language">language code, if set by the user</param>
        /// <returns>the translation box</returns>
        public TranslateBox setTbox(string language)
        {
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