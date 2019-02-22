using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public partial class ApartmentDay
    {
        public Money revenueAsMoney()
        {
            decimal priceDecimal = ((decimal)this.revenue) / 100;     //The price is kept in cents in the DB.
            Money priceMoney = new Money(priceDecimal, this.Currency.currencyCode);
            return priceMoney;
        }
    }
}