using System;
using System.Collections.Generic;

namespace ConfigurationManager
{
    public class ApplicationConfiguration
    {
        #region Remote Tasks Configuration Values

        public static CeleryRoutingConfiguration CeleryRoutingConfiguration;
        public static ImageResizerConfiguration ImageResizerConfiguration;
        public static StringConfigurationValue GraceNoteXSLTPath;
        public static StringConfigurationValue GraceNoteALUIdConvertion;
        public static ElasticSearchHandlerConfiguration ElasticSearchHandlerConfiguration;
        public static BooleanConfigurationValue ShouldDistributeRecordingSynchronously;

        #endregion

        #region TVM Configuration Values

        public static StringConfigurationValue PictureUploaderPath;
        public static StringConfigurationValue TVMBaseUrl;
        public static BooleanConfigurationValue TVMSkipLoginIPCheck;
        public static StringConfigurationValue ClearCachePath;
        public static StringConfigurationValue StagingClearCachePath;
        public static StringConfigurationValue BatchUpload;
        public static StringConfigurationValue LookupGenericUpload;
        public static BooleanConfigurationValue EnableHttpLogin;
        public static StringConfigurationValue AppState;
        public static StringConfigurationValue ServerName;
        public static StringConfigurationValue ApplicationName;

        #endregion

        #region Configuration values

        public static NumericConfigurationValue CrowdSourceTimeSpan;
        public static StringConfigurationValue DMSUrl;
        public static MailerConfiguration MailerConfiguration;
        public static GroupsManagerConfiguration GroupsManagerConfiguration;
        public static RequestParserConfiguration RequestParserConfiguration;
        public static OTTUserControllerConfiguration OTTUserControllerConfiguration;
        public static CouchbaseSectionMapping CouchbaseSectionMapping;
        public static UsersCacheConfiguration UsersCacheConfiguration;
        public static NamedCacheConfiguration BaseCacheConfiguration;
        public static DatabaseConfiguration DatabaseConfiguration;
        public static NamedCacheConfiguration WSCacheConfiguration;
        public static NamedCacheConfiguration ODBCWrapperCacheConfiguration;
        public static NamedCacheConfiguration CatalogCacheConfiguration;
        public static NamedCacheConfiguration NotificationCacheConfiguration;
        public static NamedCacheConfiguration GroupsCacheConfiguration;
        public static StringConfigurationValue SingleInMemoryCacheName;
        public static CouchBaseDesigns CouchBaseDesigns;
        public static NumericConfigurationValue EPGDocumentExpiry;
        public static EutelsatSettings EutelsatSettings;
        public static ElasticSearchConfiguration ElasticSearchConfiguration;
        public static StringConfigurationValue SearchIndexType;
        public static StringConfigurationValue CatalogSignatureKey;
        public static HarmonicProviderConfiguration HarmonicProviderConfiguration;
        public static RabbitConfiguration RabbitConfiguration;
        public static RoleIdsConfiguration RoleIdsConfiguration;
        public static StringConfigurationValue ExcludePsDllImplementation;
        public static NumericConfigurationValue DomainCacheDocTimeout;
        public static NumericConfigurationValue SocialCacheDocTimeout;
        public static FacebookConfiguration FacebookConfiguration;
        public static NumericConfigurationValue PlayCycleDocumentExpiryMinutes;
        public static TwitterConfiguration TwitterConfiguration;
        public static SocialFeedConfiguration SocialFeedConfiguration;
        public static StringConfigurationValue UsersAssemblyLocation;
        public static StringConfigurationValue FriendsActivityViewStaleState;
        public static LicensedLinksCacheConfiguration LicensedLinksCacheConfiguration;
        public static SocialFeedQueueConfiguration SocialFeedQueueConfiguration;
        public static LayeredCacheConfigurationValidation LayeredCacheConfigurationValidation;
        public static ExportConfiguration ExportConfiguration;
        public static NumericConfigurationValue BillingCacheTTL;
        public static StringConfigurationValue UDRMUrl;
        public static StringConfigurationValue UseOldImageServer;
        public static CatalogLogicConfiguration CatalogLogicConfiguration;
        public static AnnouncementManagerConfiguration AnnouncementManagerConfiguration;
        public static CDVRAdapterConfiguration CDVRAdapterConfiguration;
        public static StringConfigurationValue DMSAdapterUrl;
        public static NumericConfigurationValue PersonalizedFeedTTLDays;
        public static EngagementsConfiguration EngagementsConfiguration;
        public static NumericConfigurationValue UserInterestsTTLDays;
        public static StringConfigurationValue PlayManifestDynamicQueryStringParamsNames;
        public static NumericConfigurationValue RecordingsMaxDegreeOfParallelism;
        public static NumericConfigurationValue ReconciliationFrequencySeconds;
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
        public static BooleanConfigurationValue CheckImageUrl;
        public static ImageUtilsConfiguration ImageUtilsConfiguration;
        public static StringConfigurationValue EPGUrl;
        public static StringConfigurationValue GroupIDsWithIPNOFilteringSeperatedBySemiColon;
        public static StringConfigurationValue EncryptorService;
        public static StringConfigurationValue EncryptorPassword;
        public static StringConfigurationValue PicsBasePath;
        public static NotificationConfiguration NotificationConfiguration;
        public static StringConfigurationValue IngestFtpUrl;
        public static StringConfigurationValue IngestFtpUser;
        public static StringConfigurationValue IngestFtpPass;

