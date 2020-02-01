using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EngagementAdapter
{
    public class AdapterConstants
    {
        // messages strings
        public const string OK = "OK";
        public const string ERROR = "Internal error";
        public const string SIGNATURE_MISMATCH = "Signature does not match";
        public const string CONFIGURATION_NOT_FOUND = "Configuration not found";
        public const string PROVIDER_URL_ERROR = "Provider URL not found";
        public const string SECRET_KEY_ERROR = "Secret key not found";
        public const string SYSTEM_CODE_ERROR = "System code not found";
        public const string HAPPINESS_LEVEL_ERROR = "Happiness level was not found";

        // configuration key strings
        public const string SECRET_KEY = "secret_key";
        public const string SYSTEM_CODE = "system_code";
        public const string HAPPINESS_LEVEL = "Happiness_level";
    }
}