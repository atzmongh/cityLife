using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace cityLife4
{
    public static class Test
    {
        private static string seriesName = "undefined";
        private static DateTime? testStartTime=null;


        /// <summary>
        /// Each time we run a test cycle we wish to set its start time. This is done in order to make sure that all tests actually were 
        /// conducted during the test run. Because if we see that a certain test was conducted prior to the start of the test cycle - it means
        /// that this test was not run during the cycle.
        /// Before starting a new test run we need to call this method.
        /// The unitTest table contains a special record that holds the datetime of the last test run. (series = "testStart"
        /// </summary>
        public static DateTime startTestCycle()
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            unitTest unitTestStart = db.unitTests.SingleOrDefault(aRecord => aRecord.series == "testStart");
            if (unitTestStart == null)
            {
                //This is the first time we run a unit test at all - create the special record called "testStart".
                //It will contain the date and time of the beginning of the test run. All other tests must have
                //date and time after this datetime
                Test.testStartTime = DateTime.Now;
                unitTestStart = new unitTest() { series = "testStart", number = 1, dateLastRun = (DateTime)Test.testStartTime };
                db.unitTests.Add(unitTestStart);
                db.SaveChanges();
            }
            else
            {
                //Update the date time for the current test run
                Test.testStartTime = DateTime.Now;
                unitTestStart.dateLastRun = (DateTime)Test.testStartTime;
                db.SaveChanges();
            }
            return unitTestStart.dateLastRun;
        }

        /// <summary>
        /// The tests are divided into "series" of tests. Each series has a name. Each test has a number which should be unique within 
        /// the series. When starting a new series we need to call this method
        /// </summary>
        /// <param name="seriesName"></param>
        public static void startTestSeries(string seriesName)
        {
            if (Test.testStartTime == null)
            {
                //We did not call the unit test start before calling this method. throw an exception
                throw new AppException(107, null);
            }
            Test.seriesName = seriesName;
        }
        /// <summary>
        /// This method performs the unit test itself. The method compares the actual result with the expected result
        /// as kept in the "unitTests" table in the DB.
        /// </summary>
        /// <param name="number">test number - unique number within the test series</param>
        /// <param name="actualResult">the actual result of the test</param>
        /// <returns>
        /// true if the actual result matches the expected result, as kept in the DB
        /// false if the actual result does not match the expected result in the DB
        /// null if no expected result is found in the DB
        /// </returns>
        public static bool? check(int number, string actualResult)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            string series = Test.seriesName;
            unitTest theUnitTest = db.unitTests.SingleOrDefault(aTest => aTest.series == series && aTest.number == number);

            if (theUnitTest == null)
            {
                //The unit test for this test does not exist yet - create it. (leave the expected result empty)
                theUnitTest = new unitTest() { series = series, number = number, actualResult = actualResult, dateLastRun = DateTime.Now };
                db.unitTests.Add(theUnitTest);
                db.SaveChanges();
                return null;
            }
            else
            {
                //A unit test record exists - check if it is correct
                theUnitTest.actualResult = actualResult;
                theUnitTest.dateLastRun = DateTime.Now;
                if (theUnitTest.expectedResult != null && theUnitTest.expectedResult == actualResult)
                {
                    //both results are equal
                    theUnitTest.correctFlag = true;
                }
                else if (theUnitTest.expectedResult != null)
                {
                    //the results are not the equal
                    theUnitTest.correctFlag = false;
                }
                else
                {
                    //there are no expected results
                    theUnitTest.correctFlag = null;
                }
                db.SaveChanges();
                return theUnitTest.correctFlag;  //either true, false or null
            }
        }

        public static bool? check(int number, object actualResult)
        {
            return check(number, actualResult.ToString());
        }
        public static bool? checkJson(int number, object actualResult)
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            string jsonResult = jsonSerializer.Serialize(actualResult);
            return check(number, jsonResult);

        }

        /// <summary>
        /// the method updates the test results based on user's input. 
        /// </summary>
        /// <param name="series">test series</param>
        /// <param name="number">test number (series and number uniquely identify a specific test)</param>
        /// <param name="isCorrect">specifies if the test result is correct</param>
        public static void updateTestResult(string series, int number, bool isCorrect)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            unitTest theUnitTest = db.unitTests.SingleOrDefault(aRecord => aRecord.series == series && aRecord.number == number);
            if (theUnitTest == null)
            {
                throw new AppException(109, null, series, number);
            }

            if (isCorrect)
            {
                //The result is correct - update correct flag and expected result
                theUnitTest.correctFlag = true;
                theUnitTest.expectedResult = theUnitTest.actualResult;
            }
            else
            {
                //The result is incorrect
                theUnitTest.correctFlag = false;
                if (theUnitTest.expectedResult == theUnitTest.actualResult)
                {
                    //Currently the expected is equal to the actual. However, we know that the result is incorrect. Erase the
                    //expected result
                    theUnitTest.expectedResult = null;
                }
            }
            db.SaveChanges();
        }

    }
}