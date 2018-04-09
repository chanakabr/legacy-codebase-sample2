using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        public static ProfessionalServicesTasksConfiguration ProfessionalServicesTasksConfiguration;

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
        public static BaseCacheConfiguration BaseCacheConfiguration;
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
        public static NumericConfigurationValue EpgImagePendingThresholdInMinutes;
        public static NumericConfigurationValue EpgImageActiveThresholdInMinutes;
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
        public static StringConfigurationValue AdyenWSUser;
        public static StringConfigurationValue AdyenWSPass;
        public static StringConfigurationValue AdyenWSMerchAccount;
        public static StringConfigurationValue AdyenPSPReferenceRegexOverride;
        public static NumericConfigurationValue PwwawpMaxResultsSize;
        public static NumericConfigurationValue PwlalPMaxResultsSize;
        public static NumericConfigurationValue PreviewModuleNumOfCancelOrRefundAttempts;


        #endregion

        #region Private Members

        private static List<ConfigurationValue> allConfigurationValues;
        private static string logPath = string.Empty;
        private static StringBuilder logBuilder;
        private static List<ConfigurationValue> configurationValuesWithOriginalKeys;
        #endregion

        #region Public Static Methods

        public static void Initialize(bool shouldLoadDefaults = false, string application = "", string host = "", string environment = "")
        {
            if (!string.IsNullOrEmpty(application) || !string.IsNullOrEmpty(host) || !string.IsNullOrEmpty(environment))
            {
                TCMClient.TCMConfiguration config = (TCMClient.TCMConfiguration)System.Configuration.ConfigurationManager.GetSection("TCMConfig");

                if (string.IsNullOrEmpty(application))
                {
                    application = config.Application;
                }

                if (string.IsNullOrEmpty(host))
                {
                    host = config.Host;
                }

                if (string.IsNullOrEmpty(environment))
                {
                    environment = config.Environment;
                }

                //Populate settings from remote
                TCMClient.Settings.Instance.Init(config.URL, application, host, environment, config.AppID, config.AppSecret);
            }
            else
            {
                TCMClient.Settings.Instance.Init();
            }

            #region Remote tasks configuration values

            CeleryRoutingConfiguration = new CeleryRoutingConfiguration("CELERY_ROUTING")
            {
                ShouldAllowEmpty = true,
                Description = "Remote tasks celery routing. Not used in phoenix."
            };
            ImageResizerConfiguration = new ImageResizerConfiguration("image_resizer_configuration")
            {
                ShouldAllowEmpty = true,
                Description = "Configuration for image resizer handler in remote tasks."
            };
            GraceNoteXSLTPath = new StringConfigurationValue("GraceNote_XSLT_PATH")
            {
                ShouldAllowEmpty = true,
                Description = "Remote tasks configuration for EPG XDTV Transformation."
            };
            GraceNoteALUIdConvertion = new StringConfigurationValue("GraceNote_ALU_IDConvertion")
            {
                ShouldAllowEmpty = true,
                Description = "Remote tasks configuration for EPG XDTV Transformation."
            };
            ElasticSearchHandlerConfiguration = new ElasticSearchHandlerConfiguration("elasticsearch_handler_configuration")
            {
                ShouldAllowEmpty = true
            };
            ShouldDistributeRecordingSynchronously = new BooleanConfigurationValue("ShouldDistributeRecordingSynchronously")
            {
                ShouldAllowEmpty = true,
                DefaultValue = false
            };
            ProfessionalServicesTasksConfiguration = new ProfessionalServicesTasksConfiguration("professional_services_tasks")
            {
                ShouldAllowEmpty = true,
                Description = "Remote tasks configuratin for professional services handler."
            };

            PictureUploaderPath = new StringConfigurationValue("pic_uploader_path")
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
            TVMSkipLoginIPCheck = new BooleanConfigurationValue("SKIP_LOGIN_IP_CHECK")
            {
                ShouldAllowEmpty = true,
                DefaultValue = false,
                Description = "TVM key, whether IP check during login should be skipped or not."
            };
            ClearCachePath = new StringConfigurationValue("CLEAR_CACHE_PATH")
            {
                ShouldAllowEmpty = true,
                Description = "TVM key, location of clean_cache.aspx page in different servers."
            };
            StagingClearCachePath = new StringConfigurationValue("STAGING_CLEAR_CACHE_PATH")
            {
                ShouldAllowEmpty = true,
                Description = "TVM key, location of clean_cache.aspx page in different servers, for staging environment."
            };
            BatchUpload = new StringConfigurationValue("batch_upload")
            {
                ShouldAllowEmpty = true
            };
            LookupGenericUpload = new StringConfigurationValue("lookup_generic_upload")
            {
                ShouldAllowEmpty = true
            };
            EnableHttpLogin = new BooleanConfigurationValue("EnableHttpLogin")
            {
                ShouldAllowEmpty = true,
                DefaultValue = true
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

            CrowdSourceTimeSpan = new NumericConfigurationValue("CrowdSourceTimeSpan")
            {
                ShouldAllowEmpty = true,
                DefaultValue = 30
            };
            DMSUrl = new StringConfigurationValue("dms_url")
            {
                Description = "Address of DMS server."
            };
            MailerConfiguration = new MailerConfiguration("MC");
            GroupsManagerConfiguration = new GroupsManagerConfiguration("groups_manager");
            RequestParserConfiguration = new RequestParserConfiguration("request_parser");
            OTTUserControllerConfiguration = new OTTUserControllerConfiguration("ott_user_controller");
            CouchbaseSectionMapping = new CouchbaseSectionMapping("CouchbaseSectionMapping");
            UsersCacheConfiguration = new UsersCacheConfiguration("users_cache_configuration");
            BaseCacheConfiguration = new BaseCacheConfiguration("base_cache_configuration");
            BaseCacheConfiguration.TTLSeconds.OriginalKey = "Groups_Cache_TTL";
            BaseCacheConfiguration.Type = null;

            DatabaseConfiguration = new DatabaseConfiguration("database_configuration");

            // ws cache configuration - reset defaults
            WSCacheConfiguration = new NamedCacheConfiguration("ws_cache_configuration");
            WSCacheConfiguration.TTLSeconds.DefaultValue = 7200;
            WSCacheConfiguration.TTLSeconds.OriginalKey = "CACHE_TIME_IN_MINUTES";
            WSCacheConfiguration.Name.DefaultValue = "Cache";
            WSCacheConfiguration.Name.OriginalKey = "CACHE_NAME";
            WSCacheConfiguration.Type.OriginalKey = "CACHE_TYPE";

            SingleInMemoryCacheName = new StringConfigurationValue("single_in_memory_cache_name")
            {
                DefaultValue = "Cache",
                ShouldAllowEmpty = true,
                OriginalKey = "CACHE_NAME"
            };

            ODBCWrapperCacheConfiguration = new NamedCacheConfiguration("odbc_wrapper_cache_configuration");
            ODBCWrapperCacheConfiguration.TTLSeconds.DefaultValue = 7200;
            ODBCWrapperCacheConfiguration.TTLSeconds.OriginalKey = "CACHE_TIME_IN_MINUTES";
            ODBCWrapperCacheConfiguration.Name.DefaultValue = "Cache";
            ODBCWrapperCacheConfiguration.Name.OriginalKey = "CACHE_NAME";
            ODBCWrapperCacheConfiguration.Type.OriginalKey = "CACHE_TYPE";

            CatalogCacheConfiguration = new NamedCacheConfiguration("catalog_cache_configuration");
            CatalogCacheConfiguration.TTLSeconds.DefaultValue = 3600;
            CatalogCacheConfiguration.TTLSeconds.OriginalKey = "CACHE_TIME_IN_MINUTES";
            CatalogCacheConfiguration.Name.DefaultValue = "CatalogCache";
            CatalogCacheConfiguration.Name.OriginalKey = "CACHE_NAME";
            CatalogCacheConfiguration.Type.OriginalKey = "CACHE_TYPE";

            NotificationCacheConfiguration = new NamedCacheConfiguration("notification_cache_configuration");
            NotificationCacheConfiguration.TTLSeconds.DefaultValue = 3600;
            NotificationCacheConfiguration.Name.DefaultValue = "NotificationCache";
            NotificationCacheConfiguration.TTLSeconds.OriginalKey = "CACHE_TIME_IN_MINUTES";
            NotificationCacheConfiguration.Name.OriginalKey = "CACHE_NAME";
            // type is always inner cache in this case
            NotificationCacheConfiguration.Type = null;

            GroupsCacheConfiguration = new NamedCacheConfiguration("groups_cache_configuration");
            GroupsCacheConfiguration.TTLSeconds.DefaultValue = 86400;
            GroupsCacheConfiguration.Name.DefaultValue = "GroupsCache";
            GroupsCacheConfiguration.Name.OriginalKey = "GROUPS_CACHE_NAME";
            GroupsCacheConfiguration.Type.DefaultValue = "CouchBase";
            GroupsCacheConfiguration.Type.OriginalKey = "GroupsCacheConfiguration";
            GroupsCacheConfiguration.TTLSeconds.OriginalKey = "GroupsCacheDocTimeout";

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
            LayeredCacheConfigurationValidation = new LayeredCacheConfigurationValidation("LayeredCache");
            ExportConfiguration = new ExportConfiguration("export");
            UDRMUrl = new StringConfigurationValue("UDRM_URL")
            {
                DefaultValue = "https://ny-udrm-stg.kaltura.com"
            };
            BillingCacheTTL = new ConfigurationManager.NumericConfigurationValue("BillingCacheTTL")
            {
                DefaultValue = 60
            };
            UseOldImageServer = new StringConfigurationValue("USE_OLD_IMAGE_SERVER")
            {
                DefaultValue = "0",
                Description = "Group Ids, split by ';', that wish to use old image server"
            };
            CatalogLogicConfiguration = new CatalogLogicConfiguration("catalog_logic_configuration");
            AnnouncementManagerConfiguration = new AnnouncementManagerConfiguration("announcement_manager_configuration");
            CDVRAdapterConfiguration = new CDVRAdapterConfiguration("cdvr_adapter_configuration")
            {
                ShouldAllowEmpty = true
            };
            DMSAdapterUrl = new StringConfigurationValue("DMS_ADAPTER_URL")
            {

            };
            PersonalizedFeedTTLDays = new NumericConfigurationValue("PersonalizedFeedTTLDays")
            {
                DefaultValue = 365
            };
            EngagementsConfiguration = new EngagementsConfiguration("engagements_configuration");
            UserInterestsTTLDays = new NumericConfigurationValue("ttl_user_interest_days")
            {
                DefaultValue = 30
            };
            PlayManifestDynamicQueryStringParamsNames = new StringConfigurationValue("PlayManifestDynamicQueryStringParamsNames")
            {
                DefaultValue = "clientTag,playSessionId"
            };
            RecordingsMaxDegreeOfParallelism = new NumericConfigurationValue("recordings_max_degree_of_parallelism")
            {
                OriginalKey = "MaxDegreeOfParallelism",
                DefaultValue = 5
            };
            ReconciliationFrequencySeconds = new NumericConfigurationValue("reconciliation_frequency_seconds")
            {
                DefaultValue = 7200
            };
            EventConsumersConfiguration = new EventConsumersConfiguration("ConsumerSettings");
            AuthorizationManagerConfiguration = new AuthorizationManagerConfiguration("authorization_manager_configuration");
            UserPINDigitsConfiguration = new UserPINDigitsConfiguration("user_pin_digits_configuration");
            WebServicesConfiguration = new WebServicesConfiguration("WebServices");
            ShouldGetCatalogDataFromDB = new BooleanConfigurationValue("get_catalog_data_from_db")
            {
                DefaultValue = false,
                ShouldAllowEmpty = true,
                Description = "Just in case media mark information is not in Couchbase, we might want to continue to DB. Should be false or empty."
            };
            CrowdSourcerFeedNumberOfItems = new NumericConfigurationValue("crowdsourcer.FEED_NUM_OF_ITEMS")
            {
                DefaultValue = 0,
                ShouldAllowEmpty = true
            };
            PushMessagesConfiguration = new PushMessagesConfiguration("push_messages");
            QueueFailLimit = new NumericConfigurationValue("queue_fail_limit")
            {
                DefaultValue = 3,
                ShouldAllowEmpty = true,
                Description = "Retry limit for RabbitMQ actions like enqueue."
            };
            Version = new StringConfigurationValue("Version")
            {
                Description = "CouchBase document prefix. Each version has its own cached document to avoid backward compatibilty issues."
            };
            PendingThresholdDays = new NumericConfigurationValue("pending_threshold_days")
            {
                DefaultValue = 180,
                ShouldAllowEmpty = true
            };
            ImageUtilsConfiguration = new ImageUtilsConfiguration("image_utils_configuration")
            {
                ShouldAllowEmpty = true
            };

            DownloadPicWithQueue = new BooleanConfigurationValue("downloadPicWithQueue")
            {
                DefaultValue = false,
                ShouldAllowEmpty = true
            };
            CheckImageUrl = new BooleanConfigurationValue("CheckImageUrl")
            {
                DefaultValue = true,
                ShouldAllowEmpty = true
            };
            EpgImagePendingThresholdInMinutes = new NumericConfigurationValue("epgImagePendingThresholdInMinutes")
            {
                ShouldAllowEmpty = true,
                DefaultValue = 120
            };

            EpgImageActiveThresholdInMinutes = new NumericConfigurationValue("epgImageActiveThresholdInMinutes")
            {
                ShouldAllowEmpty = true,
                DefaultValue = 43200
            };
            EPGUrl = new StringConfigurationValue("EPGUrl")
            {
                ShouldAllowEmpty = true,
                Description = "Use in yes epg BL"
            };
            GroupIDsWithIPNOFilteringSeperatedBySemiColon = new StringConfigurationValue("GroupIDsWithIPNOFilteringSeperatedBySemiColon")
            {
                ShouldAllowEmpty = true
            };

            EncryptorService = new StringConfigurationValue("EncryptorService") { ShouldAllowEmpty = true };
            EncryptorPassword = new StringConfigurationValue("EncryptorPassword") { ShouldAllowEmpty = true };
            PicsBasePath = new StringConfigurationValue("pics_base_path") { ShouldAllowEmpty = true };
            NotificationConfiguration = new NotificationConfiguration("notification_configuration");
            IngestFtpPass = new StringConfigurationValue("IngestFtpPass") { ShouldAllowEmpty = true };
            IngestFtpUrl = new StringConfigurationValue("IngestFtpUrl") { ShouldAllowEmpty = true };
            IngestFtpUser = new StringConfigurationValue("IngestFtpUser") { ShouldAllowEmpty = true };
            AdyenWSUser = new StringConfigurationValue("TvinciAdyenWS_User") { ShouldAllowEmpty = true };
            AdyenWSPass = new StringConfigurationValue("TvinciAdyenWS_Pass") { ShouldAllowEmpty = true };
            AdyenWSMerchAccount = new StringConfigurationValue("TvinciAdyenWS_MerchAccount") { ShouldAllowEmpty = true };
            AdyenPSPReferenceRegexOverride = new StringConfigurationValue("AdyenPSPReferenceRegexOverride") { ShouldAllowEmpty = true };
            PwwawpMaxResultsSize = new NumericConfigurationValue("PWWAWP_MAX_RESULTS_SIZE")
            {
                ShouldAllowEmpty = true,
                DefaultValue = 8
            };
            PwlalPMaxResultsSize = new NumericConfigurationValue("PWLALP_MAX_RESULTS_SIZE")
            {
                ShouldAllowEmpty = true,
                DefaultValue = 8
            };
            PreviewModuleNumOfCancelOrRefundAttempts = new NumericConfigurationValue("PreviewModuleNumOfCancelOrRefundAttempts")
            {
                ShouldAllowEmpty = true,
                DefaultValue = 4
            };

            allConfigurationValues = new List<ConfigurationValue>()
                {
                    CeleryRoutingConfiguration,
                    ImageResizerConfiguration,
                    GraceNoteALUIdConvertion,
                    GraceNoteXSLTPath,
                    ElasticSearchHandlerConfiguration,
                    ShouldDistributeRecordingSynchronously,
                    ProfessionalServicesTasksConfiguration,
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
                    EpgImagePendingThresholdInMinutes,
                    EpgImageActiveThresholdInMinutes,
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
                    IngestFtpUser,
                    AdyenWSUser,
                    AdyenWSPass,
                    AdyenWSMerchAccount,
                    AdyenPSPReferenceRegexOverride,
                    PwwawpMaxResultsSize,
                    PwlalPMaxResultsSize,
                    PreviewModuleNumOfCancelOrRefundAttempts
                };

            configurationValuesWithOriginalKeys = new List<ConfigurationManager.ConfigurationValue>();

            if (shouldLoadDefaults)
            {
                foreach (var configurationValue in allConfigurationValues)
                {
                    configurationValue.LoadDefault();
                }
            }
        }

        public static bool Validate(string application = "", string host = "", string environment = "", string logFilePath = "")
        {
            bool result = true;
            logPath = logFilePath;

            try
            {
                if (!string.IsNullOrEmpty(logPath))
                {
                    // create output directory
                    string directory = Path.GetDirectoryName(logPath);

                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }

                logBuilder = new StringBuilder();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error when creating directory for log file. ex = {0}", ex));
            }

            try
            {
                Initialize(false, application, host, environment);

                foreach (var configurationValue in allConfigurationValues)
                {
                    try
                    {
                        result &= configurationValue.Validate();
                    }
                    catch (Exception ex)
                    {
                        WriteToLog(string.Format("Exception when validating: {0}", ex.Message));
                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog(string.Format("Exception when validating: {0}", ex.Message));
                result = false;
            }

            WriteToLog(string.Format("Finished validating TCM and result is {0}", result));

            try
            {
                if (!string.IsNullOrEmpty(logPath))
                {
                    File.AppendAllText(logPath, logBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error when creating file for log file. ex = {0}", ex));
            }

            return result;
        }

        public static void Migrate(string migrationFilePath)
        {
            if (!string.IsNullOrEmpty(migrationFilePath) && configurationValuesWithOriginalKeys != null)
            {
                StringBuilder builder = new StringBuilder();

                JObject json = new JObject();

                foreach (var configurationValue in configurationValuesWithOriginalKeys)
                {
                    string fullKey = configurationValue.GetFullKey();

                    string[] keyPath = fullKey.Split('.');

                    if (keyPath.Length > 0)
                    {
                        JToken currentJson = json;

                        for (int i = 0; i < keyPath.Length - 1; i++)
                        {
                            string key = keyPath[i];

                            if (currentJson[key] == null)
                            {
                                currentJson[key] = new JObject();
                            }

                            currentJson = currentJson[key];
                        }

                        object originalValue = TCMClient.Settings.Instance.GetValue<object>(configurationValue.OriginalKey);

                        string currentJsonKey = keyPath[keyPath.Length - 1];

                        // default behvaior - take the original value from TCM and put it in JSON
                        currentJson[currentJsonKey] = new JValue(originalValue);

                        // If the original value is empty, but configuration validation forces its value to be not empty, then we will use the defined default value
                        if ((originalValue == null || string.IsNullOrEmpty(Convert.ToString(originalValue))) && !configurationValue.ShouldAllowEmpty)
                        {
                            currentJson[currentJsonKey] = new JValue(configurationValue.DefaultValue);
                        }
                    }
                }

                try
                {
                    // create output directory
                    string directory = Path.GetDirectoryName(migrationFilePath);

                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    File.AppendAllText(migrationFilePath, json.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Error when creating migration file. ex = {0}", ex));
                }
            }
        }

        #endregion

        #region Internal or Private Static Methods

        internal static void WriteToLog(string log)
        {
            if (string.IsNullOrEmpty(logPath))
            {
                Console.WriteLine(log);
            }
            else
            {
                logBuilder.AppendLine(log);
            }
        }

        internal static void AddConfigurationValueWithOrigin(ConfigurationValue configurationValue)
        {
            configurationValuesWithOriginalKeys.Add(configurationValue);
        }

        #endregion
    }
}