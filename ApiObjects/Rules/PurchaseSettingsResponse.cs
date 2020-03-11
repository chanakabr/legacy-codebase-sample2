using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class PurchaseSettingsResponse
    {
        public Status status
        {
            get;
            set;
        }

        public ePurchaeSettingsType? type
        {
            get;
            set;
        }

        public string pin
        {
            get;
            set;
        }

        public eRuleLevel? level
        {
            get;
            set;
        }
    }

    /// <summary>
    /// One of the following options:
    /// -	Block – purchases not allowed
    /// -	Ask – allow purchase subject to purchase PIN
    /// -	Allow – allow purchases with no purchase PIN
    /// </summary>
    public enum ePurchaeSettingsType
    {
        Block = 0,
        Ask = 1,
        Allow = 2
    }
}
