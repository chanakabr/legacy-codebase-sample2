using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

/// <summary>
/// Summary description for TVinciDBConfiguration
/// </summary>
/// 

namespace TVPApi
{
    //Wraps the web.config TVinciDBConfig section (holding attributes regarding main TVinci DB)
    public class TVinciDBConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("User", IsRequired = true)]
        public string User
        {
            get
            {
                return this["User"] as string;
            }
        }

        [ConfigurationProperty("Pass", IsRequired = true)]
        public string Pass
        {
            get
            {
                return this["Pass"] as string;
            }
        }

        [ConfigurationProperty("DBInstance", IsRequired = true)]
        public string DBInstance
        {
            get
            {
                return this["DBInstance"] as string;
            }
        }

        [ConfigurationProperty("DBServer", IsRequired = true)]
        public string DBServer
        {
            get
            {
                return this["DBServer"] as string;
            }
        }

        //Get the types TVinci DB consig section
        public static TVinciDBConfiguration GetConfig()
        {
            return ConfigurationSettings.GetConfig("TVinciDBConfig") as TVinciDBConfiguration;
        }


    }

}
