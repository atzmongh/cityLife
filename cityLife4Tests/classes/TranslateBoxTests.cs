using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cityLife
{
    [TestClass()]
    public class TranslateBoxTests
    {
        [TestMethod()]
        public void TranslateBoxTest()
        {
            //Tests with "showAsterisks" mode
            TranslateBox tBox = new TranslateBox("en", "ru", "showAsterisks");

            Assert.AreEqual("*sunday*",    tBox.translate("sunday"));  //translation key does not exist
            Assert.AreEqual("*Monday*",    tBox.translate("Monday"));  //translation does not exist 
            Assert.AreEqual("tue",         tBox.translate("Tuesday")); //translation exists in target language 
            Assert.AreEqual("*Wednesday*", tBox.translate("Wednesday"));  //translation exists only in default language
            Assert.AreEqual("thu",         tBox.translate("Thursday"));   //translation exists in both languages

            tBox = new TranslateBox("ru", "en", "showAsterisks");

            Assert.AreEqual("chetverg",    tBox.translate("Thursday"));  //translation exists in both languages

            //Tests with "dont show" mode
            tBox = new TranslateBox("en", "ru", "dontShow");
            Assert.AreEqual("sunday", tBox.translate("sunday"));  //translation key does not exist
            Assert.AreEqual("Monday", tBox.translate("Monday"));  //translation does not exist 
            Assert.AreEqual("tue", tBox.translate("Tuesday")); //translation exists in target language 
            Assert.AreEqual("creda", tBox.translate("Wednesday"));  //translation exists only in default language
            Assert.AreEqual("thu", tBox.translate("Thursday"));   //translation exists in both languages

        }

       
    }
}