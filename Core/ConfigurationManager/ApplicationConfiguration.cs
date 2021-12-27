using ConfigurationManager.Types;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ConfigurationManager
{
    public interface IApplicationConfiguration
    {
        RabbitConfiguration RabbitConfiguration { get; }
        GroupsManagerConfiguration GroupsManagerConfiguration { get; }
        ElasticSearchHandlerConfiguration ElasticSearchHandlerConfiguration { get; }
        ElasticSearchConfiguration ElasticSearchConfiguration { get; }
        ElasticSearchHttpClientConfiguration ElasticSearchHttpClientConfiguration { get; }
        BaseValue<int> RecordingsMaxDegreeOfParallelism { get; }
        BaseValue<string> CatalogSignatureKey { get; }
        EPGIngestV2Configuration EPGIngestV2Configuration { get; }
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    // BaseConfig.IterateOverClassFields goes over public fields(not properties, not private fields) with reflection and set values to them
    // But also we want to mock ApplicationConfiguration
    // TODO change from public fields to properties at least we could mock them
    public class ApplicationConfiguration : BaseConfig<ApplicationConfiguration>, IApplicationConfiguration
    {
        public override string TcmKey => null;
        public override string[] TcmPath => null;

        private ApplicationConfiguration()
        {
        }

        private static readonly Lazy<ApplicationConfiguration> LazyInstance = new Lazy<ApplicationConfiguration>(() => new ApplicationConfiguration(), System.Threading.LazyThreadSafetyMode.PublicationOnly);
        public static ApplicationConfiguration Current => LazyInstance.Value;

        public BaseValue<string> ExcludeTemplatesImplementation = new BaseValue<string>("EXCLUDE_TEMPLATES_IMPLEMENTATION", "203");
        public BaseValue<string> UDRMUrl = new BaseValue<string>("UDRM_URL", TcmObjectKeys.Stub, true);
        public BaseValue<string> UseOldImageServer = new BaseValue<string>("USE_OLD_IMAGE_SERVER", "0", false, "Group Ids, split by ';', that wish to use old image server");
        public BaseValue<string> DMSAdapterUrl = new BaseValue<string>("DMS_ADAPTER_URL", "https://dms.service.consul/");
        public BaseValue<string> PlayManifestDynamicQueryStringParamsNames = new BaseValue<string>("PlayManifestDynamicQueryStringParamsNames", "clientTag,playSessionId");
        public BaseValue<string> Version = new BaseValue<string>("Version", "SV", false, "CouchBase document prefix. Each version has its own cached document to avoid backward compatibilty issues.");
        public BaseValue<string> PictureUploaderPath = new BaseValue<string>("pic_uploader_path", string.Empty, false, "Configuration for DBManipulator/CouchBaseManipulator in TVM.");
        public BaseValue<string> TVMBaseUrl = new BaseValue<string>("BASE_URL", TcmObjectKeys.Stub, true, "Base URL for TVM.");
        public BaseValue<string> BatchUpload = new BaseValue<string>("batch_upload", TcmObjectKeys.Stub, true);
        public BaseValue<string> LookupGenericUpload = new BaseValue<string>("lookup_generic_upload", TcmObjectKeys.Stub, true);
        public BaseValue<string> AppState = new BaseValue<string>("APP_STATE", "normal");
        public BaseValue<string> ServerName = new BaseValue<string>("SERVER_NAME", "TVM_EU");
        public BaseValue<string> ApplicationName = new BaseValue<string>("APPLICATION_NAME", TcmObjectKeys.Stub, true);
        public BaseValue<string> GraceNoteXSLTPath = new BaseValue<string>("GraceNote_XSLT_PATH", TcmObjectKeys.Stub, true, "Remote tasks configuration for EPG XDTV Transformation.");
        public BaseValue<string> GraceNoteALUIdConvertion = new BaseValue<string>("GraceNote_ALU_IDConvertion", TcmObjectKeys.Stub, true, "Remote tasks configuration for EPG XDTV Transformation.");
        public BaseValue<string> DMSUrl = new BaseValue<string>("dms_url", TcmObjectKeys.Stub, true, "Address of DMS server.");
        
        private BaseValue<string> _catalogSignatureKey = new BaseValue<string>("CatalogSignatureKey", "liat regev", false, "liat regev");
        public BaseValue<string> CatalogSignatureKey => _catalogSignatureKey;

        public BaseValue<string> ExcludePsDllImplementation = new BaseValue<string>("EXCLUDE_PS_DLL_IMPLEMENTATION", TcmObjectKeys.Stub, true);
        public BaseValue<string> FriendsActivityViewStaleState = new BaseValue<string>("FRIENDS_ACTIVITY_VIEW_STALE_STATE", "None", false, "Corresponding to ViewStaleState enum. Possible values: None, False, Ok, UpdateAfter");
        public BaseValue<string> EncryptorService = new BaseValue<string>("EncryptorService", TcmObjectKeys.Stub, true);
        public BaseValue<string> EncryptorPassword = new BaseValue<string>("EncryptorPassword", TcmObjectKeys.Stub, true);
        public BaseValue<string> PicsBasePath = new BaseValue<string>("pics_base_path", TcmObjectKeys.Stub, true);
        public BaseValue<string> AdyenPSPReferenceRegexOverride = new BaseValue<string>("AdyenPSPReferenceRegexOverride", TcmObjectKeys.Stub, true);
        public BaseValue<string> MetaFeaturesPattern = new BaseValue<string>("meta_features_pattern", @"\W|[^ ]{64}[^ ]", false);
        public BaseValue<int> PwwawpMaxResultsSize = new BaseValue<int>("PWWAWP_MAX_RESULTS_SIZE", 8);
        public BaseValue<int> PwlalPMaxResultsSize = new BaseValue<int>("PWLALP_MAX_RESULTS_SIZE", 8);
        public BaseValue<int> PreviewModuleNumOfCancelOrRefundAttempts = new BaseValue<int>("PreviewModuleNumOfCancelOrRefundAttempts", 4, false, "Number of attempts when Adyen Direct Debit sends a cancel or refund request to the payment port client"); 
        public BaseValue<int> EPGDocumentExpiry = new BaseValue<int>("epg_doc_expiry", 7);
        public BaseValue<int> UserInterestsTTLDays = new BaseValue<int>("ttl_user_interest_days", 30);
        public BaseValue<int> PersonalizedFeedTTLDays = new BaseValue<int>("PersonalizedFeedTTLDays", 365);
        
        private BaseValue<int> _recordingsMaxDegreeOfParallelism = new BaseValue<int>("recordings_max_degree_of_parallelism", 5);
        public BaseValue<int> RecordingsMaxDegreeOfParallelism => _recordingsMaxDegreeOfParallelism;

        public BaseValue<int> QueueFailLimit = new BaseValue<int>("queue_fail_limit", 3, false, "Retry limit for RabbitMQ actions like enqueue.");
        public BaseValue<int> PendingThresholdDays = new BaseValue<int>("pending_threshold_days", 180);
        public BaseValue<int> UserSegmentTTL = new BaseValue<int>("user_segment_ttl_hours", 36, false, "How long do we keep information about the users' segments");
        public BaseValue<int> EPGDeleteBulkSize = new BaseValue<int>("epg_delete_bulk_size", 10);
        public BaseValue<int> MediaMarksListLength = new BaseValue<int>("media_marks_list_limit", 300);
        public BaseValue<int> MediaMarksTTL = new BaseValue<int>("media_marks_ttl_days", 90);
        public BaseValue<int> LogReloadInterval = new BaseValue<int>("log_reload_interval", 0, false, "Interval of reloading the KLogger configuration from Couchbase, in milliseconds. If set to a positive value, log reload mechanism will be initiated on this application.");
        public BaseValue<double> SocialCacheDocTimeout = new BaseValue<double>("socialCacheDocTimeout", 1440);
        public BaseValue<long> ReconciliationFrequencySeconds = new BaseValue<long>("reconciliation_frequency_seconds", 7200);
        public BaseValue<ulong> EpgInitialId = new BaseValue<ulong>("epg_initial_id", 100000000);
        public BaseValue<bool> DownloadPicWithQueue = new BaseValue<bool>("downloadPicWithQueue", false);
        public BaseValue<bool> CheckImageUrl = new BaseValue<bool>("CheckImageUrl", true);
        public BaseValue<bool> AllowUnknownCountry = new BaseValue<bool>("allow_unknown_country", false);
        public BaseValue<bool> ShouldSubscriptionOverlapConsiderDLM = new BaseValue<bool>("should_subscription_overlap_consider_dlm", false);
        public BaseValue<bool> TVMSkipLoginIPCheck = new BaseValue<bool>("SKIP_LOGIN_IP_CHECK", false, false, "TVM key, whether IP check during login should be skipped or not.");
        public BaseValue<bool> EnableHttpLogin = new BaseValue<bool>("EnableHttpLogin", true);
        public BaseValue<int> CbMaxInsertTries = new BaseValue<int>("cb_max_insert_tries", 2);
        public BaseValue<List<HealthCheckDefinition>> HealthCheckConfiguration = 
            new BaseValue<List<HealthCheckDefinition>>(TcmObjectKeys.HealthCheckConfiguration, new List<HealthCheckDefinition>()
                {
                    new HealthCheckDefinition() { Type = HealthCheckType.ElasticSearch },
                    new HealthCheckDefinition() { Type = HealthCheckType.SQL },
                    new HealthCheckDefinition() { Type = HealthCheckType.CouchBase },
                    new HealthCheckDefinition() { Type = HealthCheckType.RabbitMQ }
                }, false, 
            "List of definitions of health check that this application will use. By default it is ES, CB, SQL and Rabbit.");

        public AuthorizationManagerConfiguration AuthorizationManagerConfiguration = new AuthorizationManagerConfiguration();
        public FileUploadConfiguration FileUpload = new FileUploadConfiguration();
        public DataLakeConfiguration DataLake = new DataLakeConfiguration();
        public ElasticSearchHandlerConfiguration _elasticSearchHandlerConfiguration = new ElasticSearchHandlerConfiguration();
        public ElasticSearchHandlerConfiguration ElasticSearchHandlerConfiguration => _elasticSearchHandlerConfiguration;
        public AnnouncementManagerConfiguration AnnouncementManagerConfiguration = new AnnouncementManagerConfiguration();
        public MailerConfiguration MailerConfiguration = new MailerConfiguration();
        public GroupsManagerConfiguration _groupsManagerConfiguration = new GroupsManagerConfiguration();
        public GroupsManagerConfiguration GroupsManagerConfiguration => _groupsManagerConfiguration;
        public RequestParserConfiguration RequestParserConfiguration = new RequestParserConfiguration();
        public OTTUserControllerConfiguration OTTUserControllerConfiguration = new OTTUserControllerConfiguration();
        
        public EPGIngestV2Configuration _epgIngestV2Configuration = new EPGIngestV2Configuration();
        public EPGIngestV2Configuration EPGIngestV2Configuration => _epgIngestV2Configuration;
        
        public ImageResizerConfiguration ImageResizerConfiguration = new ImageResizerConfiguration();
        public FtpApiServerConfiguration FtpApiServerConfiguration = new FtpApiServerConfiguration();
        public HttpClientConfiguration HttpClientConfiguration = new HttpClientConfiguration();
        public MicroservicesClientConfiguration MicroservicesClientConfiguration = new MicroservicesClientConfiguration();
        public DatabaseConfiguration DatabaseConfiguration = new DatabaseConfiguration();
        public NotificationConfiguration NotificationConfiguration = new NotificationConfiguration();
        public ImageUtilsConfiguration ImageUtilsConfiguration = new ImageUtilsConfiguration();
        public PushMessagesConfiguration PushMessagesConfiguration = new PushMessagesConfiguration();
        public WebServicesConfiguration WebServicesConfiguration = new WebServicesConfiguration();
        public ElasticSearchConfiguration _elasticSearchConfiguration = new ElasticSearchConfiguration();
        public ElasticSearchConfiguration ElasticSearchConfiguration => _elasticSearchConfiguration;
        
        public RoleIdsConfiguration RoleIdsConfiguration = new RoleIdsConfiguration();
        public TwitterConfiguration TwitterConfiguration = new TwitterConfiguration();
        public FacebookConfiguration FacebookConfiguration = new FacebookConfiguration();
        public SocialFeedConfiguration SocialFeedConfiguration = new SocialFeedConfiguration();
        public SocialFeedQueueConfiguration SocialFeedQueueConfiguration = new SocialFeedQueueConfiguration();
        public ExportConfiguration ExportConfiguration = new ExportConfiguration();
        public CDVRAdapterConfiguration CDVRAdapterConfiguration = new CDVRAdapterConfiguration();
        public SqlTrafficConfiguration SqlTrafficConfiguration = new SqlTrafficConfiguration();
        public UserPINDigitsConfiguration UserPINDigitsConfiguration = new UserPINDigitsConfiguration();
        public EngagementsConfiguration EngagementsConfiguration = new EngagementsConfiguration();
        public CatalogLogicConfiguration CatalogLogicConfiguration = new CatalogLogicConfiguration();
        public LayeredCacheConfigurationValidation LayeredCacheConfigurationValidation = new LayeredCacheConfigurationValidation();
        public EventConsumersConfiguration EventConsumersConfiguration = new EventConsumersConfiguration();
        public RabbitConfiguration _rabbitConfiguration = new RabbitConfiguration();
        public RabbitConfiguration RabbitConfiguration => _rabbitConfiguration;
        public WSCacheConfiguration WSCacheConfiguration = new WSCacheConfiguration();
        public TVPApiConfiguration TVPApiConfiguration = new TVPApiConfiguration();
        public BaseCacheConfiguration BaseCacheConfiguration = new BaseCacheConfiguration();
        public ODBCWrapperCacheConfiguration ODBCWrapperCacheConfiguration = new ODBCWrapperCacheConfiguration();
        public CatalogCacheConfiguration CatalogCacheConfiguration = new CatalogCacheConfiguration();
        public NotificationCacheConfiguration NotificationCacheConfiguration = new NotificationCacheConfiguration();
        public GroupsCacheConfiguration GroupsCacheConfiguration = new GroupsCacheConfiguration();
        public LicensedLinksCacheConfiguration LicensedLinksCacheConfiguration = new LicensedLinksCacheConfiguration();
        public UserEncryptionKeysCacheConfiguration UserEncryptionKeysCacheConfiguration = new UserEncryptionKeysCacheConfiguration();
        public CouchBaseDesigns CouchBaseDesigns = new CouchBaseDesigns();
        public CouchbaseSectionMapping CouchbaseSectionMapping = new CouchbaseSectionMapping();
        public CouchbaseClientConfiguration CouchbaseClientConfiguration = new CouchbaseClientConfiguration();
        public AdaptersConfiguration AdaptersConfiguration = new AdaptersConfiguration();
        public CeleryRoutingConfiguration CeleryRoutingConfiguration = new CeleryRoutingConfiguration();
        public ProfessionalServicesTasksConfiguration ProfessionalServicesTasksConfiguration = new ProfessionalServicesTasksConfiguration();
        public NPVRHttpClientConfiguration NPVRHttpClientConfiguration = new NPVRHttpClientConfiguration();
        public KafkaClientConfiguration KafkaClientConfiguration = new KafkaClientConfiguration();
        public ElasticSearchHttpClientConfiguration _elasticSearchHttpClientConfiguration = new ElasticSearchHttpClientConfiguration();
        public ElasticSearchHttpClientConfiguration ElasticSearchHttpClientConfiguration => _elasticSearchHttpClientConfiguration;
        public MailerHttpClientConfiguration MailerHttpClientConfiguration = new MailerHttpClientConfiguration();
        public IotHttpClientConfiguration IotHttpClientConfiguration = new IotHttpClientConfiguration();
        public UdidUsageConfiguration UdidUsageConfiguration = new UdidUsageConfiguration();
        public IotAdapterConfiguration IotAdapterConfiguration = new IotAdapterConfiguration();
        public KestrelConfiguration KestrelConfiguration = new KestrelConfiguration();
        public RedisClientConfiguration RedisClientConfiguration = new RedisClientConfiguration();
        public LayeredCacheInMemoryCacheConfiguration LayeredCacheInMemoryCacheConfiguration = new LayeredCacheInMemoryCacheConfiguration();
        public GeneralInMemoryCacheConfiguration GeneralInMemoryCacheConfiguration = new GeneralInMemoryCacheConfiguration();

        public T GetValueByKey<T>(string key)
        {
            T result = default;

            try
            {
                result = TCMClient.Settings.Instance.GetValue<T>(key);
            }
            catch (Exception ex)
            {
                _Logger.Error("Error getting key:" + key + ", from TCM" + ex.Message, ex);
            }

            return result;
        }

        public static void Init()
        {
            TCMClient.Settings.Instance.Init();
            Init(Current);
        }

        public static void InitDefaults()
        {
            Init(Current);
        }
    }
}