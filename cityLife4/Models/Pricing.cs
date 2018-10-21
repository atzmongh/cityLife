using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public partial class Pricing
    {
        private cityLifeDBContainer1 db;
        /// <summary>
        /// converts the weekend price into the desired currency
        /// </summary>
        /// <param name="currencyCode">desired currency code</param>
        /// <returns>weekend price in the desired currency</returns>
        public int WeekendPriceBy(string currencyCode, DateTime atDate)
        {
            return this.PriceBy(this.priceWeekEnd, currencyCode, atDate);
        }

        /// <summary>
        /// converts the weekday price into the desired currency
        /// </summary>
        /// <param name="currencyCode">desired currency code</param>
        /// <returns>weekday price in the desired currency</returns>
        public int WeekdayPriceBy(string currencyCode, DateTime atDate)
        {
            return this.PriceBy(this.priceWeekDay, currencyCode, atDate);
        }

        /// <summary>
        /// converts the price to the desired currency
        /// </summary>
        /// <param name="price">the price to convert</param>
        /// <param name="currencyCode">the target currency.</param>
        /// <returns>the price in the desired currency</returns>
        private int PriceBy(int price, string currencyCode, DateTime atDate)
        {
            if (this.Currency.currencyCode == currencyCode)
            {
                //No need to conversion - we already have the desired currency code
                return price;
            }
            else
            {
                Currency fromCurrency = this.Currency;
                this.db = new cityLifeDBContainer1();
                Currency toCurrency = db.Currencies.Single(aCurrency => aCurrency.currencyCode == currencyCode);
                decimal theRate = this.FindExchangeRate(fromCurrency, toCurrency, atDate);
                int convertedPrice = (int)Math.Round(price * theRate);
                return convertedPrice;
            }
        }
        /// <summary>
        /// Find the conversion rate of 2 currencies, for a given date. If there is not data for the exact date, the system will 
        /// look for the closest record which is still earlier than the required date. (if we want to get the rate at 1/10/18, but we only have 
        /// data for 25/9/18 - we will give this rate)
        /// If we are looking, for example, for exchange rate between Euro and Dollar, but we can only find the rate between Dollar and Euro - 
        /// we will use this data - just with 1 divided by the rate.
        /// </summary>
        /// <param name="fromCurrency"></param>
        /// <param name="toCurrency"></param>
        /// <param name="atDate"></param>
        /// <returns>exchange rate between the 2 currencies for the given date.</returns>
        /// <exception cref="100">if no record is found</exception>
        private decimal FindExchangeRate (Currency fromCurrency, Currency toCurrency, DateTime atDate)
        {
            var exchangeRates = from anExchangeRate in db.CurrencyExchanges
                                where (anExchangeRate.FromCurrency.currencyCode == fromCurrency.currencyCode && 
                                       anExchangeRate.ToCurrency.currencyCode == toCurrency.currencyCode) ||
                                      (anExchangeRate.FromCurrency.currencyCode == toCurrency.currencyCode && 
                                       anExchangeRate.ToCurrency.currencyCode == fromCurrency.currencyCode) &&
                                anExchangeRate.date <= atDate
                                orderby anExchangeRate.date descending
                                select anExchangeRate;
            if (exchangeRates.Count() == 0)
            {
                string message = string.Format("exchange rate was not found for currency pair: {0} and (1) for date {2}",
                                                toCurrency.currencyCode, fromCurrency.currencyCode, atDate);
                throw new AppException(100, message);
            }
            CurrencyExchange theExchangeRate = exchangeRates.First();
            decimal theRate = theExchangeRate.conversionRate;
            //Check if the found currency exchange is reversed (the "to" and the "from")
            if (theExchangeRate.ToCurrency.currencyCode == fromCurrency.currencyCode)
            {
                theRate = ((decimal)1) / theRate;
            }
            return theRate;
        }
    }
}