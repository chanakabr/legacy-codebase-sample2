using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Billing
{
    public static class Constants
    {
        public const string BUSINESS_MODULE_TYPE = "type";
        public const string SITE_GUID = "id";
        public const string SUBSCRIPTION_ID = "s";
        public const string PRE_PAID_ID = "pp"; // not used in cinepolis
        public const string PRE_PAID_CREDIT_VALUE = "cpri"; // not used in cinepolis
        public const string COUPON_CODE = "cc";
        public const string PAYMENT_NUMBER = "n";
        public const string NUMBER_OF_PAYMENTS = "o";
        public const string IS_RECURRING = "ir";
        public const string MEDIA_FILE = "mf";
        public const string PPV_MODULE = "ppvm";
        public const string RELEVANT_SUBSCRIPTION = "rs";
        public const string MAX_NUM_OF_USES = "mnou";
        public const string COUNTRY_CODE = "lcc";
        public const string LANGUAGE_CODE = "llc";
        public const string DEVICE_NAME = "ldn";
        public const string MAX_USAGE_MODULE_LIFE_CYCLE = "mumlc";
        public const string VIEW_LIFE_CYCLE_SECS = "vlcs";
        public const string CC_DIGITS = "cc_card_number";
        public const string PRICE = "pri";
        public const string CURRENCY = "cu";
        public const string USER_IP = "up";
        public const string CAMPAIGN_CODE = "campcode";
        public const string CAMPAIGN_MAX_NUM_OF_USES = "cmnov";
        public const string CAMPAIGN_MAX_LIFE_CYCLE = "cmumlc";
        public const string OVERRIDE_END_DATE = "oed";
        public const string PREVIEW_MODULE = "pm";
        public const string PRICE_CODE = "pc";
        public const string MEDIA_ID = "m";
        public const string DOMAIN = "domain";
    }
}
