using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Configuration;
using TVPPro.Configuration.PlatformServices;
using Tvinci.Configuration.ConfigSvc;

namespace TVPApi.Configuration.PlatformServices
{
    public class ApiPlatformServicesConfiguration : ConfigurationManager<PlatformServicesData>
    {
        public ApiPlatformServicesConfiguration()
        {
            base.SyncFromFile(System.Configuration.ConfigurationManager.AppSettings["TVPPro.Configuration.PlatformServices"], true);
        }

        public ApiPlatformServicesConfiguration(string syncFile)
        {

                base.SyncFromFile(syncFile, true);
                m_syncFile = syncFile;

        }

        public ApiPlatformServicesConfiguration(int nGroupID, string sPlatform, string sEnvironment)
            : base(eSource.Service)
        {
            SyncFromService(nGroupID, sPlatform, sEnvironment, eConfigType.Platform, CreatePlatformServicesConfig);
        }

        private PlatformServicesData CreatePlatformServicesConfig(IEnumerable<ConfigKeyVal> source)
        {
            PlatformServicesData retVal = new PlatformServicesData
                {
                    ApiService = CreateApiSvcConfig(source),
                    UsersService = CreateUsersSvcConfig(source),
                    ConditionalAccessService = CreateCASvcConfig(source),
                    PricingService = CreatePricingSvcConfig(source),
                    BillingService = CreateBillingSvcConfig(source),
                    DomainsService = CreateDomainsSvcConfig(source),
                    SocialService = CreateSocialSvcConfig(source)
                };

            return retVal;
        }


        private ApiService CreateApiSvcConfig(IEnumerable<ConfigKeyVal> source)
        {
            ApiService retVal = new ApiService()
            {
                URL = DbConfigManager.GetValFromConfig(source, "ApiService_Url"),
                DefaultUser = DbConfigManager.GetValFromConfig(source, "ApiService_User"),
                DefaultPassword = DbConfigManager.GetValFromConfig(source, "ApiService_Pass")
            };

            return retVal;
        }

        private UsersService CreateUsersSvcConfig(IEnumerable<ConfigKeyVal> source)
        {
            UsersService retVal = new UsersService
            {
                URL = DbConfigManager.GetValFromConfig(source, "UsersService_Url"),
                DefaultUser = DbConfigManager.GetValFromConfig(source, "UsersService_User"),
                DefaultPassword = DbConfigManager.GetValFromConfig(source, "UsersService_Pass")
            };

            return retVal;
        }

        private ConditionalAccessService CreateCASvcConfig(IEnumerable<ConfigKeyVal> source)
        {
            ConditionalAccessService retVal = new ConditionalAccessService
            {
                URL = DbConfigManager.GetValFromConfig(source, "ConditionalAccessService_Url"),
                DefaultUser = DbConfigManager.GetValFromConfig(source, "ConditionalAccessService_User"),
                DefaultPassword = DbConfigManager.GetValFromConfig(source, "ConditionalAccessService_Pass")
            };

            return retVal;
        }

        private PricingService CreatePricingSvcConfig(IEnumerable<ConfigKeyVal> source)
        {
            PricingService retVal = new PricingService
            {
                URL = DbConfigManager.GetValFromConfig(source, "PricingService_Url"),
                DefaultUser = DbConfigManager.GetValFromConfig(source, "PricingService_User"),
                DefaultPassword = DbConfigManager.GetValFromConfig(source, "PricingService_Pass")
            };

            return retVal;
        }

        private BillingService CreateBillingSvcConfig(IEnumerable<ConfigKeyVal> source)
        {
            BillingService retVal = new BillingService
            {
                URL = DbConfigManager.GetValFromConfig(source, "BillingService_Url"),
                DefaultUser = DbConfigManager.GetValFromConfig(source, "BillingService_User"),
                DefaultPassword = DbConfigManager.GetValFromConfig(source, "BillingService_Pass")
            };

            return retVal;
        }

        private DomainsService CreateDomainsSvcConfig(IEnumerable<ConfigKeyVal> source)
        {
            DomainsService retVal = new DomainsService
            {

                URL = DbConfigManager.GetValFromConfig(source, "DomainsService_Url"),
                DefaultUser = DbConfigManager.GetValFromConfig(source, "DomainsService_User"),
                DefaultPassword = DbConfigManager.GetValFromConfig(source, "DomainsService_Pass")

            };

            return retVal;
        }

        private SocialService CreateSocialSvcConfig(IEnumerable<ConfigKeyVal> source)
        {
            SocialService retVal = new SocialService()
            {
                URL = DbConfigManager.GetValFromConfig(source, "SocialService_Url"),
                DefaultUser = DbConfigManager.GetValFromConfig(source, "SocialService_User"),
                DefaultPassword = DbConfigManager.GetValFromConfig(source, "SocialService_Pass")
            };

            return retVal;
        }

    }
}
