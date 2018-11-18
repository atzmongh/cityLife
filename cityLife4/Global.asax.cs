using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using cityLife4;

namespace cityLife
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            int a = 1;
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            // Get the error details
            Exception theException = Server.GetLastError();
            if (theException.InnerException != null)
            {
                theException = theException.InnerException;
            }

            int errorCode = 0;
            if (theException is HttpException)
            {
                errorCode = (theException as HttpException).ErrorCode;
            }
            string lastErrorTypeName = theException.GetType().ToString();
            string lastErrorMessage = theException.Message;
            string lastErrorStackTrace = theException.StackTrace;
            //throw new AppException(110, null, errorCode, lastErrorTypeName, lastErrorMessage);
            AppException.writeException(110, null, lastErrorStackTrace, errorCode, lastErrorTypeName, lastErrorMessage);
        }
    }
}
