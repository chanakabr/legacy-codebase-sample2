using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Configuration;
using System.Threading;
using TVPPro.Configuration.Site;
using Tvinci.Configuration.ConfigSvc;

namespace TVPApi.Configuration.Site
{
    public partial class ApiSiteConfiguration : ConfigurationManager<SiteData>
    {

		public ApiSiteConfiguration()
		{
			base.SyncFromFile(ConfigurationManager.AppSettings["TVPPro.Configuration.Site"], true);
		}

        public ApiSiteConfiguration(string syncFile)
        {
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }


        public ApiSiteConfiguration(int nGroupID, string sPlatform, string sEnvironment)
            : base(eSource.Service)
        {
            SyncFromService(nGroupID, sPlatform, sEnvironment, eConfigType.Site, CreateSiteConfig);
        }


		public bool SupportPricing
		{
			get
			{
				return this.Data.Features.Pricing.SupportFeature;
			}
		}

        
        private SiteData CreateSiteConfig(IEnumerable<ConfigKeyVal> source)
        {
            SiteData retVal = new SiteData();

            retVal.Global.DefaultSiteFont = DbConfigManager.GetValFromConfig(source, "Global_Price_Symbol");
            retVal.Global.Title = DbConfigManager.GetValFromConfig(source, "Global_Title");
            retVal.Global.DefaultSiteFont = DbConfigManager.GetValFromConfig(source, "Global_DefaultSiteFont");
            retVal.Global.Price.Location =
                (Location)Enum.Parse(typeof(Location), DbConfigManager.GetValFromConfig(source, "Global_Price_Location"), true);
            retVal.Global.Price.Format = DbConfigManager.GetValFromConfig(source, "Global_Price_Format");
            retVal.Global.Price.Code = DbConfigManager.GetValFromConfig(source, "Global_Price_Code");
            retVal.Pages.ShowPage.EpisodesInPage = int.Parse(DbConfigManager.GetValFromConfig(source, "Pages_ShowPage_EpisodesInPage"));
            retVal.Features.Comments.FeatureMode =
               (FeatureMode)
               Enum.Parse(typeof(FeatureMode), DbConfigManager.GetValFromConfig(source, "Features_Comments_FeatureMode"), true);

            retVal.Features.Comments.AutoActive = bool.Parse(DbConfigManager.GetValFromConfig(source, "Features_Comments_AutoActive"));

            retVal.Features.Pricing.SupportFeature =
                bool.Parse(DbConfigManager.GetValFromConfig(source, "Features_Comments_AutoActive"));

            retVal.Features.FriendlyURL.SupportFeature =
                bool.Parse(DbConfigManager.GetValFromConfig(source, "Features_FriendlyURL_SupportFeature"));

            retVal.Features.FriendlyURL.SupportSearch =
                bool.Parse(DbConfigManager.GetValFromConfig(source, "Features_FriendlyURL_SupportSearch"));

            retVal.Features.SingleLogin.SupportFeature =
                bool.Parse(DbConfigManager.GetValFromConfig(source, "Features_SingleLogin_SupportFeature"));

            retVal.Features.SingleLogin.IntervalTime = DbConfigManager.GetValFromConfig(source, "Features_SingleLogin_IntervalTime");

            retVal.Features.EmbedFlashResponseData.SupportFeature =
                bool.Parse(DbConfigManager.GetValFromConfig(source, "Features_EmbedFlashResponseData_SupportFeature"));

            retVal.Features.Player.Type =
                (TVPPro.Configuration.Site.Type)
                Enum.Parse(typeof(TVPPro.Configuration.Site.Type),
                           DbConfigManager.GetValFromConfig(source, "Features_SingleLogin_IntervalTime"), true);

            retVal.Features.FutureAssets.UseStartDate = DbConfigManager.GetValFromConfig(source, "Features_SingleLogin_IntervalTime");


            return retVal;
        }
	}
}
