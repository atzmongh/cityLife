using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public static class Test
    {
        private static cityLifeDBContainer1 db = new cityLifeDBContainer1();

        public static bool? check(string series, int number, string actualResult)
        {
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