using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    /// <summary>
    /// Money represents an amount of money in a specific currency.
    /// </summary>
    public class Money
    {
        private decimal amount=0;
        private string currencyCode;
        private static cityLifeDBContainer1 db = new cityLifeDBContainer1();
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="anAmount">the money amount</param>
        /// <param name="aCurrencyCode">should be a valid currency code, which exists in the DB. The constructor does not check for validity.
        /// Main currency codes are: USD, EUR, UAH, ILS... See list of currency codes in: https://www.iban.com/currency-codes
        /// </param>
        public Money(decimal anAmount, string aCurrencyCode)
        {
            amount = anAmount;
            currencyCode = aCurrencyCode;
        }
        public Money(Money m)
        {
            amount = m.amount;
            currencyCode = m.currencyCode;
        }
        /// <summary>
        /// returns a string representation of the Money object as : USD 156.32
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return currencyCode + " "+string.Format("{0:###0.00}",this.amount);
        }

        /// <summary>
        /// converts the money object to string representation
        /// </summary>
        /// <param name="showCents">when true will show cents as 123.45 When false will round to the closest integral value</param>
        /// <returns>$1,234.56 orf $1235 bsaed on the showCents param</returns>
        public string toMoneyString(bool showCents = true)
        {
            var theCurrency = Money.db.Currencies.SingleOrDefault(aCurrency => aCurrency.currencyCode == this.currencyCode);
            if (theCurrency == null)
            {
                throw new AppException(102, "such currency does not exist in DB:" + this.currencyCode);
            }
            string moneyString;
            if (showCents)
            {
                moneyString = string.Format("{0}{1:#,##0.00}", theCurrency.symbol, this.amount);
            }
            else
            {
                moneyString = string.Format("{0}{1:#,##0}", theCurrency.symbol, decimal.Round(this.amount));
            }
            
            return moneyString;
        }

        /// <summary>
        /// Converts the amount of money from currency to currency based on currency exchange records in the DB.
        /// </summary>
        /// <param name="newCurrencyCode">target currency to which we need to convert</param>
        /// <param name="atDate">the date for which we would like to convert. The method will look for latest currency 
        /// exchange record which is still before or at that date. </param>
        /// <param name="rounded">When set to true - rounds the converted result to integral value</param>
        /// <returns>a Money object converted to the new currency.</returns>
        /// <exception cref="101">no suitable currency exchange record found in the DB. (Either the currency code was illegal, or no
        /// such record exists)</exception>
        public Money converTo(string newCurrencyCode, DateTime atDate, bool rounded = false)
        {
            if (currencyCode == newCurrencyCode)
            {
                //current and target currencies are the same - no conversion needed.
                return new Money(this);
            }
            else
            {
                //Look for all exchange rates records that match the "to currency" and "from currency". 
                //Note that if we wish to convert from USD to EUR, but in the DB we find only a conversion from EUR to USD - we can use this record  
                //as well.
                var exchangeRates = from anExchangeRate in Money.db.CurrencyExchanges
                                    where (anExchangeRate.FromCurrency.currencyCode == this.currencyCode &&
                                          anExchangeRate.ToCurrency.currencyCode    == newCurrencyCode) ||
                                          (anExchangeRate.FromCurrency.currencyCode == newCurrencyCode &&
                                          anExchangeRate.ToCurrency.currencyCode    == this.currencyCode) &&
                                          anExchangeRate.date <= atDate
                                    orderby anExchangeRate.date descending
                                    select anExchangeRate;
                if (exchangeRates.Count() == 0)
                {
                    //no suitable exchange rate was found
                    string message = string.Format("no suitable exchange rate was found for currencies: {0} and {1} for date {2} (or later). Sum was {3}",
                                                               this.currencyCode, newCurrencyCode, atDate, this.amount);
                    throw new AppException(101, message);
                }
                var theExchangeRate = exchangeRates.First();  //take the latest record which is still before or at the requested date
                decimal result;
                if (theExchangeRate.FromCurrency.currencyCode == this.currencyCode)
                {
                    result = this.amount * theExchangeRate.conversionRate;
                }
                else
                {
                    result = this.amount / theExchangeRate.conversionRate;
                }
                if (rounded)
                {
                    result = decimal.Round(result);
                }
                return new Money(result, newCurrencyCode);
            }
        }
    }
}