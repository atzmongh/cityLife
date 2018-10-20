using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public partial class Apartment
    {
        public int Price(int adults, int children, bool weekend)
        {
            var thePrice = from price in this.Pricings
                           where price.adults == adults && price.children == children
                           select price;
            if (thePrice.Count()==0)
            {
                //We did not find a pricing record for that number of adults and children
                return 0;
            }
            else
            {
                if (weekend)
                {
                    return thePrice.First().priceWeekEnd;
                }
                else
                {
                    return thePrice.First().priceWeekDay;
                }
            }
        }
    }
}