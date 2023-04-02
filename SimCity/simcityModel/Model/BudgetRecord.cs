using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    /// <summary>
    /// Class for storing data of income/expense in SimCityModel
    /// </summary>
    public class BudgetRecord
    {
        /// <summary>
        /// Represents the text of income/expense
        /// </summary>
        public String Text { get; private set; }

        /// <summary>
        /// Represents the amount of income/expense
        /// </summary>
        public Int32 Amount { get; private set; }

        /// <summary>
        /// Initializes a new instance of BudgetRecord class.
        /// </summary>
        /// <param name="text">Text of the income/expense</param>
        /// <param name="amount">Amount of income/expense</param>
        public BudgetRecord(String text, Int32 amount)
        {
            Text = text;
            Amount = amount;
        }
    }
}
