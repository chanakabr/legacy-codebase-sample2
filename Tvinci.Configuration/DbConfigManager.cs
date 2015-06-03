using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Tvinci.Configuration;
using Tvinci.Configuration.ConfigSvc;


namespace Tvinci.Configuration
{
    public class DbConfigManager
    {
        public IEnumerable<ConfigKeyVal> source { get; set; }

        public DbConfigManager(int nGroupID, string sPlatform, string sEnvironment, eConfigType type)
        {
            //ConfigKeyVal[] resKeyVal;
            using (TVPConfigSvcClient configSvc = new TVPConfigSvcClient())
            {
                source = configSvc.GetConfig(nGroupID, sEnvironment, sPlatform, type);
            }
        }

        public static IEnumerable<string> GetMultipleValsFromConfig(IEnumerable<ConfigKeyVal> source, string sKey)
        {
            IEnumerable<string> retVal = source.Where(kv => kv.Key.ToLower() == sKey.ToLower()).Select(kv => kv.Value);
            return retVal;
        }

        public static string GetValFromConfig(IEnumerable<ConfigKeyVal> source, string sKey)
        {
            ConfigKeyVal keyVal = source.SingleOrDefault(kv => kv.Key.ToLower() == sKey.ToLower());
            return keyVal != null ? keyVal.Value : string.Empty;
        }

        public static bool GetBoolFromConfig(IEnumerable<ConfigKeyVal> source, string sKey)
        {
            ConfigKeyVal keyVal = source.SingleOrDefault(kv => kv.Key.ToLower() == sKey.ToLower());
            if (keyVal != null)
            {
                bool retVal = false;
                if (bool.TryParse(keyVal.Value, out retVal))
                {
                    return retVal;
                }
            }
            return false;
        }


        #region Types



        #endregion


    }
}