        #endregion

        #region Private Members

        private static List<ConfigurationValue> AllConfigurationValues;

        #endregion

        #region Public Static Methods

        public static void Initialize(bool shouldLoadDefaults = false)
        {
            TCMClient.Settings.Instance.Init();

            #region Remote tasks configuration values

            CeleryRoutingConfiguration = new ConfigurationManager.CeleryRoutingConfiguration("CELERY_ROUTING")
            {
                ShouldAllowEmpty = true,
                Description = "Remote tasks celery routing. Not used in phoenix."
            };
            ImageResizerConfiguration = new ConfigurationManager.ImageResizerConfiguration("image_resizer_configuration")
            {
                ShouldAllowEmpty = true,
                Description = "Configuration for image resizer handler in remote tasks."
            };
            GraceNoteXSLTPath = new ConfigurationManager.StringConfigurationValue("GraceNote_XSLT_PATH")
            {
                ShouldAllowEmpty = true,
                Description = "Remote tasks configuration for EPG XDTV Transformation."
            };
            GraceNoteALUIdConvertion = new StringConfigurationValue("GraceNote_ALU_IDConvertion")
            {
                ShouldAllowEmpty = true,
                Description = "Remote tasks configuration for EPG XDTV Transformation."
            };
            ElasticSearchHandlerConfiguration = new ConfigurationManager.ElasticSearchHandlerConfiguration("elasticsearch_handler_configuration")
            {
                ShouldAllowEmpty = true
            };
            ShouldDistributeRecordingSynchronously = new ConfigurationManager.BooleanConfigurationValue("ShouldDistributeRecordingSynchronously")
            {
                ShouldAllowEmpty = true,
                DefaultValue = false
            };

            PictureUploaderPath = new ConfigurationManager.StringConfigurationValue("pic_uploader_path")
            {
                ShouldAllowEmpty = true,
                Description = "Configuration for DBManipulator/CouchBaseManipulator in TVM."
            };
            TVMBaseUrl = new StringConfigurationValue("BASE_URL")
            {
                ShouldAllowEmpty = true,
                DefaultValue = "/",
                Description = "Base URL for TVM."
            };
            TVMSkipLoginIPCheck = new ConfigurationManager.BooleanConfigurationValue("SKIP_LOGIN_IP_CHECK")
            {
                ShouldAllowEmpty = true,
                DefaultValue = false,
                Description = "TVM key, whether IP check during login should be skipped or not."
            };
            ClearCachePath = new ConfigurationManager.StringConfigurationValue("CLEAR_CACHE_PATH")
            {
                ShouldAllowEmpty = true,
                Description = "TVM key, location of clean_cache.aspx page in different servers."
            };
            StagingClearCachePath = new ConfigurationManager.StringConfigurationValue("STAGING_CLEAR_CACHE_PATH")
            {
                ShouldAllowEmpty = true,
                Description = "TVM key, location of clean_cache.aspx page in different servers, for staging environment."
            };
            BatchUpload = new ConfigurationManager.StringConfigurationValue("batch_upload")
            {
                ShouldAllowEmpty = true
            };
            LookupGenericUpload = new StringConfigurationValue("lookup_generic_upload")
            {
                ShouldAllowEmpty = true
            };
            EnableHttpLogin = new BooleanConfigurationValue("EnableHttpLogin")
            {
                ShouldAllowEmpty = false
            };
            AppState = new StringConfigurationValue("APP_STATE")
            {
                ShouldAllowEmpty = true,
                DefaultValue = "normal"
            };
            ServerName = new StringConfigurationValue("SERVER_NAME")
            {
                ShouldAllowEmpty = true,
                DefaultValue = "TVM_EU"
            };
            ApplicationName = new StringConfigurationValue("APPLICATION_NAME")
            {
                ShouldAllowEmpty = true
            };
            #endregion

            CrowdSourceTimeSpan = new ConfigurationManager.NumericConfigurationValue("CrowdSourceTimeSpan")
            {
                ShouldAllowEmpty = true,
                DefaultValue = 30
            };
            DMSUrl = new StringConfigurationValue("dms_url");
            MailerConfiguration = new MailerConfiguration("MC");
            GroupsManagerConfiguration = new GroupsManagerConfiguration("groups_manager");
            RequestParserConfiguration = new RequestParserConfiguration("request_parser");
            OTTUserControllerConfiguration = new OTTUserControllerConfiguration("ott_user_controller");
            CouchbaseSectionMapping = new CouchbaseSectionMapping("CouchbaseSectionMapping");
            UsersCacheConfiguration = new ConfigurationManager.UsersCacheConfiguration("users_cache_configuration");
            BaseCacheConfiguration = new NamedCacheConfiguration("base_cache_configuration");
            DatabaseConfiguration = new DatabaseConfiguration("database_configuration");

            // ws cache configuration - reset defaults
            WSCacheConfiguration = new NamedCacheConfiguration("ws_cache_configuration");
            WSCacheConfiguration.TTLSeconds.DefaultValue = 7200;
            WSCacheConfiguration.Name.DefaultValue = "Cache";

            SingleInMemoryCacheName = new ConfigurationManager.StringConfigurationValue("single_in_memory_cache_name")
            {
                DefaultValue = "Cache",
                ShouldAllowEmpty = true
            };

            ODBCWrapperCacheConfiguration = new ConfigurationManager.NamedCacheConfiguration("odbc_wrapper_cache_configuration");
            ODBCWrapperCacheConfiguration.TTLSeconds.DefaultValue = 7200;
            ODBCWrapperCacheConfiguration.Name.DefaultValue = "Cache";

            CatalogCacheConfiguration = new ConfigurationManager.NamedCacheConfiguration("catalog_cache_configuration");
            CatalogCacheConfiguration.TTLSeconds.DefaultValue = 3600;
            CatalogCacheConfiguration.Name.DefaultValue = "CatalogCache";

            NotificationCacheConfiguration = new ConfigurationManager.NamedCacheConfiguration("notification_cache_configuration");
            NotificationCacheConfiguration.TTLSeconds.DefaultValue = 3600;
            NotificationCacheConfiguration.Name.DefaultValue = "NotificationCache";

            GroupsCacheConfiguration = new ConfigurationManager.NamedCacheConfiguration("groups_cache_configuration");
            GroupsCacheConfiguration.TTLSeconds.DefaultValue = 86400;
            GroupsCacheConfiguration.Name.DefaultValue = "GroupsCache";
            GroupsCacheConfiguration.Name.Description = "Original key is GROUPS_CACHE_NAME";
            GroupsCacheConfiguration.Type.DefaultValue = "CouchBase";
            GroupsCacheConfiguration.Type.Description = "Original key is GroupsCacheConfiguration";
            GroupsCacheConfiguration.TTLSeconds.Description = "Original key is GroupsCacheDocTimeout";

            CouchBaseDesigns = new CouchBaseDesigns("couchbase_designs");
            EPGDocumentExpiry = new NumericConfigurationValue("epg_doc_expiry")
            {
                DefaultValue = 7
            };
            EutelsatSettings = new EutelsatSettings("eutelsat_settings");
            ElasticSearchConfiguration = new ElasticSearchConfiguration("elasticsearch_settings");
            SearchIndexType = new StringConfigurationValue("search_index_type")
            {
                ShouldAllowEmpty = true,
                DefaultValue = "ES",
                Description = "Used in TVM, for transition between Lucene and ElasticSearch. " +
                "Today we use ES exculisvely. Only valid value is 'ES', otherwise Lucene is used"
            };
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
            SocialCacheDocTimeout = new NumericConfigurationValue("socialCacheDocTimeout")
            {
                DefaultValue = 1440,
                ShouldAllowEmpty = true
            };
            FacebookConfiguration = new FacebookConfiguration("facebook_configuration");
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
            BillingCacheTTL = new ConfigurationManager.NumericConfigurationValue("BillingCacheTTL")
            {
                DefaultValue = 60
            };
            UseOldImageServer = new ConfigurationManager.StringConfigurationValue("USE_OLD_IMAGE_SERVER")
            {
                DefaultValue = "0",
                Description = "Group Ids, split by ';', that wish to use old image server"
            };
            CatalogLogicConfiguration = new CatalogLogicConfiguration("catalog_logic_configuration");
            AnnouncementManagerConfiguration = new ConfigurationManager.AnnouncementManagerConfiguration("announcement_manager_configuration");
            CDVRAdapterConfiguration = new CDVRAdapterConfiguration("cdvr_adapter_configuration")
            {
                ShouldAllowEmpty = true
            };
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
            ReconciliationFrequencySeconds = new ConfigurationManager.NumericConfigurationValue("reconciliation_frequency_seconds")
            {
                DefaultValue = 7200
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
            ImageUtilsConfiguration = new ConfigurationManager.ImageUtilsConfiguration("image_utils_configuration")
            {
                ShouldAllowEmpty = true
            };

            DownloadPicWithQueue = new ConfigurationManager.BooleanConfigurationValue("downloadPicWithQueue")
            {
                DefaultValue = false,
                ShouldAllowEmpty = true
            };
            CheckImageUrl = new ConfigurationManager.BooleanConfigurationValue("CheckImageUrl")
            {
                DefaultValue = true,
                ShouldAllowEmpty = true
            };
            EPGUrl = new ConfigurationManager.StringConfigurationValue("EPGUrl")
            {
                ShouldAllowEmpty = true,
                Description = "Use in yes epg BL"
            };
            GroupIDsWithIPNOFilteringSeperatedBySemiColon = new ConfigurationManager.StringConfigurationValue("GroupIDsWithIPNOFilteringSeperatedBySemiColon")
            {
                ShouldAllowEmpty = true
            };

            EncryptorService = new ConfigurationManager.StringConfigurationValue("EncryptorService") { ShouldAllowEmpty = true };
            EncryptorPassword = new ConfigurationManager.StringConfigurationValue("EncryptorPassword") { ShouldAllowEmpty = true };
            PicsBasePath = new ConfigurationManager.StringConfigurationValue("pics_base_path") { ShouldAllowEmpty = true };
            NotificationConfiguration = new NotificationConfiguration("notification_configuration");
            IngestFtpPass = new StringConfigurationValue("IngestFtpPass") { ShouldAllowEmpty = true };
            IngestFtpUrl = new StringConfigurationValue("IngestFtpUrl") { ShouldAllowEmpty = true };
            IngestFtpUser = new StringConfigurationValue("IngestFtpUser") { ShouldAllowEmpty = true };

            AllConfigurationValues = new List<ConfigurationValue>()
                {
                    CeleryRoutingConfiguration,
                    ImageResizerConfiguration,
                    GraceNoteALUIdConvertion,
                    GraceNoteXSLTPath,
                    ElasticSearchHandlerConfiguration,
                    ShouldDistributeRecordingSynchronously,
                    PictureUploaderPath,
                    TVMBaseUrl,
                    TVMSkipLoginIPCheck,
                    ClearCachePath,
                    StagingClearCachePath,
                    BatchUpload,
                    LookupGenericUpload,
                    EnableHttpLogin,
                    AppState,
                    ServerName,
                    ApplicationName,
                    DMSUrl,
                    MailerConfiguration,
                    GroupsManagerConfiguration,
                    RequestParserConfiguration,
                    OTTUserControllerConfiguration,
                    CouchbaseSectionMapping,
                    UsersCacheConfiguration,
                    BaseCacheConfiguration,
                    DatabaseConfiguration,
                    WSCacheConfiguration,
                    CouchBaseDesigns,
                    EPGDocumentExpiry,
                    EutelsatSettings,
                    ElasticSearchConfiguration,
                    SearchIndexType,
                    CatalogSignatureKey,
                    HarmonicProviderConfiguration,
                    RabbitConfiguration,
                    RoleIdsConfiguration,
                    ExcludePsDllImplementation,
                    DomainCacheDocTimeout,
                    SocialCacheDocTimeout,
                    FacebookConfiguration,
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
                    BillingCacheTTL,
                    UseOldImageServer,
                    CatalogLogicConfiguration,
                    AnnouncementManagerConfiguration,
                    CDVRAdapterConfiguration,
                    PersonalizedFeedTTLDays,
                    EngagementsConfiguration,
                    UserInterestsTTLDays,
                    PlayManifestDynamicQueryStringParamsNames,
                    RecordingsMaxDegreeOfParallelism,
                    ReconciliationFrequencySeconds,
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
                    ImageUtilsConfiguration,
                    DownloadPicWithQueue,
                    CheckImageUrl,
                    EPGUrl,
                    SingleInMemoryCacheName,
                    ODBCWrapperCacheConfiguration,
                    CatalogCacheConfiguration,
                    NotificationCacheConfiguration,
                    GroupsCacheConfiguration,
                    GroupIDsWithIPNOFilteringSeperatedBySemiColon,
                    EncryptorService,
                    EncryptorPassword,
                    PicsBasePath,
                    NotificationConfiguration,
                    IngestFtpPass,
                    IngestFtpUrl,
                    IngestFtpUser
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