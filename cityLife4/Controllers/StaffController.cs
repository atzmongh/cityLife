using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using cityLife;
using cityLife4;

namespace cityLife.Controllers
{
    public class LoginFormData
    {
        public FieldData userName = new FieldData();
        public FieldData password = new FieldData();
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
            LoginFormData theLoginFormData = new LoginFormData();
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            TranslateBox tBox = this.setTbox("RU");
            ViewBag.tBox = tBox;
            ViewBag.theLoginFormData = theLoginFormData;

            bool errorExists = false;
            if (userName == "")
            {
                theLoginFormData.userName.errorMessage = "Please enter your user name";
                errorExists = true;
            }
            if (password == "")
            {
                theLoginFormData.password.errorMessage = "Please enter the password";
                errorExists = true;
            }
            if (errorExists)
            {
                //return the same login screen with error messages
                return View("s10login");
            }
            theLoginFormData.userName.content = userName;

            //check if user name and password are valid
            Employee theEmployee = db.Employees.FirstOrDefault(anEmp => anEmp.userName == userName);
            if (theEmployee == null)
            {
                theLoginFormData.userName.errorMessage = "Username was not found";
                return View("s10login");
            }

            if (theEmployee.passwordHash != password)
            {
                theLoginFormData.password.errorMessage = "The password does not match our records";
                return View("s10login");
            }

            Session["loggedinUser"] = theEmployee;
            return View("s20dashboard");
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
        /// <param name="fromDateOrNull">Date entered in the date picker, or null</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult s21Dashboard(DateTime? fromDateOrNull)
        {
            FakeDateTime.SetFakeTime(new DateTime(2018, 09, 20));  //DEBUGGING!!!!!!
            DateTime fromDate = fromDateOrNull ?? FakeDateTime.Now;
            var apartmentDayBlocks = s21dashboardPreparation(fromDate);
            ViewBag.apartmentDayBlocks = apartmentDayBlocks;
            TranslateBox tBox = this.setTbox("RU");
            ViewBag.tBox = tBox;
            ViewBag.fromDate = fromDate;
            return View("s21horizontalDashboard");
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