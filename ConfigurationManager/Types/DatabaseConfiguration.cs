using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using System.Collections.Generic;
using System.Linq;

namespace ConfigurationManager
{
    public class DatabaseConfiguration : BaseConfig<DatabaseConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.DatabaseConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<uint> WriteLockTTL = new BaseValue<uint>("WriteLock_TTL", 1);

        public BaseValue<int> ODBCCacheSeconds = new BaseValue<int>("ODBC_CACH_SEC", 60);
        public BaseValue<int> DbCommandExecuteTimeoutSec = new BaseValue<int>("DbCommandExecuteTimeoutSec", 1800);

        public BaseValue<string> BillingConnectionString = new BaseValue<string>("BILLING_CONNECTION_STRING", null, true, "Still not in use, requires massive amount of code refactoring.");
        public BaseValue<string> ConnectionString = new BaseValue<string>("CONNECTION_STRING",null, true, "Still not in use, requires massive amount of code refactoring.");
        public BaseValue<string> MainConnectionString = new BaseValue<string>("MAIN_CONNECTION_STRING",null, true, "Still not in use, requires massive amount of code refactoring.");
        public BaseValue<string> ConditionalAccessConnectionString = new BaseValue<string>("CA_CONNECTION_STRING",null, true, "Still not in use, requires massive amount of code refactoring.");
        public BaseValue<string> FinancialReportConnectionString = new BaseValue<string>("FR_CONNECTION_STRING", null,true, "Still not in use, requires massive amount of code refactoring.");
        public BaseValue<string> RecordingConnectionString = new BaseValue<string>("RECORDING_CONNECTION_STRING", null, true, "Still not in use, requires massive amount of code refactoring.");
        public BaseValue<string> MessageBoxConnectionString = new BaseValue<string>("MESSAGE_BOX_CONNECTION_STRING",null, true, "Still not in use, requires massive amount of code refactoring.");
        public BaseValue<string> PricingConnectionString = new BaseValue<string>("pricing_connection_string",null, true, "Still not in use, requires massive amount of code refactoring.");
        public BaseValue<string> UsersConnectionString = new BaseValue<string>("users_connection_string",null, true, "Still not in use, requires massive amount of code refactoring.");
        public BaseValue<string> WriteLockParameters = new BaseValue<string>("WriteLock_Params", "userid;user_id;usersid;siteguid;userids;domain_id;domainid;name;site_user_guid;site_guid;userslist;users;co_guid;user_site_guid;username");
        public BaseValue<string> Prefix = new BaseValue<string>("prefix", null);

        public BaseValue<bool> UseAlwaysOn = new BaseValue<bool>("UseAlwaysOn", true);
        public BaseValue<bool> WriteLockUse = new BaseValue<bool>("WriteLock_Use", false);


        

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