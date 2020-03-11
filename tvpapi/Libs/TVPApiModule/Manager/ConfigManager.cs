using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;
using System.Threading;
using TVPApi.Configuration.PlatformServices;
using TVPApi.Configuration.Technical;
using TVPApi.Configuration.Site;
using TVPApi.Configuration.Media;
using TVPPro.Configuration.PlatformServices;
using TVPApi.Configuration.OrcaConfiguration;
using KLogMonitor;
using System.Reflection;


/// <summary>
/// Summary description for ConfigurationManager
/// </summary>
/// 

namespace TVPApi
{
    public class ConfigManager
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static ConfigManager m_instance = null;
        private Dictionary<string, ConfigType> m_configs = new Dictionary<string, ConfigType>();
        private static ReaderWriterLockSlim m_ConfigManagerLocker = new ReaderWriterLockSlim();

        public struct ConfigType
        {
            public ApiTechnichalConfiguration TechnichalConfiguration;
            public ApiPlatformServicesConfiguration PlatformServicesConfiguration;
            public ApiSiteConfiguration SiteConfiguration;
            public ApiMediaConfiguration MediaConfiguration;
            public ApiOrcaRecommendationsConfiguration OrcaRecommendationsConfiguration;
        }

        //Get specific config manager by group ID. If this is a new group ID - create new instances of platform configuration and Technichal c
        //configuration (sync from different files)
        public static ConfigManager GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new ConfigManager();
            }
            return m_instance;
        }

        public ConfigType GetConfig(int groupID, PlatformType platform)
        {
            string sKey = string.Concat(groupID.ToString(), platform);

            ConfigType configType = new ConfigType();
            bool bConfigExist = false;

            if (m_ConfigManagerLocker.TryEnterReadLock(1000))
            {
                try
                {
                    bConfigExist = m_configs.TryGetValue(sKey, out configType);
                }
                catch (Exception ex)
                {
                    logger.Error("GetConfig->", ex);
                }
                finally
                {
                    m_ConfigManagerLocker.ExitReadLock();
                }

            }

            if (!bConfigExist)
            {
                if (ConfigurationManager.AppSettings["ConfigSrc_" + groupID] != null)
                {
                    configType = ConfigurationManager.AppSettings["ConfigSrc_" + groupID].ToLower() == "edge"
                                 ? ServiceGetConfig(groupID, platform)
                                 : FileGetConfig(groupID, platform);
                }
                else { configType = FileGetConfig(groupID, platform); }


                if (m_ConfigManagerLocker.TryEnterWriteLock(1000))
                {
                    try
                    {
                        if (!m_configs.Keys.Contains(sKey))
                        {
                            m_configs.Add(sKey, configType);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("GetConfig->", ex);
                    }
                    finally
                    {
                        m_ConfigManagerLocker.ExitWriteLock();
                    }
                }
            }

            return configType;


        }

        private ConfigType ServiceGetConfig(int nGroupID, PlatformType sPlatform)
        {
            ConfigType configType = new ConfigType();
            string sEnvironment = ConfigurationManager.AppSettings["DomainEnv"];

            configType.PlatformServicesConfiguration = new ApiPlatformServicesConfiguration(nGroupID, sPlatform.ToString(), sEnvironment);
            configType.TechnichalConfiguration = new ApiTechnichalConfiguration(nGroupID, sPlatform.ToString(), sEnvironment);
            configType.MediaConfiguration = new ApiMediaConfiguration(nGroupID, sPlatform.ToString(), sEnvironment);
            configType.SiteConfiguration = new ApiSiteConfiguration(nGroupID, sPlatform.ToString(), sEnvironment);

            return configType;
        }

        private ConfigType FileGetConfig(int groupID, PlatformType platform)
        {
            ConfigType configType = new ConfigType();
            string parentDirectoryStr = HttpContext.Current.Server.MapPath(string.Concat(ConfigurationManager.AppSettings[groupID.ToString()], ConfigurationManager.AppSettings["DomainEnv"], "/"));
            string mediaConfigFile = string.Concat(parentDirectoryStr, "MediaConfiguration.config");
            string directoryStr = string.Concat(parentDirectoryStr, platform);

            if (!string.IsNullOrEmpty(directoryStr))
            {
                string[] fileNames = Directory.GetFiles(directoryStr);
                foreach (string file in fileNames)
                {
                    if (file.Contains("Technical"))
                    {
                        configType.TechnichalConfiguration = new TVPApi.Configuration.Technical.ApiTechnichalConfiguration(file);
                    }
                    else if (file.Contains("Platform"))
                    {
                        configType.PlatformServicesConfiguration = new TVPApi.Configuration.PlatformServices.ApiPlatformServicesConfiguration(file);
                    }
                    else if (file.Contains("Site"))
                    {
                        configType.SiteConfiguration = new TVPApi.Configuration.Site.ApiSiteConfiguration(file);
                    }
                    else if (file.Contains("OrcaRecommendations"))
                    {
                        configType.OrcaRecommendationsConfiguration = new TVPApi.Configuration.OrcaConfiguration.ApiOrcaRecommendationsConfiguration(file);
                    }
                }
            }

            configType.MediaConfiguration = new TVPApi.Configuration.Media.ApiMediaConfiguration(mediaConfigFile);

            return configType;
        }

    }
}
