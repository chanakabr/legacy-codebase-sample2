using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using TCMClient;

namespace ConfigurationManager
{
    public class ApplicationConfiguration
    {
        #region Configuration values

        public static StringConfigurationValue DMSUrl;
        public static BooleanConfigurationValue UseUserCache;
        public static MailerConfiguration MailerConfiguration;
        public static GroupsManagerConfiguration GroupsManagerConfiguration;
        public static RequestParserConfiguration RequestParserConfiguration;
        public static OTTUserControllerConfiguration OTTUserControllerConfiguration;
        public static CouchbaseSectionMapping CouchbaseSectionMapping;
        public static BaseCacheConfiguration BaseCacheConfiguration;
        public static DatabaseConfiguration DatabaseConfiguration;
        public static WSCacheSettings WSCacheSettings;
        public static CouchBaseDesigns CouchBaseDesigns;
        public static NumericConfigurationValue EPGDocumentExpiry;
        public static EutelsatSettings EutelsatSettings;
        public static ElasticSearchConfiguration ElasticSearchConfiguration;
        public static StringConfigurationValue CatalogSignatureKey;
        public static HarmonicProviderConfiguration HarmonicProviderConfiguration;
        public static RabbitConfiguration RabbitConfiguration;
        public static RoleIdsConfiguration RoleIdsConfiguration;
        public static StringConfigurationValue ExcludePsDllImplementation;
        public static NumericConfigurationValue DomainCacheDocTimeout;
        public static FacebookConfiguration FacebookConfiguration;
        public static StringConfigurationValue NotificationsService;
        public static NumericConfigurationValue PlayCycleDocumentExpiryMinutes;
        public static TwitterConfiguration TwitterConfiguration;
        public static SocialFeedConfiguration SocialFeedConfiguration;
        public static StringConfigurationValue UsersAssemblyLocation;
        public static StringConfigurationValue FriendsActivityViewStaleState;
        public static LicensedLinksCacheConfiguration LicensedLinksCacheConfiguration;
        public static SocialFeedQueueConfiguration SocialFeedQueueConfiguration;
        public static LayeredCacheConfigurationValidation LayeredCacheConfigurationValidation;
        public static ExportConfiguration ExportConfiguration;
        public static StringConfigurationValue UDRMUrl;
        public static StringConfigurationValue MediaSearcher;
        public static StringConfigurationValue UseOldImageServer;
        public static CatalogLogicConfiguration CatalogLogicConfiguration;
        public static AnnouncementManagerConfiguration AnnouncementManagerConfiguration;
        public static StringConfigurationValue DMSAdapterUrl;
        public static NumericConfigurationValue PersonalizedFeedTTLDays;
        public static EngagementsConfiguration EngagementsConfiguration;
        public static NumericConfigurationValue UserInterestsTTLDays;
        public static StringConfigurationValue PlayManifestDynamicQueryStringParamsNames;
        public static NumericConfigurationValue RecordingsMaxDegreeOfParallelism;
        public static StringConfigurationValue CatalogWSUrl;
        public static EventConsumersConfiguration EventConsumersConfiguration;
        public static AuthorizationManagerConfiguration AuthorizationManagerConfiguration;
        public static UserPINDigitsConfiguration UserPINDigitsConfiguration;
        public static WebServicesConfiguration WebServicesConfiguration;
        public static BooleanConfigurationValue ShouldGetCatalogDataFromDB;
        public static NumericConfigurationValue CrowdSourcerFeedNumberOfItems;
        public static PushMessagesConfiguration PushMessagesConfiguration;
        public static NumericConfigurationValue QueueFailLimit;
        public static StringConfigurationValue Version;
        public static NumericConfigurationValue PendingThresholdDays;
        public static BooleanConfigurationValue DownloadPicWithQueue;
        public static StringConfigurationValue EPGUrl;        

        #endregion

        #region Private Members

        private static List<ConfigurationValue> AllConfigurationValues;

        #endregion

        #region Public Static Methods

