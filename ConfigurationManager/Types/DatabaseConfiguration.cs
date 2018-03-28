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
                DefaultValue = 60
            };
            BillingConnectionString = new StringConfigurationValue("BILLING_CONNECTION_STRING", this)
            {
                DefaultValue = "Driver={SQL Server};Server=amazonSQL;Database=billing;Uid=API;Pwd=QxK9yVXASB;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200;"
            };
            ConnectionString = new StringConfigurationValue("CONNECTION_STRING", this)
            {
                DefaultValue = "Driver={SQL Server};Server=amazonSQL;Database=tvinci;Uid=API;Pwd=QxK9yVXASB;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200;"
            };

            MainConnectionString = new StringConfigurationValue("MAIN_CONNECTION_STRING", this)
            {
                DefaultValue = "Driver={SQL Server};Server=amazonSQL;Database=tvinci;Uid=API;Pwd=QxK9yVXASB;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200;"
            };
            ConditionalAccessConnectionString = new StringConfigurationValue("CA_CONNECTION_STRING", this)
            {
                DefaultValue = "Driver={SQL Server};Server=amazonSQL;Database=conditionalaccess;Uid=API;Pwd=QxK9yVXASB;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200;"
            };
            FinancialReportConnectionString = new StringConfigurationValue("FR_CONNECTION_STRING", this)
            {
                DefaultValue = "Driver={SQL Server};Server=amazonSQL;Database=financialreporting;Uid=API;Pwd=QxK9yVXASB;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200;"
            };
            RecordingConnectionString = new StringConfigurationValue("RECORDING_CONNECTION_STRING", this)
            {
                DefaultValue = "Driver={SQL Server};Server=amazonSQL;Database=recording;Uid=API;Pwd=QxK9yVXASB;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200;"
            };
            MessageBoxConnectionString = new StringConfigurationValue("MESSAGE_BOX_CONNECTION_STRING", this)
            {
                DefaultValue = "Driver={SQL Server};Server=amazonSQL;Database=MessageBox;Uid=API;Pwd=QxK9yVXASB;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200;"
            };
            PricingConnectionString = new StringConfigurationValue("pricing_connection", this)
            {
                DefaultValue = "Driver={SQL Server};Server=amazonSQL;Database=pricing;Uid=API;Pwd=QxK9yVXASB;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200;"
            };
            UsersConnectionString = new StringConfigurationValue("users_connection_string", this)
            {
                DefaultValue = "Driver={SQL Server};Server=amazonSQL;Database=users;Uid=API;Pwd=QxK9yVXASB;Trusted_Connection=False;Min Pool Size=5;Max Pool Size=200;"
            };
            UseAlwaysOn = new BooleanConfigurationValue("UseAlwaysOn", this)
            {
                DefaultValue = true
            };
            WriteLockUse = new BooleanConfigurationValue("WriteLock_Use", this)
            {
                DefaultValue = false
            };
            WriteLockParameters = new StringConfigurationValue("WriteLock_Params", this)
            {
                DefaultValue = "userid;user_id;usersid;siteguid;userids;domain_id;domainid;name;site_user_guid;site_guid;userslist;users;co_guid;user_site_guid;username"
            };
            WriteLockTTL = new NumericConfigurationValue("WriteLock_TTL", this)
            {
                DefaultValue = 1
            };
            Prefix = new StringConfigurationValue("prefix", this);
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