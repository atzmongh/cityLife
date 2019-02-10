using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public partial class Order
    {
        public bool isFirstDay(DateTime aDate)
        {
            return this.checkinDate == aDate;
        }
        /// <summary>
        /// Last day is the day before the checkout date. For example: if the guest checked in on 1/12/2019 and checkout on 4/12/2019
        /// then: 
        ///             isFirstDay  isLastDate
        /// 1/12/2019 - true        false
        /// 2/12/2019   false       false
        /// 3/12/2019   false       true
        /// 4/12/2019   fakse       false
        /// 
        /// If the guest checked in on 1/12/2019 and checkout on 2/12/2019, then:
        /// 1/12/2019 - true        true
        /// 2/12/2019   false       false
        /// </summary>
        /// <param name="aDate"></param>
        /// <returns></returns>
        public bool isLastDay(DateTime aDate)
        {
            return this.checkoutDate == aDate.AddDays(1);
        }

        public Money priceAsMoney()
        {
            decimal priceDecimal = ((decimal)this.price) / 100;     //The price is kept in cents in the DB.
            Money priceMoney = new Money(priceDecimal, this.Currency.currencyCode);
            return priceMoney;
        }

        public Money amountPaidAsMoney()
        {
            decimal amountPaidDecimal = ((decimal)this.amountPaid) / 100;     //The price is kept in cents in the DB.
            Money amountPaidMoney = new Money(amountPaidDecimal, this.Currency.currencyCode);
            return amountPaidMoney;
        }

        public int nights
        {
            get
            {
                return Order.dateDifference(this.checkoutDate, this.checkinDate);
            }
            
        }
        public static int dateDifference(DateTime checkoutDate, DateTime checkinDate)
        {
            var nightsSpan = (checkoutDate - checkinDate);
            var nightCount = (int)Math.Round(nightsSpan.TotalDays, 0);
            return nightCount;
        }
    }
}