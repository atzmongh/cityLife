using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public class AppException:Exception
    {
        public AppException(int code, string message):base(message)
        {
            //TBD - add logging of exceptions
        }
    }
}