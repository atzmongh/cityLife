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

        public ActionResult s20dashboard()
        {
            //return View("s21horizontalDashboard");
            return View("s20dashboard");
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