        public static void Initialize(bool shouldLoadDefaults = false)
        {
            TCMClient.Settings.Instance.Init();

            DMSUrl = new StringConfigurationValue("dms_url");
            UseUserCache = new BooleanConfigurationValue("UseUsersCache")
            {
                DefaultValue = true
            };

            MailerConfiguration = new MailerConfiguration("MC");

            GroupsManagerConfiguration = new GroupsManagerConfiguration("groups_manager");

            RequestParserConfiguration = new RequestParserConfiguration("request_parser");
            OTTUserControllerConfiguration = new OTTUserControllerConfiguration("ott_user_controller");
            CouchbaseSectionMapping = new CouchbaseSectionMapping("CouchbaseSectionMapping");
            BaseCacheConfiguration = new BaseCacheConfiguration("base_cache_configuration");
            DatabaseConfiguration = new DatabaseConfiguration("database_configuration");
            WSCacheSettings = new WSCacheSettings("ws_cache_settings");
            CouchBaseDesigns = new CouchBaseDesigns("couchbase_designs");
            EPGDocumentExpiry = new NumericConfigurationValue("epg_doc_expiry")
            {
                DefaultValue = 7
            };
            EutelsatSettings = new EutelsatSettings("eutelsat_settings");
            ElasticSearchConfiguration = new ElasticSearchConfiguration("elasticsearch_settings");
            CatalogSignatureKey = new StringConfigurationValue("CatalogSignatureKey")
            {
                DefaultValue = "liat regev"
            };
            HarmonicProviderConfiguration = new HarmonicProviderConfiguration("harmonic_provider_configuration");
            RabbitConfiguration = new RabbitConfiguration("rabbit_configuration");
            RoleIdsConfiguration = new RoleIdsConfiguration("role_ids");
            ExcludePsDllImplementation = new StringConfigurationValue("EXCLUDE_PS_DLL_IMPLEMENTATION")
            {
                ShouldAllowEmpty = true
            };
            DomainCacheDocTimeout = new NumericConfigurationValue("DomainCacheDocTimeout")
            {
                DefaultValue = 1440
            };
            FacebookConfiguration = new FacebookConfiguration("facebook_configuration");
            NotificationsService = new StringConfigurationValue("NotificationService")
            {
            };
            PlayCycleDocumentExpiryMinutes = new NumericConfigurationValue("playCycle_doc_expiry_min")
            {
                DefaultValue = 60,
                Description = "TTL for CouchBase documents of play cycle data in minutes."
            };
            TwitterConfiguration = new TwitterConfiguration("twitter_configuration")
            {
            };
            SocialFeedConfiguration = new SocialFeedConfiguration("social_feed_configuration");
            UsersAssemblyLocation = new StringConfigurationValue("USERS_ASSEMBLY_LOCATION")
            {
                ShouldAllowEmpty = true
            };
            FriendsActivityViewStaleState = new StringConfigurationValue("FRIENDS_ACTIVITY_VIEW_STALE_STATE")
            {
                DefaultValue = "None",
                ShouldAllowEmpty = true,
                Description = "Corresponding to ViewStaleState enum. Possible values: None, False, Ok, UpdateAfter"
            };
            LicensedLinksCacheConfiguration = new LicensedLinksCacheConfiguration("licensed_links_cache_configuration");
            SocialFeedQueueConfiguration = new SocialFeedQueueConfiguration("social_feed_queue_configuration");
            LayeredCacheConfigurationValidation = new ConfigurationManager.LayeredCacheConfigurationValidation("LayeredCache");
            ExportConfiguration = new ConfigurationManager.ExportConfiguration("export");
            UDRMUrl = new ConfigurationManager.StringConfigurationValue("UDRM_URL")
            {
                DefaultValue = "https://ny-udrm-stg.kaltura.com"
            };
            MediaSearcher = new ConfigurationManager.StringConfigurationValue("media_searcher")
            {
                DefaultValue = "Core.Catalog.ElasticsearchWrapper, ApiLogic"
            };
            UseOldImageServer = new ConfigurationManager.StringConfigurationValue("USE_OLD_IMAGE_SERVER")
            {
                DefaultValue = "0",
                Description = "Group Ids, split by ';', that wish to use old image server"
            };
            CatalogLogicConfiguration = new CatalogLogicConfiguration("catalog_logic_configuration");
            AnnouncementManagerConfiguration = new ConfigurationManager.AnnouncementManagerConfiguration("announcement_manager_configuration");
            DMSAdapterUrl = new ConfigurationManager.StringConfigurationValue("DMS_ADAPTER_URL")
            {

            };
            PersonalizedFeedTTLDays = new NumericConfigurationValue("PersonalizedFeedTTLDays")
            {
                DefaultValue = 365
            };
            EngagementsConfiguration = new ConfigurationManager.EngagementsConfiguration("engagements_configuration");
            UserInterestsTTLDays = new ConfigurationManager.NumericConfigurationValue("ttl_user_interest_days")
            {
                DefaultValue = 30
            };
            PlayManifestDynamicQueryStringParamsNames = new ConfigurationManager.StringConfigurationValue("PlayManifestDynamicQueryStringParamsNames")
            {
                DefaultValue = "clientTag,playSessionId"
            };
            RecordingsMaxDegreeOfParallelism = new ConfigurationManager.NumericConfigurationValue("recordings_max_degree_of_parallelism")
            {
                DefaultValue = 5
            };
            CatalogWSUrl = new ConfigurationManager.StringConfigurationValue("WS_Catalog")
            {

            };
            EventConsumersConfiguration = new ConfigurationManager.EventConsumersConfiguration("ConsumerSettings");
            AuthorizationManagerConfiguration = new ConfigurationManager.AuthorizationManagerConfiguration("authorization_manager_configuration");
            UserPINDigitsConfiguration = new UserPINDigitsConfiguration("user_pin_digits_configuration");
            WebServicesConfiguration = new ConfigurationManager.WebServicesConfiguration("WebServices");
            ShouldGetCatalogDataFromDB = new ConfigurationManager.BooleanConfigurationValue("get_catalog_data_from_db")
            {
                DefaultValue = false,
                ShouldAllowEmpty = true,
                Description = "Just in case media mark information is not in Couchbase, we might want to continue to DB. Should be false or empty."
            };
            CrowdSourcerFeedNumberOfItems = new ConfigurationManager.NumericConfigurationValue("crowdsourcer.FEED_NUM_OF_ITEMS")
            {
                DefaultValue = 0,
                ShouldAllowEmpty = true
            };
            PushMessagesConfiguration = new ConfigurationManager.PushMessagesConfiguration("push_messages");
            QueueFailLimit = new ConfigurationManager.NumericConfigurationValue("queue_fail_limit")
            {
                DefaultValue = 3,
                ShouldAllowEmpty = true
            };
            Version = new ConfigurationManager.StringConfigurationValue("Version");
            PendingThresholdDays = new ConfigurationManager.NumericConfigurationValue("pending_threshold_days")
            {
                DefaultValue = 180,
                ShouldAllowEmpty = true
            };
            DownloadPicWithQueue = new ConfigurationManager.BooleanConfigurationValue("downloadPicWithQueue")
            {
                DefaultValue = false,
                ShouldAllowEmpty = true
            };

            EPGUrl = new ConfigurationManager.StringConfigurationValue("EPGUrl")
            {
                ShouldAllowEmpty = true,
                Description = "Use in yes epg BL"
            };

            AllConfigurationValues = new List<ConfigurationValue>()
                {
                    DMSUrl,
                    UseUserCache,
                    MailerConfiguration,
                    GroupsManagerConfiguration,
                    RequestParserConfiguration,
                    OTTUserControllerConfiguration,
                    CouchbaseSectionMapping,
                    BaseCacheConfiguration,
                    DatabaseConfiguration,
                    WSCacheSettings,
                    CouchBaseDesigns,
                    EPGDocumentExpiry,
                    EutelsatSettings,
                    ElasticSearchConfiguration,
                    CatalogSignatureKey,
                    HarmonicProviderConfiguration,
                    RabbitConfiguration,
                    RoleIdsConfiguration,
                    ExcludePsDllImplementation,
                    DomainCacheDocTimeout,
                    FacebookConfiguration,
                    NotificationsService,
                    PlayCycleDocumentExpiryMinutes,
                    TwitterConfiguration,
                    SocialFeedConfiguration,
                    UsersAssemblyLocation,
                    FriendsActivityViewStaleState,
                    LicensedLinksCacheConfiguration,
                    SocialFeedQueueConfiguration,
                    LayeredCacheConfigurationValidation,
                    ExportConfiguration,
                    UDRMUrl,
                    MediaSearcher,
                    UseOldImageServer,
                    CatalogLogicConfiguration,
                    AnnouncementManagerConfiguration,
                    PersonalizedFeedTTLDays,
                    EngagementsConfiguration,
                    UserInterestsTTLDays,
                    PlayManifestDynamicQueryStringParamsNames,
                    RecordingsMaxDegreeOfParallelism,
                    CatalogWSUrl,
                    EventConsumersConfiguration,
                    AuthorizationManagerConfiguration,
                    UserPINDigitsConfiguration,
                    WebServicesConfiguration,
                    ShouldGetCatalogDataFromDB,
                    CrowdSourcerFeedNumberOfItems,
                    PushMessagesConfiguration,
                    QueueFailLimit,
                    Version,
                    PendingThresholdDays,
                    DownloadPicWithQueue,
                    EPGUrl
                };

            if (shouldLoadDefaults)
            {
                foreach (var configurationValue in AllConfigurationValues)
                {
                    configurationValue.LoadDefault();
                }
            }
        }

        public static bool Validate()
        {
            bool result = true;

            try
            {
                Initialize();

                foreach (var configurationValue in AllConfigurationValues)
                {
                    result &= configurationValue.Validate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception when validating: {0}", ex.Message));
                result = false;
            }

            Console.WriteLine(string.Format("Finished validating TCM and result is {0}", result));

            return result;
        }

        #endregion
    }
}