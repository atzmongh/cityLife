using cityLife4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public partial class Pricing
    {
        private cityLifeDBContainer1 db;
        
        public Money priceWeekendAsMoney()
        {
            return new Money(this.priceWeekEnd, this.Currency.currencyCode);
        }

        public Money priceWeekdayAsMoney()
        {
            return new Money(this.priceWeekDay, this.Currency.currencyCode);
        }
    }
}