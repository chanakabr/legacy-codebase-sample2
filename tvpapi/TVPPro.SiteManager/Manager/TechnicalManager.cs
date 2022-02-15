using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Tvinci.Helpers;
using Tvinci.Helpers.Link;
using System.Configuration;
using Tvinci.Web.HttpModules.Configuration;
using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.TVMDataLoader.Protocols;
using TVPPro.Configuration.Site;
using TVPPro.Configuration.Technical;
using Tvinci.Configuration;
using Tvinci.Localization;
using TVPPro.Configuration.Online;
using Phx.Lib.Log;
using System.Reflection;

namespace TVPPro.SiteManager.Manager
{
    public class TechnicalManager
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        

        //public static bool IsTVMOnline()
        //{

        //    switch (TVPOnlineConfiguration.Instance.Data.TVM.TVMMode)
        //    {
        //        case TVMMode.Main:
        //            return true;
        //        case TVMMode.Default:
        //            // Check tvm online from database
        //            if (string.IsNullOrEmpty(TVMCachingHelper.Instance.TVMOnline))
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //            return true;
        //        case TVMMode.Alternative:
        //            return false;
        //        default:
        //            throw new NotSupportedException();
        //    }
        //}

        public static string GetTVMUrl(bool IsWriteProtocol)
        {
            string result = string.Empty;
            bool blnUseDefaultUrl = false;

            switch (TVPProOnlineConfiguration.Instance.Data.TVM.TVMMode)
            {
                case TVMMode.Main:
                    result = TechnicalConfiguration.Instance.Data.TVM.Servers.MainServer.URL;
                    //check read / write protocol
                    if (IsWriteProtocol)
                    {
                        if (!string.IsNullOrEmpty(TechnicalConfiguration.Instance.Data.TVM.Servers.MainServer.TVMWriteURL))
                            result = TechnicalConfiguration.Instance.Data.TVM.Servers.MainServer.TVMWriteURL;
                        else //if empty return default
                            blnUseDefaultUrl = true;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(TechnicalConfiguration.Instance.Data.TVM.Servers.MainServer.TVMReadURL))
                            result = TechnicalConfiguration.Instance.Data.TVM.Servers.MainServer.TVMReadURL;
                        else //if empty return default
                            blnUseDefaultUrl = true;
                    }

                    if (blnUseDefaultUrl)
                    {
                        result = TechnicalConfiguration.Instance.Data.TVM.Servers.MainServer.URL;
                    }
                    break;
                case TVMMode.Default:
                    if (string.IsNullOrEmpty(TVMCachingHelper.Instance.TVMOnline))
                    {
                        // TVM is online from database
                        result = TechnicalConfiguration.Instance.Data.TVM.Servers.MainServer.URL;
                        //check read / write protocol
                        if (IsWriteProtocol)
                        {
                            if (!string.IsNullOrEmpty(TechnicalConfiguration.Instance.Data.TVM.Servers.MainServer.TVMWriteURL))
                                result = TechnicalConfiguration.Instance.Data.TVM.Servers.MainServer.TVMWriteURL;
                            else //if empty return default
                                blnUseDefaultUrl = true;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(TechnicalConfiguration.Instance.Data.TVM.Servers.MainServer.TVMReadURL))
                                result = TechnicalConfiguration.Instance.Data.TVM.Servers.MainServer.TVMReadURL;
                            else //if empty return default
                                blnUseDefaultUrl = true;
                        }


                        if (blnUseDefaultUrl)
                        {
                            result = TechnicalConfiguration.Instance.Data.TVM.Servers.MainServer.URL;
                        }
                    }
                    else
                    {
                        // No Need
                        // TVM is offline from database
                        //result = TVPProOnlineConfiguration.Instance.GetActiveAlternativeTVMUrl();
                    }
                    break;
                // No Need
                //case TVMMode.Alternative:
                //    result = TVPProOnlineConfiguration.Instance.GetActiveAlternativeTVMUrl();
                //    break;
                default:
                    throw new NotSupportedException();
            }

            result = result.Trim();

            return result;
        }

        //public static bool IsUserCommentor()
        //{
        //    TechnicalProfile profile;

        //    if (TVPProfile.Instance.TryGetTechnicalProfile(out profile))
        //    {
        //        return profile.Editor.EditComments.Value;
        //    }

        //    return false;
        //}

        //public static bool IsUserEditorial()
        //{
        //    TechnicalProfile profile;

        //    if (HttpContext.Current == null || HttpContext.Current.Session == null)
        //    {
        //        return false;
        //    }

        //    if (TVPProfile.Instance.TryGetTechnicalProfile(out profile))
        //    {
        //        return profile.Editor.SuspendSiteCache.Value;
        //    }

        //    return false;
        //}

        public static object GetLanguageID()
        {
            if (HttpContext.Current == null)
            {
                return null;
            }

            return TextLocalization.Instance.UserContext.ValueInDB;
        }

        //public static string GetUserIdentifier()
        //{
        //    return TVPManager.UserIdentifier;

        //}

        //public static string GenerateSms(string userID)
        //{
        //    if (string.IsNullOrEmpty(userID))
        //    {
        //        return string.Empty;
        //    }
        //    else
        //    {
        //        return TVPManager.SMSGenerator.GenerateSms(userID);
        //    }

        //}

        //public static bool ShouldRaiseMaintaince()
        //{
        //    if (TVPOnlineConfiguration.Instance.Data.Modes.SiteMaintaince.Status == Tvinci.Projects.TVP.Core.Configuration.Online.Status2.Off)
        //    {
        //        return false;
        //    }

        //    TechnicalProfile tp;
        //    if (TVPProfile.Instance.TryGetTechnicalProfile(out tp))
        //    {
        //        return (!tp.General.BypassMaintaince);
        //    }

        //    return true;

        //}

        public static object GetRequestLanguageID()
        {
            return TextLocalization.Instance.UserContext.LanguageID;
        }

        public static string GetTVMRequestLanguageValue()
        {
            //Russian 
            return TextLocalization.Instance.UserContext.CultureInfo.EnglishName;
        }

        public static TVMProtocolConfiguration GetTVMConfiguration()
        {
            return TVPPro.Configuration.Technical.TechnicalConfiguration.Instance.TVMConfiguration;
        }

        public static string GetDBConnectionString()
        {
            return TVPPro.Configuration.Technical.TechnicalConfiguration.Instance.GenerateConnectionString();
        }

        //private static bool HandleMapping(string token)
        //{
        //    if (SiteConfiguration.Instance.IsSiteOf("orange"))
        //    {
        //        if (token.ToLower() == "ru")
        //        {
        //            HttpContext.Current.Response.Redirect(QueryStringHelper.CreateQueryString("~/homepage.aspx", new QueryStringPair("Language", "ru")));
        //            HttpContext.Current.Response.End();
        //            return true;
        //        }
        //    }
        //    else if (SiteConfiguration.Instance.IsSiteOf("nds.demo"))
        //    {
        //        if (token.ToLower() == "live" || token.ToLower() == "vod")
        //        {
        //            if (string.IsNullOrEmpty(MultiClientHelper.Instance.ActiveUserClient.ClientIdentifier))
        //            {
        //                HttpContext.Current.Response.Redirect(LinkHelper.ParseURL("~/ClientLogin.aspx"));
        //                HttpContext.Current.Response.End();
        //                return true;
        //            }

        //            long pageID = new ExtractClientPagesAdapter() { Token = token }.Execute();

        //            if (pageID > 0)
        //            {
        //                HttpContext.Current.Response.Redirect(QueryStringHelper.CreateQueryString("~/Galleries.aspx", new QueryStringPair("PageID", pageID.ToString())));
        //                HttpContext.Current.Response.End();
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}

        public static void ApplicationInitialize()
        {
            // setting log file name for cloud
            string EnvironmentClient = System.Configuration.ConfigurationManager.AppSettings["ClientIdentifier"].ToLower();

            //if (!string.IsNullOrEmpty(TVPPro.Configuration.Technical.TechnicalConfiguration.Instance.Data.Site.LogBasePath))
            //{
            //    log4net.GlobalContext.Properties["DebuggingLogFilePath"] = string.Format(@"{0}\{1}\Debugging_{2}.xml", TVPPro.Configuration.Technical.TechnicalConfiguration.Instance.Data.Site.LogBasePath, EnvironmentClient, System.Environment.MachineName);
            //    log4net.GlobalContext.Properties["InformationLogFilePath"] = string.Format(@"{0}\{1}\Information_{2}.xml", TVPPro.Configuration.Technical.TechnicalConfiguration.Instance.Data.Site.LogBasePath, EnvironmentClient, System.Environment.MachineName);
            //    log4net.GlobalContext.Properties["ExceptionsLogFilePath"] = string.Format(@"{0}\{1}\Exceptions_{2}.xml", TVPPro.Configuration.Technical.TechnicalConfiguration.Instance.Data.Site.LogBasePath, EnvironmentClient, System.Environment.MachineName);
            //    log4net.GlobalContext.Properties["PerformancesLogFilePath"] = string.Format(@"{0}\{1}\Performances_{2}.xml", TVPPro.Configuration.Technical.TechnicalConfiguration.Instance.Data.Site.LogBasePath, EnvironmentClient, System.Environment.MachineName);

            //    string logConfigPath = System.Configuration.ConfigurationManager.AppSettings["Log4NetConfiguration"];
            //    if (!string.IsNullOrEmpty(logConfigPath))
            //    {
            //        logConfigPath = Server.MapPath(logConfigPath);
            //        log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(logConfigPath));
            //    }
            //}

            //string configurationPath = System.Configuration.ConfigurationManager.AppSettings["TVP.Core.Configuration.Querystring"];
            //logger.InfoFormat("Initialiing QueryConfigManager from '{0}'", configurationPath);

            //if (!string.IsNullOrEmpty(configurationPath))
            //{
            //    QueryConfigManager.Instance.Initialize(configurationPath);
            //}
            //else
            //{
            //    throw new Exception("missing appsetting key 'TVP.Core.Configuration.Querystring'");
            //}

            logger.InfoFormat("Assigning dynamic methods");
            //MappingModule.HandleMappingMethod = HandleMapping;
            //QueryStringHelper.HandleLanguageMethod = TechnicalManager.HandleLanguage;

            //Tvinci.MultiClient.MultiClientHelper.Instance.ExtractClientsList = SiteManager.ExtractClientsList;
            //Tvinci.MultiClient.MultiClientHelper.Instance.Data.Definitions.Mode = Tvinci.MultiClient.Configuration.Mode.Prohibited;
            //Tvinci.MultiClient.MultiClientHelper.Instance.Data.Definitions.Defaults.ConfigurationID = string.Empty;

            //MainMenuManager.CustomizeLinkMethod = TechnicalManager.CustomizeLink;
            TVPApi.ODBCWrapper.Connection.GetDefaultConnectionStringMethod = TechnicalManager.GetDBConnectionString;
            //Tvinci.Data.DataLoader.LoaderAdapterManager.ForceDataRetrieveMethod = TechnicalManager.IsUserEditorial;
            //Tvinci.Helpers.DatabaseHelper.IsUserEditorialMethod = TechnicalManager.IsUserEditorial;
            //Tvinci.Projects.TVP.Core.Manager.TVPProfile.Instance.PermissionManager.GetUserIdentifierMethod = TechnicalManager.GetUserIdentifier;
            //Tvinci.Projects.TVP.Core.Manager.TVPProfile.Instance.PermissionManager.GenerateSmsCodeMethod = TechnicalManager.GenerateSms;
            //Tvinci.Web.HttpModules.MaintainceModule.ShouldRaiseMaintaince = TechnicalManager.ShouldRaiseMaintaince;
            //iucon.web.Controls.PartialUpdatePanel.GetRequestLanguageIDMethod = TechnicalManager.GetRequestLanguageID;

            Tvinci.Data.DataLoader.LoaderAdapterManager.GetLanguageIDMethod = TechnicalManager.GetLanguageID;
            Tvinci.Data.TVMDataLoader.Protocols.Protocol.GetTVMConfigurationMethod = TechnicalManager.GetTVMConfiguration;
            Tvinci.Data.TVMDataLoader.Protocols.Protocol.GetRequestLanguageMethod = TechnicalManager.GetTVMRequestLanguageValue;
            Tvinci.Data.TVMDataLoader.TVMProvider.GetTVMUrlMethod = TechnicalManager.GetTVMUrl;
            Tvinci.Data.Loaders.CatalogRequestManager.SignatureKey = TVPPro.Configuration.PlatformServices.PlatformServicesConfiguration.Instance.Data.CatalogService.SignatureKey;

            TextLocalization.Instance.Sync(null);
            TextLocalization.Instance.NotExistsAction = Tvinci.Localization.eNotExistsAction.ShowKey;
            TextLocalization.Instance.RegisterInstance();

            // starting the iucon singleton for friendlyurl
            //iucon.web.Controls.PartialUpdatePanelSingleton.Instance.BaseUrl = string.Empty;

            //eNotExistsAction notExistsAction = eNotExistsAction.ShowKey;
            //TextLocalization instance = new TextLocalization() { NotExistsAction = notExistsAction };
            //instance.Sync(TechnicalManager.GetDBConnectionString());
            //instance.RegisterInstance();


            //SiteConfiguration.Sync();
            //TechnicalConfiguration.Sync(delegate(object sender, ItemAddedEventArgs<TechnicalConfiguration> args)
            //{

            //    eNotExistsAction notExistsAction;
            //    // set translation attributes
            //    switch (args.Item.Data.Translation.ActionOnUnknownKey)
            //    {
            //        case ActionOnUnknownKey.ShowKey:
            //            notExistsAction = eNotExistsAction.ShowKey;
            //            break;
            //        case ActionOnUnknownKey.ShowNothing:
            //            notExistsAction = eNotExistsAction.ShowEmptyString;
            //            break;
            //        case ActionOnUnknownKey.ShowHyphen:
            //        default:
            //            notExistsAction = eNotExistsAction.ShowHyphen;
            //            break;
            //    }

            //    TextLocalization instance = new TextLocalization(args.Identifier) { NotExistsAction = notExistsAction };
            //    instance.Sync(args.Item.GenerateConnectionString());
            //    instance.RegisterInstance();



            //    //if (!string.IsNullOrEmpty(args.Item.Data.Flash.PapiInstanceType) && !string.IsNullOrEmpty(args.Item.Data.Flash.PapiIdentifier))
            //    //{
            //    //    System.Type papiType = System.Type.GetType(args.Item.Data.Flash.PapiInstanceType);

            //    //    if (papiType == null)
            //    //    {
            //    //        throw new Exception(string.Format("Failed to get papi type '{0}'", args.Item.Data.Flash.PapiInstanceType));
            //    //    }

            //    //    PapiBase papi = Activator.CreateInstance(papiType) as PapiBase;

            //    //    if (papi == null)
            //    //    {
            //    //        throw new Exception(string.Format("Failed to create instance of papi '{0}'", args.Item.Data.Flash.PapiInstanceType));
            //    //    }


            //    //    switch (args.Item.Data.Flash.XsdMode)
            //    //    {
            //    //        case XsdMode.Required:
            //    //            papi.DefaultExecuteParameters.XsdValidation = ExecuteParameters.eXsdValidation.Required;
            //    //            break;
            //    //        case XsdMode.None:
            //    //        default:
            //    //            papi.DefaultExecuteParameters.XsdValidation = ExecuteParameters.eXsdValidation.None;
            //    //            break;
            //    //    }

            //    //    PapiManager.Instance.RegisterPapi(args.Item.Data.Flash.PapiIdentifier, papi);
            //    //}
            //    //else
            //    //{
            //    //    if (string.IsNullOrEmpty(args.Item.Data.Flash.PapiIdentifier)
            //    //        && string.IsNullOrEmpty(args.Item.Data.Flash.PapiInstanceType))
            //    //    {
            //    //        throw new Exception("Papi configuration are missing. no papi registered to application");
            //    //    }
            //    //    else if (string.IsNullOrEmpty(args.Item.Data.Flash.PapiIdentifier))
            //    //    {
            //    //        throw new Exception("Papi identifier is missing. no papi registered to application");
            //    //    }
            //    //    else if (string.IsNullOrEmpty(args.Item.Data.Flash.PapiInstanceType))
            //    //    {
            //    //        logger.WarnFormat("Papi instance type is missing. assuming that other application raised the papi with identifier '{0}'", args.Item.Data.Flash.PapiIdentifier);
            //    //    }
            //    //}
            //});


            logger.InfoFormat("Check if needs to start TVMCachingHelper");
            // Check if needs to start TVMCachingHelper
            //if (Tvinci.Projects.TVP.Core.Manager.TVPOnlineConfiguration.Instance.Data.TVM.TVMMode == Tvinci.Projects.TVP.Core.Configuration.Online.TVMMode.Default)
            //{
            //    logger.InfoFormat("TVMCachingHelper is going to be activated");
            //    Tvinci.Helpers.TVMCachingHelper.Instance.Start();
            //}
            //else
            //{
            //    logger.InfoFormat("TVMCachingHelper is disactivated");
            //}

            // create and cache search AutoComplete list
            //TVPPro.SiteManager.Helper.DataHelper.GetAutoCompleteList();
            //TVPPro.SiteManager.Helper.MediaMappingHelper.Initialize();
        }

        //public static QueryStringPair HandleLanguage()
        //{
        //    if (HttpContext.Current != null)
        //    {
        //        if (!TextLocalization.Instance.IsDefaultLanguage())
        //        {
        //            return new QueryStringPair("Language", TextLocalization.Instance.UserContext.Culture);
        //        }
        //        else
        //        {
        //            return new QueryStringPair("Language", string.Empty, eItemType.Base64, false);
        //        }
        //    }

        //    return null;
        //}
    }
}
