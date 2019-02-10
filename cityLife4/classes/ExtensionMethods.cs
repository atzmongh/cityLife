using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using cityLife.Controllers;

namespace cityLife4
{
    public static class ExtensionMethods
    {
        public static bool IsWeekend(this DateTime aDate)
        {
            return (aDate.DayOfWeek.Equals(DayOfWeek.Saturday) || aDate.DayOfWeek.Equals(DayOfWeek.Sunday));

        }
        /// <summary>
        /// The method outputs the HTML for a standard text input field. 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="theFieldData">contains: field name, value, error, and name without spaces that can be used for
        /// element ID, for example</param>
        /// <param name="tBox"></param>
        /// <param name="fieldType"></param>
        /// <returns> an HTML item of the form: If the item has no errors:
        /// <div class='inputBox'>
        /// <input id='email' name='email' type='email' value='moshe@gmail.com' class='inputElement' />
        /// <label for='email' class='label'>Email</label>
        /// <span class='error-text'></span>
        /// </div>
        /// 
        /// If the item has errors:
        /// <div class='inputBox error'>
        /// <input id='email' name='email' type='email' value='moshe@gmail.com' class='inputElement' />
        /// <label for='email' class='label'>Email</label>
        /// <span class='error-text'>Email is invalid</span>
        /// </div>
        /// 
        /// </returns>
        public static MvcHtmlString inputField(this HtmlHelper helper, string fieldName, string fieldContent, string fieldErrorMessage, TranslateBox tBox, string fieldType = "text", string style="")
        {
            string error = fieldErrorMessage != "" ? "error" : "";
            string fieldCamelName = fieldName.Replace(" ", "");
            string theField = string.Format(@"
<div class='inputBox {0}' style='{6}'>
  <input id = '{1}' name = '{1}' type = '{2}' value = '{3}' class='inputElement' />
  <label for='{1}' class='label'>{4}</label>
  <span class='error-text'>{5}</span>
</div>",
                                /*0*/error,   //returns either empty string or "error" (the error CSS class)
                                /*1*/fieldCamelName,//returns the field name without spaces, such as "SpecialRequest"
                                /*2*/fieldType,                  //normally either text or email
                                /*3*/fieldContent,        //the value of the field
                                /*4*/tBox.translate(fieldName),   //returns the translation of the field name
                                /*5*/tBox.translate(fieldErrorMessage),    //returns the error message, if any (translated). Or empty string
                                /*6*/style   ///any additional style, such as width:...
);
            MvcHtmlString mvcString = new MvcHtmlString(theField);
            return mvcString;
        }

    }
}


