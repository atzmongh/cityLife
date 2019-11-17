using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public partial class Expense
    {
        public Money expenseAsMoney()
        {
            decimal expenseDecimal = ((decimal)this.amount) / 100;     //The price is kept in cents in the DB.
            Money expenseMoney = new Money(expenseDecimal, this.Currency.currencyCode);
            return expenseMoney;
        }
    }
}