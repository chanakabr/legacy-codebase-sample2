using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApi
{
    public class SubscriptionPrice
    {
        private double _price;
        public double Price
        {
            get { return _price; }
            set { _price = value; }
        }

        private string _currency;
        public string Currency
        {
            get { return _currency; }
            set { _currency = value; }
        }
    }
}
