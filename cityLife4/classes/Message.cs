using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace cityLife4
{
    /// <summary>
    /// The class is used to manage email and SMS messages. All messages are files located in
    /// the messages folder. They contains the message text (which can be HTML) and which may contain
    /// variables in the form [table.field]. the class can replace the variables with actual values 
    /// and return the message text.
    /// </summary>
    public class Message
    {
        private StringBuilder messageText;
        private string messageFilePath;

        public Message(string messageFilePath, Dictionary<string, string> variableDict)
        {
            this.messageFilePath = messageFilePath;

            StreamReader reader = new StreamReader(messageFilePath);
            string message = reader.ReadToEnd();
            this.messageText = new StringBuilder();

            //substitute the variables with their values. Use the following FSA:
            //state    [    any     ]
            //0-S      A     S      S
            //1-A      S     B      E
            //2-B      E     B      C
            //3-C      A     S      S
            //4-E

            int[,] stateMat =
                {
                {1,0,0 },
                {0,2,4 },
                {4,2,3 },
                {1,0,0 }
                };

            int charI;
            int state = 0;
            string variable = "";
            foreach (char aChar in message)
            {
                //Read each character from the message and analyze it
                switch (aChar)
                {
                    case '[':
                        charI = 0;
                        break;
                    case ']':
                        charI = 2;
                        break;
                    default:
                        charI = 1;
                        break;
                }
                state = stateMat[state, charI];
                switch (state)
                {
                    case 0:
                        //state S - copy the letter to the output message
                        messageText.Append(aChar);
                        break;
                    case 1:
                        //state A - do nothing
                        break;
                    case 2:
                        //state B - accumulate the variable name
                        variable += aChar;
                        break;
                    case 3:
                        //state C - replace the variable with the value
                        if (!variableDict.ContainsKey(variable))
                        {
                            throw new AppException(115, null, variable);
                        }
                        string value = variableDict[variable];
                        messageText.Append(value);
                        variable = "";
                        break;
                    case 4:
                        //state E - either we encountered [] (with empty variable name) or [sds[ - open bracket inside open bracket (but 
                        //not 2 in a row, which is legal and denotes the open bracket character)
                        throw new AppException(116, null, message);
                }

            }
        }
        public override string ToString()
        {
            return messageText.ToString();
        }

    }

    public class EmailMessage 
    {
        private string to;
        private string subject;
        private string mailName;
        private string body;
        public EmailMessage(string to, string subject, string mailName, Controller theController) 
        {
            this.to = to;
            this.subject = subject;
            this.mailName = mailName;
            var context = theController.ControllerContext;
            context.Controller.ViewData.Model = null;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindView(context, mailName, null);
                var viewContext = new ViewContext(context, viewResult.View, context.Controller.ViewData, context.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(context, viewResult.View);
                this.body = sw.GetStringBuilder().ToString();
            }
        }
        public void send()
        {
            var message = new MailMessage();
            message.To.Add(new MailAddress(this.to));
            message.From = new MailAddress("apart.citylife@gmail.com");
            message.Subject = this.subject;
            message.Body = this.body;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "apart.citylife@gmail.com",
                    Password = "456ertksenia"
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                try
                {
                    smtp.Send(message);
                }
                catch(Exception e)
                {
                    //Send mail failed - continue working, but write error message to the log
                    AppException.writeException(122, e, null, this.to, this.mailName);
                }
                
            }
        }
    }
}