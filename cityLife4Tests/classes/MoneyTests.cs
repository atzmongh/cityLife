using Microsoft.VisualStudio.TestTools.UnitTesting;
using cityLife;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cityLife.Tests
{
    [TestClass()]
    public class MoneyTests
    {
       

        [TestMethod()]
        public void ToStringTest()
        {
            Money m1 = new Money(1234.56M, "USD");
            Assert.AreEqual("USD 1234.56", m1.ToString());
            Assert.AreEqual("$1,234.56", m1.toMoneyString());

            Money m2 = new Money(m1);
            Assert.AreEqual("$1,234.56", m2.toMoneyString());
            Assert.AreEqual("$1,235", m2.toMoneyString(showCents: false));

            Money m3 = m1.converTo("EUR", new DateTime(2018, 10, 25),rounded:false);
            Assert.AreEqual("EUR 1073.53", m3.ToString());
            Money m9 = m1.converTo("EUR", new DateTime(2018, 10, 25), rounded: true);
            Assert.AreEqual("EUR 1074.00", m9.ToString());

            Money m6 = m1.converTo("EUR", new DateTime(2018, 10, 20),rounded:false);
            Assert.AreEqual("EUR 1028.80", m6.ToString());

            Money m4 = m3.converTo("USD", new DateTime(2018, 10, 25));
            Assert.AreEqual("$1,234.56", m4.toMoneyString());
            string anException = "exception was not raised";
            try
            {
                Money m5 = m1.converTo("EUR", new DateTime(2018, 10, 17));
            }
            catch
            {
                anException = "exception was raised";
            }
            Assert.AreEqual("exception was raised", anException);
        }

       

       
    }
}