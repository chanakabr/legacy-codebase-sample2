using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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




        public static ProfessionalServicesTasksConfiguration ProfessionalServicesTasksConfiguration;

        #region TVM Configuration Values

        

        #endregion

        #region TVP Api

        public static TVPApiConfiguration TVPApiConfiguration;

        #endregion

        #region Configuration values



        
        
        public static CouchbaseSectionMapping CouchbaseSectionMapping;
        
        public static BaseCacheConfiguration BaseCacheConfiguration;
        public static DatabaseConfiguration DatabaseConfiguration;
        public static NamedCacheConfiguration WSCacheConfiguration;
        public static NamedCacheConfiguration ODBCWrapperCacheConfiguration;
        public static NamedCacheConfiguration CatalogCacheConfiguration;
        public static NamedCacheConfiguration NotificationCacheConfiguration;
        public static NamedCacheConfiguration GroupsCacheConfiguration;

        public static CouchBaseDesigns CouchBaseDesigns;
        
        public static EutelsatSettings EutelsatSettings;
        public static ElasticSearchConfiguration ElasticSearchConfiguration;
     
        public static HarmonicProviderConfiguration HarmonicProviderConfiguration;
        public static RabbitConfiguration RabbitConfiguration;
        public static CouchbaseClientConfiguration CouchbaseClientConfiguration;
        public static RoleIdsConfiguration RoleIdsConfiguration;
     
        public static FacebookConfiguration FacebookConfiguration;
        public static TwitterConfiguration TwitterConfiguration;
        public static SocialFeedConfiguration SocialFeedConfiguration;
        public static LicensedLinksCacheConfiguration LicensedLinksCacheConfiguration;
        public static SocialFeedQueueConfiguration SocialFeedQueueConfiguration;
        public static LayeredCacheConfigurationValidation LayeredCacheConfigurationValidation;
        public static ExportConfiguration ExportConfiguration;
        public static EngagementsConfiguration EngagementsConfiguration;
        public static CatalogLogicConfiguration CatalogLogicConfiguration;
        
        public static CDVRAdapterConfiguration CDVRAdapterConfiguration;
        public static EventConsumersConfiguration EventConsumersConfiguration;
        public static UserPINDigitsConfiguration UserPINDigitsConfiguration;
        public static WebServicesConfiguration WebServicesConfiguration;
        public static PushMessagesConfiguration PushMessagesConfiguration;
        public static ImageUtilsConfiguration ImageUtilsConfiguration;
        public static NotificationConfiguration NotificationConfiguration;


        
        
        public HttpClientConfiguration HttpClientConfiguration;


 


        #endregion

        #region Private Members

        private static List<ConfigurationValue> allConfigurationValues;
        private static string logPath = string.Empty;
        private static StringBuilder logBuilder;
        private static List<ConfigurationValue> configurationValuesWithOriginalKeys;
        private static bool isSilent = false;

        

        #endregion

        #region Public Static Methods

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
            
            #region Remote tasks configuration values

  /*          CeleryRoutingConfiguration = new CeleryRoutingConfiguration("CELERY_ROUTING")
            {
                ShouldAllowEmpty = true,
                Description = "Remote tasks celery routing. Not used in phoenix."
            };*/
 /*           ImageResizerConfiguration = new ImageResizerConfiguration("image_resizer_configuration")
            {
                ShouldAllowEmpty = true,
                Description = "Configuration for image resizer handler in remote tasks."
            };*/



            ProfessionalServicesTasksConfiguration = new ProfessionalServicesTasksConfiguration("professional_services_tasks")
            {
                ShouldAllowEmpty = true,
                Description = "Remote tasks configuratin for professional services handler."
            };
 
         

          
            #endregion


            
            
            
            
            CouchbaseSectionMapping = new CouchbaseSectionMapping("CouchbaseSectionMapping");
            
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

            EutelsatSettings = new EutelsatSettings("eutelsat_settings")
            {
                ShouldAllowEmpty = true
            };
            ElasticSearchConfiguration = new ElasticSearchConfiguration("elasticsearch_settings");
 

            HarmonicProviderConfiguration = new HarmonicProviderConfiguration("harmonic_provider_configuration");
            RabbitConfiguration = new RabbitConfiguration("rabbit_configuration");
            CouchbaseClientConfiguration = new CouchbaseClientConfiguration("couchbase_client_config");
            RoleIdsConfiguration = new RoleIdsConfiguration("role_ids");

   
            FacebookConfiguration = new FacebookConfiguration("facebook_configuration");

            TwitterConfiguration = new TwitterConfiguration("twitter_configuration")
            {
            };
            SocialFeedConfiguration = new SocialFeedConfiguration("social_feed_configuration");

            LicensedLinksCacheConfiguration = new LicensedLinksCacheConfiguration("licensed_links_cache_configuration");
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
            WebServicesConfiguration = new WebServicesConfiguration("WebServices");

            
            PushMessagesConfiguration = new PushMessagesConfiguration("push_messages");
            
          
            ImageUtilsConfiguration = new ImageUtilsConfiguration("image_utils_configuration")
            {
                ShouldAllowEmpty = true
            };

            //AdaptersConfiguration = new AdaptersConfiguration("adapters_client_configuration");
          

            TVPApiConfiguration = new TVPApiConfiguration("tvpapi_configuration")
            {
                ShouldAllowEmpty = true
            };

          

            allConfigurationValues = new List<ConfigurationValue>()
                {
                    ProfessionalServicesTasksConfiguration,
                    CouchbaseSectionMapping,
                    BaseCacheConfiguration,
                    DatabaseConfiguration,
                    WSCacheConfiguration,
                    CouchBaseDesigns,
                    EutelsatSettings,
                    ElasticSearchConfiguration,
                    HarmonicProviderConfiguration,
                    RabbitConfiguration,
                    CouchbaseClientConfiguration,
                    RoleIdsConfiguration,
                    FacebookConfiguration,
                    TwitterConfiguration,
                    SocialFeedConfiguration,
               
                    LicensedLinksCacheConfiguration,
                    SocialFeedQueueConfiguration,
                    LayeredCacheConfigurationValidation,
                    ExportConfiguration,
                    CatalogLogicConfiguration,
                    CDVRAdapterConfiguration,
                    EngagementsConfiguration,
                    EventConsumersConfiguration,
                    UserPINDigitsConfiguration,
                    WebServicesConfiguration,
                    PushMessagesConfiguration,
                    ImageUtilsConfiguration,
                    ODBCWrapperCacheConfiguration,
                    CatalogCacheConfiguration,
                    NotificationCacheConfiguration,
                    GroupsCacheConfiguration,
                    NotificationConfiguration,
   
                    TVPApiConfiguration
            };

            configurationValuesWithOriginalKeys = new List<ConfigurationManager.ConfigurationValue>();

/*            if (shouldLoadDefaults)
            {
                foreach (var configurationValue in allConfigurationValues)
                {
                    configurationValue.LoadDefault();
                }
            }*/
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
                Initialize(false, false, application, host, environment);

                foreach (var configurationValue in allConfigurationValues)
                {
                    try
                    {
                        result &= configurationValue.Validate();
                    }
                    catch (Exception ex)
                    {
                        WriteToLog(string.Format("Exception when validating: {0}", ex));
                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog(string.Format("Exception when validating: {0}", ex));
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