using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

//Look in http://www.binaryintellect.net/articles/6d19edd9-7582-4caf-b254-73deca44ecfb.aspx
//for how to create infinite scrolling using MVC and Jquery.

namespace cityLife.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult uploadDB()
        {
            return View();
        }
        [HttpPost]
        public ActionResult uploadDB(HttpPostedFileBase dbCSV)
        {
            //drop all DB tables and create a new DB schema with empty DB
            StreamReader sqlReader = new StreamReader(Server.MapPath("/cityLifeDB.edmx.sql"));
            string createDBsql = sqlReader.ReadToEnd();
            
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
           
            db.Database.ExecuteSqlCommand(createDBsql);
            Stream csvFileStream = dbCSV.InputStream;
            
            PopulateDB(db,csvFileStream);
           

            return View("uploadDB");
        }

        [HttpGet]
        public ActionResult unitTests(string theAction,  bool? skipCorrectTests)
        {
            if (theAction == null)
            {
                //initial call to unit tests
                ViewBag.testTime = null;
                ViewBag.unitTests = null;
            }
            else if (theAction == "run Tests")
            {
                DateTime testTime = this.runTests();
                ViewBag.testTime = testTime;
            }
            else
            {
                //The action is "show test"
                cityLifeDBContainer1 db = new cityLifeDBContainer1();
                var unitTestsBySeries = from aUnitTest in db.unitTests
                                    orderby aUnitTest.series, aUnitTest.number
                                    group aUnitTest by aUnitTest.series;
                ViewBag.unitTestsBySeries = unitTestsBySeries;
                ViewBag.skipCorrectTests = skipCorrectTests ?? false;
                                    
               
                        
            }
            return View("unitTests");
        }

        /// <summary>
        /// The method is called by ajax post request from the unit test view after the user pressed a "correct" or "incorrect" 
        /// checkbox button in the unit test table. The method updates the unit test record accordingly
        /// </summary>
        /// <param name="checkBoxName">the name contains testSeries-number (for example: money-23  </param>
        /// <param name="checkBoxValue"> either "correct" or "incorrect"</param>
        [HttpPost]
        public void unitTestsResult(string checkBoxName, string checkBoxValue)
        {
            string series;
            int number;
            bool isCorrect;
            try
            {
                //split the "series" and the "nunber" from the name component
                string [] seriesAndNumber = checkBoxName.Split('-');  //create an array containing series name and number
                series = seriesAndNumber[0];
                number = int.Parse(seriesAndNumber[1]);
                isCorrect = (checkBoxValue == "correct");
            }
            catch(Exception innerException)
            {
                throw new AppException(108, innerException, checkBoxName);
            }

            Test.updateTestResult(series, number, isCorrect);
        }

        /// <summary>
        /// The method displays the translation screen which lets the user see which translations exist and add/edit translations in various languages
        /// </summary>
        /// <returns>A starting screen - without the actual translations, as they will be added by an ajax call</returns>
        [HttpGet]
        public ActionResult translations()
        {
            //Read the languages available - remove the XX language if exists, and add the translation key to the "from" languages
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            //The "toLanguages" is the list of target languages
            var toLanguages = (from aLanguage in db.Languages
                             where aLanguage.languageCode != "XX"
                             select aLanguage).ToList();
            
            //"fromLanguages" is the list of origin languages. The "translation key" is also a "language" for the origin language list
            List<Language> fromLanguages = new List<Language>();
            var translationKey = new Language() { languageCode = "TK", name = "translation key", isDefault = false };
            fromLanguages.Add(translationKey);
            fromLanguages.AddRange(toLanguages);

            ViewBag.fromLanguages = fromLanguages;
            ViewBag.toLanguages = toLanguages;

            return View("translations");
        }
        [HttpGet]
        public ActionResult getTranslations(string fromLanguage, string toLanguage, bool showOnlyMissing)
        {
            int a = 1;
            return View();
        }
        /// <summary>
        /// The function reads the CSV file that contains the DB content and creates SQL statements to populate it
        /// Then it executes this SQL and populates the DB
        /// </summary>
        private void PopulateDB(cityLifeDBContainer1 db, Stream csvFileStream)
        {
            //string CSVFilePath = Server.MapPath("/cityLifeDB.csv");
            using (TextFieldParser parser = new TextFieldParser(csvFileStream))
            {
                parser.Delimiters = new string[] { "," };

                string tableName="undefined";
                List<string> columnNames = new List<string>();
                List<string> columnValues =  new List<string>();

                string[] lineFields = parser.ReadFields();
                int columnCount=0;
                string setIdentityOn = "";
                while (lineFields !=null)
                {
                    if (lineFields[0] != "")
                    {
                        //This is a new table - the line contains the table name and then the field names
                        tableName = lineFields[0];
                        columnNames.Clear();
                        columnNames = new List<string>();

                        //copy all column names to the columnNames list - if the first column is "id" - generate a "set identity_insert on command"
                        //so that we can insert ids explicitly
                        if (lineFields[1] == "id")
                        {
                            setIdentityOn = "SET IDENTITY_INSERT " + tableName + " ON;";
                        }
                        else
                        {
                            setIdentityOn = "";
                        }

                        int i;
                        for (i = 1; i < lineFields.Count(); i++)
                        {
                            if (lineFields[i] == "")
                            {
                                //We reached an empty field in the column names. This means that we reahced the end of the table columns.
                                break;
                            }
                            columnNames.Add(lineFields[i]);
                        }
                        //At this point i contains the number of fields in the table
                        columnCount = i;
                    }
                    else if (lineFields[0].Contains("//"))
                    {
                        //This is a comment line - ignore it
                    }
                    else
                    {
                        //this is a data line - create an insert command to insert it into the DB
                        //Copy all value names into the valueName list
                        columnValues.Clear();

                        bool valuesExist = false;
                        for (int i = 1; i < columnCount; i++)
                        {
                            if (lineFields[i] != "")
                            {
                                valuesExist = true;
                            }
                            columnValues.Add(lineFields[i]);
                        }
                        //At this point we have all column names in columnName list and valus in columnValues list
                        if (valuesExist == false)
                        {
                            //This is an empty line - skip it
                            lineFields = parser.ReadFields();
                            continue;
                        }
                        StringBuilder insertCommand = new StringBuilder(setIdentityOn + "  INSERT INTO ");
                        insertCommand.Append(tableName);                   //INSERT INTO apartment
                        insertCommand.Append("(");                         //INSERT INTO apartment(
                        insertCommand.Append(columnNames[0]);              //INSERT INTO apartment(id
                        for (int i = 1; i < columnNames.Count(); i++)
                        {
                            insertCommand.Append(",");                     //INSERT INTO apartment(id,
                            insertCommand.Append(columnNames[i]);          //INSERT INTO apartment(id,name
                        }
                        insertCommand.Append(") VALUES (");                //INSERT INTO apartment(id,name,description) VALUES (
                        insertCommand.Append("N'"+columnValues[0]+"'");     //INSERT INTO apartment(id,name,description) VALUES (N'123'
                        for (int i = 1; i < columnValues.Count(); i++)
                        {
                            insertCommand.Append(",");                     //INSERT INTO apartment(id,name,description) VALUES (N'123',
                            insertCommand.Append("N'"+columnValues[i]+"'"); //INSERT INTO apartment(id,name,description) VALUES (N'123',N'nice'
                        }
                        insertCommand.Append(")");
                        string insertCommandSt = insertCommand.ToString();
                        db.Database.ExecuteSqlCommand(insertCommandSt);
                    }
                    lineFields = parser.ReadFields();
                }
                return;
            }
        }

        private DateTime runTests()
        {
            DateTime testTime = Test.startTestCycle();
            Test.startTestSeries("stam");
            Test.check(1, "kuku the great");
            Test.check(2, "muku the great");
            moneyTests();
            TranslateBoxTest();
            return testTime;
        }
        private void moneyTests()
        {
            Test.startTestSeries("money");
            Money m1 = new Money(1234.56M, "USD");
            Test.check(1, m1);
            Test.check(2, m1.toMoneyString());

            Money m2 = new Money(m1);
            Test.check(3, m2.toMoneyString());
            Test.check(4, m2.toMoneyString(showCents: false));

            Money m3 = m1.converTo("EUR", new DateTime(2018, 10, 25), rounded: false);
            Test.check(5, m3);

            Money m9 = m1.converTo("EUR", new DateTime(2018, 10, 25), rounded: true);
            Test.check(6, m9);
           // Assert.AreEqual("EUR 1074.00", m9.ToString());

            Money m6 = m1.converTo("EUR", new DateTime(2018, 10, 20), rounded: false);
            Test.check(7, m6);
          // Assert.AreEqual("EUR 1028.80", m6.ToString());

            Money m4 = m3.converTo("USD", new DateTime(2018, 10, 25));
            Test.check(8, m4.toMoneyString());
           // Assert.AreEqual("$1,234.56", m4.toMoneyString());
            string anException = "exception was not raised";
            try
            {
                Money m5 = m1.converTo("EUR", new DateTime(2018, 10, 17));
            }
            catch
            {
                anException = "exception was raised";
            }
            Test.check(9, anException);
            //Assert.AreEqual("exception was raised", anException);
        }

        public void TranslateBoxTest()
        {
            //Tests with "showAsterisks" mode
            TranslateBox tBox = new TranslateBox("en", "ru", "showAsterisks");
            Test.startTestSeries("TranslationBox");
            Test.check(1, tBox.translate("sunday"));
            Test.check(2, tBox.translate("Monday"));
            Test.check(3, tBox.translate("Tuesday"));
            Test.check(4, tBox.translate("Wednesday"));
            Test.check(5, tBox.translate("Thursday"));


            tBox = new TranslateBox("ru", "en", "showAsterisks");
            Test.check(6, tBox.translate("Thursday"));

            //Tests with "dont show" mode
            tBox = new TranslateBox("en", "ru", "dontShow");
            Test.check(7, tBox.translate("sunday"));//translation key does not exist
            Test.check(8, tBox.translate("Monday"));//translation does not exist 
            Test.check(9, tBox.translate("Tuesday"));//translation exists in target language 
            Test.check(10, tBox.translate("Wednesday"));//translation exists only in default language
            Test.check(11, tBox.translate("Thursday"));//translation exists in both languages
        }
    }
}