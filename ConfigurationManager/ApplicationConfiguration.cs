using System;
using System.Collections.Generic;
using System.Text;
using ConfigurationManager.Types;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class ApplicationConfiguration : BaseConfig<ApplicationConfiguration>
    {
        public override string TcmKey => null;
        public override string[] TcmPath => null;
        

        static ApplicationConfiguration()
        {
        }

        private ApplicationConfiguration()
        {
        }

        public static ApplicationConfiguration Current { get; } = new ApplicationConfiguration();

        public RabbitConfiguration RabbitConfiguration = new RabbitConfiguration();
        public EutelsatSettings EutelsatSettings = new EutelsatSettings();
        public ProfessionalServicesTasksConfiguration ProfessionalServicesTasksConfiguration= new ProfessionalServicesTasksConfiguration();
        public WSCacheConfiguration WSCacheConfiguration = new WSCacheConfiguration();

        public TVPApiConfiguration TVPApiConfiguration = new TVPApiConfiguration();
        public BaseCacheConfiguration BaseCacheConfiguration = new BaseCacheConfiguration();
        public ODBCWrapperCacheConfiguration ODBCWrapperCacheConfiguration = new ODBCWrapperCacheConfiguration();
        public CatalogCacheConfiguration CatalogCacheConfiguration = new CatalogCacheConfiguration();
        public NotificationCacheConfiguration NotificationCacheConfiguration = new NotificationCacheConfiguration();
        public GroupsCacheConfiguration GroupsCacheConfiguration = new GroupsCacheConfiguration();
        public LicensedLinksCacheConfiguration LicensedLinksCacheConfiguration = new LicensedLinksCacheConfiguration();


        public CouchBaseDesigns CouchBaseDesigns = new CouchBaseDesigns();
        public CouchbaseSectionMapping CouchbaseSectionMapping = new CouchbaseSectionMapping();

        public BaseValue<string> ExcludeTemplatesImplementation = new BaseValue<string>("EXCLUDE_TEMPLATES_IMPLEMENTATION", "203");
        public BaseValue<string> UDRMUrl = new BaseValue<string>("UDRM_URL", "https://ny-udrm-stg.kaltura.com");
        public BaseValue<string> UseOldImageServer = new BaseValue<string>("USE_OLD_IMAGE_SERVER", "0", true, "Group Ids, split by ';', that wish to use old image server");
        public BaseValue<string> DMSAdapterUrl = new BaseValue<string>("DMS_ADAPTER_URL", null);
        public BaseValue<string> PlayManifestDynamicQueryStringParamsNames = new BaseValue<string>("PlayManifestDynamicQueryStringParamsNames", "clientTag,playSessionId");
        public BaseValue<string> Version = new BaseValue<string>("Version", null,true, "CouchBase document prefix. Each version has its own cached document to avoid backward compatibilty issues.");
        public BaseValue<string> PictureUploaderPath = new BaseValue<string>("pic_uploader_path", null, true, "Configuration for DBManipulator/CouchBaseManipulator in TVM.");
        public BaseValue<string> TVMBaseUrl = new BaseValue<string>("BASE_URL", "/", true, "Base URL for TVM.");
        public BaseValue<string> ClearCachePath = new BaseValue<string>("CLEAR_CACHE_PATH", null, true, "TVM key, location of clean_cache.aspx page in different servers.");
        public BaseValue<string> StagingClearCachePath = new BaseValue<string>("STAGING_CLEAR_CACHE_PATH", null, true, "TVM key, location of clean_cache.aspx page in different servers, for staging environment.");
        public BaseValue<string> BatchUpload = new BaseValue<string>("batch_upload", null);
        public BaseValue<string> LookupGenericUpload = new BaseValue<string>("lookup_generic_upload", null);
        public BaseValue<string> AppState = new BaseValue<string>("APP_STATE", "normal");
        public BaseValue<string> ServerName = new BaseValue<string>("SERVER_NAME", "TVM_EU");
        public BaseValue<string> ApplicationName = new BaseValue<string>("APPLICATION_NAME", null);
        public BaseValue<string> GraceNoteXSLTPath = new BaseValue<string>("GraceNote_XSLT_PATH", null, true, "Remote tasks configuration for EPG XDTV Transformation.");
        public BaseValue<string> GraceNoteALUIdConvertion = new BaseValue<string>("GraceNote_ALU_IDConvertion", null, true, "Remote tasks configuration for EPG XDTV Transformation.");
        public BaseValue<string> DMSUrl = new BaseValue<string>("DMSUrl", null, false, "Address of DMS server.");
        public BaseValue<string> CatalogSignatureKey = new BaseValue<string>("CatalogSignatureKey", null, false, "liat regev");
        public BaseValue<string> SingleInMemoryCacheName = new BaseValue<string>("single_in_memory_cache_name", "Cache", true, null);
        public BaseValue<string> ExcludePsDllImplementation = new BaseValue<string>("EXCLUDE_PS_DLL_IMPLEMENTATION", null, true, null);
        public BaseValue<string> UsersAssemblyLocation = new BaseValue<string>("USERS_ASSEMBLY_LOCATION", null, true, null);
        public BaseValue<string> FriendsActivityViewStaleState = new BaseValue<string>("FRIENDS_ACTIVITY_VIEW_STALE_STATE", "None", true, "Corresponding to ViewStaleState enum. Possible values: None, False, Ok, UpdateAfter");
        public BaseValue<string> EPGUrl = new BaseValue<string>("EPGUrl", null, true, "Use in yes epg BL");
        public BaseValue<string> GroupIDsWithIPNOFilteringSeperatedBySemiColon = new BaseValue<string>("GroupIDsWithIPNOFilteringSeperatedBySemiColon", null, true, null);
        public BaseValue<string> EncryptorService = new BaseValue<string>("EncryptorService", null, true, null);
        public BaseValue<string> EncryptorPassword = new BaseValue<string>("EncryptorPassword", null, true, null);
        public BaseValue<string> PicsBasePath = new BaseValue<string>("pics_base_path", null, true, null);
        public BaseValue<string> IngestFtpPass = new BaseValue<string>("IngestFtpPass", null, true, null);
        public BaseValue<string> IngestFtpUrl = new BaseValue<string>("IngestFtpUrl", null, true, null);
        public BaseValue<string> IngestFtpUser = new BaseValue<string>("IngestFtpUser", null, true, null);
        public BaseValue<string> AdyenWSUser = new BaseValue<string>("TvinciAdyenWS_User", null, true, null);
        public BaseValue<string> AdyenWSPass = new BaseValue<string>("TvinciAdyenWS_Pass", null, true, null);
        public BaseValue<string> AdyenWSMerchAccount = new BaseValue<string>("TvinciAdyenWS_MerchAccount", null, true, null);
        public BaseValue<string> AdyenPSPReferenceRegexOverride = new BaseValue<string>("AdyenPSPReferenceRegexOverride", null, true, null);
        public BaseValue<string> MetaFeaturesPattern = new BaseValue<string>("meta_features_pattern", @"\W|[^ ]{64}[^ ]", true, null);
        public BaseValue<string> LogConfigurationDocumentKey = new BaseValue<string>("log_configuration_document_key", "phoenix_log_configuration", true, "Document key in Couchbase from which the log reloader mechanism will read the configuration of log4net.config");
        public BaseValue<string> SearchIndexType = new BaseValue<string>("search_index_type", "ES", true, "Used in TVM, for transition between Lucene and ElasticSearch. " +
        "Today we use ES exclusively. Only valid value is 'ES', otherwise Lucene is used");


        public BaseValue<int> EpgImagePendingThresholdInMinutes = new BaseValue<int>("epgImagePendingThresholdInMinutes", 120);
        public BaseValue<int> EpgImageActiveThresholdInMinutes = new BaseValue<int>("epgImageActiveThresholdInMinutes", 43200);
        public BaseValue<int> PwwawpMaxResultsSize = new BaseValue<int>("PWWAWP_MAX_RESULTS_SIZE", 8);
        public BaseValue<int> PwlalPMaxResultsSize = new BaseValue<int>("PWLALP_MAX_RESULTS_SIZE", 8);
        public BaseValue<int> PreviewModuleNumOfCancelOrRefundAttempts = new BaseValue<int>("PreviewModuleNumOfCancelOrRefundAttempts", 120);
        public BaseValue<int> EPGDocumentExpiry = new BaseValue<int>("epg_doc_expiry", 7);
        public BaseValue<int> DomainCacheDocTimeout = new BaseValue<int>("DomainCacheDocTimeout", 1440);
        public BaseValue<int> PlayCycleDocumentExpiryMinutes = new BaseValue<int>("playCycle_doc_expiry_min", 60, true, "TTL for CouchBase documents of play cycle data in minutes.");
        public BaseValue<int> UserInterestsTTLDays = new BaseValue<int>("ttl_user_interest_days", 30);
        public BaseValue<int> PersonalizedFeedTTLDays = new BaseValue<int>("PersonalizedFeedTTLDays", 365);
        public BaseValue<int> RecordingsMaxDegreeOfParallelism = new BaseValue<int>("recordings_max_degree_of_parallelism", 5);
        public BaseValue<int> QueueFailLimit = new BaseValue<int>("queue_fail_limit", 3, true, "Retry limit for RabbitMQ actions like enqueue.");
        public BaseValue<int> PendingThresholdDays = new BaseValue<int>("pending_threshold_days", 180);
        public BaseValue<int> CrowdSourcerFeedNumberOfItems = new BaseValue<int>("crowdsourcer.FEED_NUM_OF_ITEMS", 0);
        public BaseValue<int> UserSegmentTTL = new BaseValue<int>("user_segment_ttl_hours", 36, true, "How long do we keep information about the users' segments");
        public BaseValue<int> EPGDeleteBulkSize = new BaseValue<int>("epg_delete_bulk_size", 10);
        public BaseValue<int> MediaMarksListLength = new BaseValue<int>("media_marks_list_limit", 300);
        public BaseValue<int> MediaMarksTTL = new BaseValue<int>("media_marks_ttl_days", 90);
        public BaseValue<int> LogReloadInterval = new BaseValue<int>("log_reload_interval", 0, true, "Interval of reloading the KLogger configuration from Couchbase, in milliseconds.");

        public BaseValue<double> CrowdSourceTimeSpan = new BaseValue<double>("CrowdSourceTimeSpan", 30);
        public BaseValue<double> BillingCacheTTL = new BaseValue<double>("BillingCacheTTL", 60);
        public BaseValue<double> SocialCacheDocTimeout = new BaseValue<double>("socialCacheDocTimeout", 1440);
        
        public BaseValue<long> ReconciliationFrequencySeconds = new BaseValue<long>("reconciliation_frequency_seconds", 7200);
        public BaseValue<ulong> EpgInitialId = new BaseValue<ulong>("epg_initial_id", 100000000);

        public BaseValue<bool> ShouldDistributeRecordingSynchronously = new BaseValue<bool>("elasticsearch_handler_configuration", true, true, null);
        public BaseValue<bool> ShouldSupportCeleryMessages = new BaseValue<bool>("should_support_celery_messages", true, true, null);
        public BaseValue<bool> ShouldSupportEventBusMessages = new BaseValue<bool>("should_support_event_bus_messages", false, true, null);
        public BaseValue<bool> ShouldRecoverSubscriptionRenewalToMessageBus = new BaseValue<bool>("should_recover_subscription_renewal_to_message_bus", false, true, null);
        public BaseValue<bool> ShouldGetMediaFileDetailsDirectly = new BaseValue<bool>("ShouldGetMediaFileDetailsDirectly", false, false, "description");
        public BaseValue<bool> ShouldGetCatalogDataFromDB = new BaseValue<bool>("get_catalog_data_from_db",false, true, "Just in case media mark information is not in Couchbase, we might want to continue to DB. Should be false or empty.");
        public BaseValue<bool> DownloadPicWithQueue = new BaseValue<bool>("downloadPicWithQueue", false);
        public BaseValue<bool> CheckImageUrl = new BaseValue<bool>("CheckImageUrl", true);
        public BaseValue<bool> AllowUnknownCountry = new BaseValue<bool>("allow_unknown_country", false);
        public BaseValue<bool> ShouldSubscriptionOverlapConsiderDLM = new BaseValue<bool>("should_subscription_overlap_consider_dlm", false);
        public BaseValue<bool> ShouldAddInvalidationKeysToHeader = new BaseValue<bool>("add_invalidation_keys_to_header", false);
        public BaseValue<bool> TVMSkipLoginIPCheck = new BaseValue<bool>("SKIP_LOGIN_IP_CHECK", false, true, "TVM key, whether IP check during login should be skipped or not.");
        public BaseValue<bool> EnableHttpLogin = new BaseValue<bool>("EnableHttpLogin", true);

        public AuthorizationManagerConfiguration AuthorizationManagerConfiguration = new AuthorizationManagerConfiguration();
        public FileUploadConfiguration FileUpload = new FileUploadConfiguration();
        public ElasticSearchHandlerConfiguration ElasticSearchHandlerConfiguration = new ElasticSearchHandlerConfiguration();
        public AnnouncementManagerConfiguration AnnouncementManagerConfiguration = new AnnouncementManagerConfiguration();
        public MailerConfiguration MailerConfiguration = new MailerConfiguration();
        public GroupsManagerConfiguration GroupsManagerConfiguration = new GroupsManagerConfiguration();
        public RequestParserConfiguration RequestParserConfiguration = new RequestParserConfiguration();
        public OTTUserControllerConfiguration OTTUserControllerConfiguration = new OTTUserControllerConfiguration();
        public UsersCacheConfiguration UsersCacheConfiguration = new UsersCacheConfiguration();
        public CeleryRoutingConfiguration CeleryRoutingConfiguration = new CeleryRoutingConfiguration();
        public ImageResizerConfiguration ImageResizerConfiguration = new ImageResizerConfiguration();
        public AdaptersConfiguration AdaptersConfiguration = new AdaptersConfiguration();
        public FtpApiServerConfiguration FtpApiServerConfiguration = new FtpApiServerConfiguration();
        public HttpClientConfiguration HttpClientConfiguration = new HttpClientConfiguration();

        public DatabaseConfiguration DatabaseConfiguration = new DatabaseConfiguration();
        public NotificationConfiguration NotificationConfiguration = new NotificationConfiguration();
        public ImageUtilsConfiguration ImageUtilsConfiguration = new ImageUtilsConfiguration();
        public PushMessagesConfiguration PushMessagesConfiguration = new PushMessagesConfiguration();

        public WebServicesConfiguration WebServicesConfiguration = new WebServicesConfiguration(); // todo

        public ElasticSearchConfiguration ElasticSearchConfiguration = new ElasticSearchConfiguration();
        public HarmonicProviderConfiguration HarmonicProviderConfiguration = new HarmonicProviderConfiguration();
        public RoleIdsConfiguration RoleIdsConfiguration = new RoleIdsConfiguration();

        #region Configuration values





        
        public static CouchbaseClientConfiguration CouchbaseClientConfiguration;
     
        public static FacebookConfiguration FacebookConfiguration;
        public static TwitterConfiguration TwitterConfiguration;
        public static SocialFeedConfiguration SocialFeedConfiguration;
        public static SocialFeedQueueConfiguration SocialFeedQueueConfiguration;
        public static LayeredCacheConfigurationValidation LayeredCacheConfigurationValidation;
        public static ExportConfiguration ExportConfiguration;
        public static EngagementsConfiguration EngagementsConfiguration;
        public static CatalogLogicConfiguration CatalogLogicConfiguration;
        
        public static CDVRAdapterConfiguration CDVRAdapterConfiguration;
        public static EventConsumersConfiguration EventConsumersConfiguration;
        public static UserPINDigitsConfiguration UserPINDigitsConfiguration;


        #endregion

        #region Private Members

        private static List<ConfigurationValue> allConfigurationValues;
        private static string logPath = string.Empty;
        private static StringBuilder logBuilder;
        private static List<ConfigurationValue> configurationValuesWithOriginalKeys;
        private static bool isSilent = false;

        

        #endregion

        #region Public Static Methods

        public static void Init()
        {
            Type type = typeof(ApplicationConfiguration);
            Init(type, Current);
        }



        public static void Initialize(bool shouldLoadDefaults = false, bool silent = false, string application = "", string host = "", string environment = "")
        {
            isSilent = silent;

            if (!string.IsNullOrEmpty(application) || !string.IsNullOrEmpty(host) || !string.IsNullOrEmpty(environment))
            {
                var config = (TCMClient.TCMConfiguration)System.Configuration.ConfigurationManager.GetSection("TCMConfig");
                config.OverrideEnvironmentVariable();

                //config.OverrideEnvironmentVariable();
                
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
                TCMClient.Settings.Instance.Init(config.URL, application, host, environment, config.AppID, config.AppSecret, false);
            }
            else
            {
                TCMClient.Settings.Instance.Init();
            }

            CouchbaseClientConfiguration = new CouchbaseClientConfiguration("couchbase_client_config");
            

   
            FacebookConfiguration = new FacebookConfiguration("facebook_configuration");

            TwitterConfiguration = new TwitterConfiguration("twitter_configuration")
            {
            };
            SocialFeedConfiguration = new SocialFeedConfiguration("social_feed_configuration");

            SocialFeedQueueConfiguration = new SocialFeedQueueConfiguration("social_feed_queue_configuration");
            LayeredCacheConfigurationValidation = new LayeredCacheConfigurationValidation("LayeredCache");
            ExportConfiguration = new ExportConfiguration("export");
           
 
            CatalogLogicConfiguration = new CatalogLogicConfiguration("catalog_logic_configuration");
            
            CDVRAdapterConfiguration = new CDVRAdapterConfiguration("cdvr_adapter_configuration")
            {
                ShouldAllowEmpty = true
            };
          
  
            EngagementsConfiguration = new EngagementsConfiguration("engagements_configuration");
            
            EventConsumersConfiguration = new EventConsumersConfiguration("ConsumerSettings");
            //AuthorizationManagerConfiguration = new AuthorizationManagerConfiguration("authorization_manager_configuration");
            UserPINDigitsConfiguration = new UserPINDigitsConfiguration("user_pin_digits_configuration");

            allConfigurationValues = new List<ConfigurationValue>()
                {
                    
                    CouchbaseClientConfiguration,
                    FacebookConfiguration,
                    TwitterConfiguration,
                    SocialFeedConfiguration,
               
                    SocialFeedQueueConfiguration,
                    LayeredCacheConfigurationValidation,
                    ExportConfiguration,
                    CatalogLogicConfiguration,
                    CDVRAdapterConfiguration,
                    EngagementsConfiguration,
                    EventConsumersConfiguration,
                    UserPINDigitsConfiguration,
 
   
            };

            configurationValuesWithOriginalKeys = new List<ConfigurationManager.ConfigurationValue>();

        }

        

        #endregion

        #region Internal or Private Static Methods

        internal static void WriteToLog(string log)
        {
            if (string.IsNullOrEmpty(logPath))
            {
                if (!isSilent)
                {
                    Console.WriteLine(log);
                }
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