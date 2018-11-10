using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public static class ErrorCodeCollection
    {
        private static Dictionary<int, string> errorCodesDict = new Dictionary<int, string>()
        {
            {101,"no suitable exchange rate was found for currencies: {0} and {1} for date {2} (or later). Sum was {3}"},
            {102,"such currency does not exist in DB:{0}"},
            {103,"Addition of money objects - the 2 objects have different currency, which is not supported. {0} and {1}"},
            {104,"Multiplication of money objects - the 2 objects have different currency, which is not supported. {0} and {1}"},
            {105,"Currency not found in DB:{0}"},
            {106,"an unsupported language requested:{0}"},
            {107,"Before running setStartSeries you must run startTestCycle" },
            {108,"checkbox name of unit test report has an unexpected format. Actual content:{0}. Expected format:<series>-<number>" },
            {109,"unit test series {0} number {1} was not found in DB" }

        };
        public static bool exists(int code)
        {
            return errorCodesDict.ContainsKey(code);
        }
        public static string message(int code)
        {
            if (exists(code))
            {
                return errorCodesDict[code];
            }
            else
            {
                return "undefined:" + code;
            }
        }
           
    }
    public class AppException : Exception
    {
        public AppException(int code, Exception innerException, params object[] parameters): base(AppException.formatMessage(code,parameters), 
                                                                                                  innerException)
        {
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            ErrorCode theErrorCode = db.ErrorCodes.SingleOrDefault(aRecord => aRecord.code == code);
            if (theErrorCode != null)
            {
                //This error code already exists in the DB. 
                theErrorCode.lastOccurenceDate = DateTime.Now;
                theErrorCode.occurenceCount++;
            }
            else
            {
                //The error code does not exist - create it
                theErrorCode = new ErrorCode() { code = code, message = ErrorCodeCollection.message(code),
                                                 lastOccurenceDate = DateTime.Now, occurenceCount = 1 };
                db.ErrorCodes.Add(theErrorCode);
            }
            db.SaveChanges();
            //check if the same error message exists in the error message table
            string formattedMessage = AppException.formatMessage(code, parameters);
            ErrorMessage theErrorMessage = (from anErrorMessage in theErrorCode.ErrorMessages
                                            where anErrorMessage.formattedMessage == formattedMessage &&
                                                  anErrorMessage.stackTrace == base.StackTrace
                                            select anErrorMessage).FirstOrDefault();  //actually we expect only a single record to be found
            if (theErrorMessage != null)
            {
                //Such an exact error message already exists - increment the counter
                theErrorMessage.lastOccurenceDate = DateTime.Now;
                theErrorMessage.occurenceCount++;
            }
            else
            {
                //such exact error message does not exist - add it
                theErrorMessage = new ErrorMessage()
                {
                   ErrorCode = theErrorCode,
                    formattedMessage = formattedMessage,
                    lastOccurenceDate = DateTime.Now,
                    occurenceCount = 1,
                    stackTrace = StackTrace,
                };
                db.ErrorMessages.Add(theErrorMessage);
            }
            db.SaveChanges();

        }
        private static string formatMessage(int code, params object[] parameters)
        {
            if (ErrorCodeCollection.exists(code))
            {
                string message = ErrorCodeCollection.message(code);
                string formattedMessage = string.Format(message, parameters);
                return formattedMessage;
            }
            else
            {
                return ErrorCodeCollection.message(code) + " " + parameters;
            }
        }
    }
}