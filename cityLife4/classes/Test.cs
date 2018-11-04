using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public static class Test
    {
        private static cityLifeDBContainer1 db = new cityLifeDBContainer1();
        private static string seriesName = "undefined";
        private static DateTime? testStartTime=null;


        /// <summary>
        /// Each time we run a test cycle we wish to give it a consecutive number. This is done in order to make sure that all tests actually were 
        /// conducted during the test run. Before starting a new test run we need to call this method. 
        /// </summary>
        public static DateTime startTestCycle()
        {
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
                unitTestStart.dateLastRun = DateTime.Now;
                db.SaveChanges();
            }
            return unitTestStart.dateLastRun;
        }

        public static void startTestSeries(string seriesName)
        {
            if (Test.testStartTime == null)
            {
               // throw new AppException(10)
            }
            Test.seriesName = seriesName;
        }
        public static bool? check(int number, string actualResult)
        {
            string series = Test.seriesName;
            unitTest theUnitTest = db.unitTests.SingleOrDefault(aTest => aTest.series == series && aTest.number == number);

            if (theUnitTest == null)
            {
                //The unit test for this test does not exist yet - create it.
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
                    //both results are equal - do nothing
                    return true;
                }
                else if (theUnitTest.expectedResult != null)
                {
                    //the results are not the equal
                    theUnitTest.correctFlag = false;
                    theUnitTest.actualResult = actualResult;
                    db.SaveChanges();
                    return false;
                }
                else
                {
                    //there are not expected results
                    theUnitTest.correctFlag = null;
                    theUnitTest.actualResult = actualResult;
                    db.SaveChanges();
                    return null;
                }
            }
        }

    }
}