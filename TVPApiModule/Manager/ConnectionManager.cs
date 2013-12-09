using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApi
{
    public class ConnectionManager
    {
        private int m_groupID;
        private PlatformType m_Platform;
        private bool m_isShared;

        public ConnectionManager(int groupID, PlatformType platformType, bool isShared)
        {
            m_groupID = groupID;
            m_Platform = platformType;
            m_isShared = isShared;
        }

        //Get client specific connection string 
        public string GetClientConnectionString()
        {

            //Get the techinchal manager associated with the current request

            string dbInstance = ConfigManager.GetInstance().GetConfig(m_groupID, m_Platform).TechnichalConfiguration.Data.DBConfiguration.DatabaseInstance;
            //Patchy - for now take all shared items (like favorites) from Web DB! (Waiting for service from Guy)
            if (m_isShared)
            {
                int index = dbInstance.IndexOf(m_Platform.ToString());
                dbInstance = dbInstance.Substring(0, index - 1);
            }
            //return ConfigManager.GetInstance(groupID).TechnichalConfiguration.GenerateConnectionString();
            return string.Concat("Driver={SQL Server};Server=", ConfigManager.GetInstance().GetConfig(m_groupID, m_Platform).TechnichalConfiguration.Data.DBConfiguration.IP,
            ";Database=", dbInstance,
            ";Uid=", ConfigManager.GetInstance().GetConfig(m_groupID, m_Platform).TechnichalConfiguration.Data.DBConfiguration.User,
            ";Pwd=", ConfigManager.GetInstance().GetConfig(m_groupID, m_Platform).TechnichalConfiguration.Data.DBConfiguration.Pass,
            ";");


        }

        //Get the TVINCI DB connection string
        public string GetTvinciConnectionString()
        {
            return string.Concat("Driver={SQL Server};Server=", TVinciDBConfiguration.GetConfig().DBServer,
                    ";Database=", TVinciDBConfiguration.GetConfig().DBInstance,
                    ";Uid=", TVinciDBConfiguration.GetConfig().User,
                    ";Pwd=", TVinciDBConfiguration.GetConfig().Pass,
                    ";");
        }

    }
}
