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
using cityLife4;

//Look in http://www.binaryintellect.net/articles/6d19edd9-7582-4caf-b254-73deca44ecfb.aspx
//for how to create infinite scrolling using MVC and Jquery.
//look in the following link to see how to log errors:
//https://docs.microsoft.com/en-us/aspnet/web-forms/overview/older-versions-getting-started/deploying-web-site-projects/processing-unhandled-exceptions-cs
//https://stackify.com/aspnet-mvc-error-handling/


namespace cityLife.Controllers
{
    public class TranslationData
    {
        public int translationKeyId;
        public string filePath;
        public int lineNumber;
        public string translationKey;
        public string textOriginLanguage;
        public string textDestinationLanguage;
    }
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
            var connectionString = ConfigurationManager.ConnectionStrings["cityLifeDBContainer1"];
            ViewBag.connectionString = connectionString.ConnectionString;
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

            PopulateDB(db, csvFileStream);

          

            return View("uploadDB");
        }

        [HttpGet]
        public ActionResult unitTests(string theAction, bool? skipCorrectTests)
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
                string[] seriesAndNumber = checkBoxName.Split('-');  //create an array containing series name and number
                series = seriesAndNumber[0];
                number = int.Parse(seriesAndNumber[1]);
                isCorrect = (checkBoxValue == "correct");
            }
            catch (Exception innerException)
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
            //Read the languages available - remove the XX language if exists
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            var languages = (from aLanguage in db.Languages
                             where aLanguage.languageCode != "XX"
                             select aLanguage).ToList();

            ViewBag.languages = languages;

            return View("translations");
        }
        [HttpGet]
        public JsonResult getTranslations(string fromLanguage, string toLanguage, bool showOnlyMissing)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            List<TranslationData> translationDataList = new List<TranslationData>();


            //Reach each translation key and attach to it the translation in the origin language and destination language, if exist.
            foreach (var aTranslationKey in db.TranslationKeys)
            {
                var aTranslationFrom = db.Translations.FirstOrDefault(aRecord => aRecord.TranslationKey.id == aTranslationKey.id &&
                                                                             aRecord.Language.languageCode == fromLanguage);
                var aTranslationTo = db.Translations.FirstOrDefault(aRecord => aRecord.TranslationKey.id == aTranslationKey.id &&
                                                                             aRecord.Language.languageCode == toLanguage);
                string textDestinationLanguage = "";
                string textOriginLanguage = "";
                if (showOnlyMissing && aTranslationTo != null)
                {
                    //The destination translation exists, and user wants to see only missing translations - skip this translation
                    continue;
                }
                if (aTranslationTo != null)
                {
                    textDestinationLanguage = aTranslationTo.message;
                }
                if (aTranslationFrom == null)
                {
                    //The translation in the origin language does not exist - insert appropriate message
                    textOriginLanguage = "(missing)";
                }
                else
                {
                    textOriginLanguage = aTranslationFrom.message;
                }
                // Trimout the start path that always looks like: C:\software\cityLife\cityLife4
                string shortFilePath = aTranslationKey.filePath != null ? aTranslationKey.filePath.Substring(31) : "";
                translationDataList.Add(new TranslationData
                {
                    translationKeyId = aTranslationKey.id,
                    filePath = shortFilePath,  
                    lineNumber = aTranslationKey.lineNumber ?? 0,
                    translationKey = aTranslationKey.transKey,
                    textOriginLanguage = textOriginLanguage,
                    textDestinationLanguage = textDestinationLanguage
                });
            }

            JsonResult jResult = Json(translationDataList, JsonRequestBehavior.AllowGet);
            return jResult;
        }
        /// <summary>
        /// the method is called when the user presses the "save all" button in the page. The form is submitted along with a list of 
        /// all translations. Each translation contains: name of the form en-1 (language code, then the translation key id). The value is the 
        /// translated text for that transaltion key
        /// </summary>
        [HttpPost]
        public ActionResult saveTranslations()
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            foreach (var aKey in Request.Form.AllKeys)
            {
                if (Request[aKey] == "")
                {
                    //No translation has been given - no need to do anything
                    continue;
                }
                try
                {
                    var langCodeAndId = aKey.Split('-');
                    string languageCode = langCodeAndId[0];
                    int transKeyId = int.Parse(langCodeAndId[1]);

                    //Look if such translation already exists
                    var theTranslation = db.Translations.FirstOrDefault(aRecord => aRecord.TranslationKey.id == transKeyId && aRecord.Language.languageCode == languageCode);
                    var theTranslationKey = db.TranslationKeys.Single(aRecord => aRecord.id == transKeyId);
                    var theLanguage = db.Languages.Single(aRecord => aRecord.languageCode == languageCode);
                    if (theTranslation == null)
                    {
                        //Such translation does not exist - add it
                        theTranslation = new Translation() { Language = theLanguage, TranslationKey = theTranslationKey, message = Request[aKey] };
                        db.Translations.Add(theTranslation);
                    }
                    else
                    {
                        //The translation exists - update the translated text
                        theTranslation.message = Request[aKey];
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    throw new AppException(111, null, aKey);
                }

            }
            return this.translations();
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

                string tableName = "undefined";
                List<string> columnNames = new List<string>();
                List<string> columnValues = new List<string>();

                string[] lineFields = parser.ReadFields();
                int columnCount = 0;
                string setIdentityOn = "";
                while (lineFields != null)
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
                        insertCommand.Append("N'" + columnValues[0] + "'");     //INSERT INTO apartment(id,name,description) VALUES (N'123'
                        for (int i = 1; i < columnValues.Count(); i++)
                        {
                            insertCommand.Append(",");                     //INSERT INTO apartment(id,name,description) VALUES (N'123',
                            insertCommand.Append("N'" + columnValues[i] + "'"); //INSERT INTO apartment(id,name,description) VALUES (N'123',N'nice'
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
            TranslateBox tBox = new TranslateBox("EN", "RU", "showAsterisks");
            Test.startTestSeries("TranslationBox");
            Test.check(1, tBox.translate("sunday"));
            Test.check(2, tBox.translate("Monday"));
            Test.check(3, tBox.translate("Tuesday"));
            Test.check(4, tBox.translate("Wednesday"));
            Test.check(5, tBox.translate("Thursday"));


            tBox = new TranslateBox("RU", "EN", "showAsterisks");
            Test.check(6, tBox.translate("Thursday"));

            //Tests with "dont show" mode
            tBox = new TranslateBox("EN", "RU", "dontShow");
            Test.check(7, tBox.translate("sunday"));//translation key does not exist
            Test.check(8, tBox.translate("Monday"));//translation does not exist 
            Test.check(9, tBox.translate("Tuesday"));//translation exists in target language 
            Test.check(10, tBox.translate("Wednesday"));//translation exists only in default language
            Test.check(11, tBox.translate("Thursday"));//translation exists in both languages
        }
    }
}