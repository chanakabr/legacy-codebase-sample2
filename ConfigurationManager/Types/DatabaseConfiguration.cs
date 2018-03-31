using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class DatabaseConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue ODBCCacheSeconds;
        public StringConfigurationValue BillingConnectionString;
        public StringConfigurationValue ConnectionString;
        public StringConfigurationValue MainConnectionString;
        public StringConfigurationValue ConditionalAccessConnectionString;
        public StringConfigurationValue FinancialReportConnectionString;
        public StringConfigurationValue RecordingConnectionString;
        public StringConfigurationValue MessageBoxConnectionString;
        public StringConfigurationValue PricingConnectionString;
        public StringConfigurationValue UsersConnectionString;
        public BooleanConfigurationValue UseAlwaysOn;
        public BooleanConfigurationValue WriteLockUse;
        public StringConfigurationValue WriteLockParameters;
        public NumericConfigurationValue WriteLockTTL;
        public StringConfigurationValue Prefix;

        public DatabaseConfiguration(string key) : base(key)
        {
            ODBCCacheSeconds = new NumericConfigurationValue("ODBC_CACH_SEC", this)
            {
                DefaultValue = 60,
                OriginalKey = "ODBC_CACH_SEC",
            };
            BillingConnectionString = new StringConfigurationValue("BILLING_CONNECTION_STRING", this)
            {
                OriginalKey = "BILLING_CONNECTION_STRING"
            };
            ConnectionString = new StringConfigurationValue("CONNECTION_STRING", this)
            {
                OriginalKey = "CONNECTION_STRING"
            };
            MainConnectionString = new StringConfigurationValue("MAIN_CONNECTION_STRING", this)
            {
                OriginalKey = "MAIN_CONNECTION_STRING"
            };
            ConditionalAccessConnectionString = new StringConfigurationValue("CA_CONNECTION_STRING", this)
            {
                OriginalKey = "CA_CONNECTION_STRING"
            };
            FinancialReportConnectionString = new StringConfigurationValue("FR_CONNECTION_STRING", this)
            {
                OriginalKey = "FR_CONNECTION_STRING"
            };
            RecordingConnectionString = new StringConfigurationValue("RECORDING_CONNECTION_STRING", this)
            {
                OriginalKey = "RECORDING_CONNECTION_STRING"
            };
            MessageBoxConnectionString = new StringConfigurationValue("MESSAGE_BOX_CONNECTION_STRING", this)
            {
                OriginalKey = "MESSAGE_BOX_CONNECTION_STRING"
            };
            PricingConnectionString = new StringConfigurationValue("pricing_connection_string", this)
            {
                OriginalKey = "pricing_connection"
            };
            UsersConnectionString = new StringConfigurationValue("users_connection_string", this)
            {
                OriginalKey = "users_connection_string"
            };
            UseAlwaysOn = new BooleanConfigurationValue("UseAlwaysOn", this)
            {
                DefaultValue = true,
                OriginalKey = "UseAlwaysOn"
            };
            WriteLockUse = new BooleanConfigurationValue("WriteLock_Use", this)
            {
                DefaultValue = false,
                OriginalKey = "DB_WriteLock_Use"
            };
            WriteLockParameters = new StringConfigurationValue("WriteLock_Params", this)
            {
                DefaultValue = "userid;user_id;usersid;siteguid;userids;domain_id;domainid;name;site_user_guid;site_guid;userslist;users;co_guid;user_site_guid;username",
                OriginalKey = "DB_WriteLock_Params"
            };
            WriteLockTTL = new NumericConfigurationValue("WriteLock_TTL", this)
            {
                DefaultValue = 1,
                OriginalKey = "DB_WriteLock_TTL"
            };
            Prefix = new StringConfigurationValue("prefix", this)
            {
                OriginalKey = "DB_Settings.prefix"
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= ODBCCacheSeconds.Validate();
            result &= BillingConnectionString.Validate();
            result &= ConnectionString.Validate();
            result &= MainConnectionString.Validate();
            result &= ConditionalAccessConnectionString.Validate();
            result &= FinancialReportConnectionString.Validate();
            result &= RecordingConnectionString.Validate();
            result &= MessageBoxConnectionString.Validate();
            result &= PricingConnectionString.Validate();
            result &= UsersConnectionString.Validate();
            result &= UseAlwaysOn.Validate();
            result &= WriteLockUse.Validate();
            result &= WriteLockParameters.Validate();
            result &= WriteLockTTL.Validate();
            result &= Prefix.Validate();

            return result;
        }

        public List<string> GetWriteLockParameters()
        {
            List<string> result = null;

            if (!string.IsNullOrEmpty(this.WriteLockParameters.Value))
            {
                result = this.WriteLockParameters.Value.Split(';').ToList();
            }

            return result;
        }
    }
}