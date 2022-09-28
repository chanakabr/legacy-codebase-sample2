using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Phx.Lib.Appconfig;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using DAL;
using ElasticSearch.Common;
using Phx.Lib.Log;
using QueueWrapper;
using QueueWrapper.Enums;
using QueueWrapper.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MetaType = ApiObjects.MetaType;

namespace Core.GroupManagers
{
    public class PartnerManager
    {
        private const string ROUTING_PARAMETER_KEY = "partner_id";
        private static readonly Lazy<PartnerManager> LazyInstance = new Lazy<PartnerManager>(() =>
            new PartnerManager(PartnerDal.Instance,
                               RabbitConnection.Instance,
                               ApplicationConfiguration.Current,
                               UserManager.Instance,
                               RabbitConfigDal.Instance,
                               PricingDAL.Instance,
                               CatalogManager.Instance,
                               UsersDal.Instance,
                               BillingDAL.Instance,
                               ConditionalAccessDAL.Instance,
                               GroupSettingsManager.Instance,
                               IndexManagerFactory.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static PartnerManager Instance => LazyInstance.Value;

        private static readonly KLogger Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly IPartnerDal _partnerDal;
        private readonly IPricingPartnerRepository _pricingPartnerRepository;
        private readonly IRabbitConnection _rabbitConnection;
        private readonly IApplicationConfiguration _applicationConfiguration;
        private readonly IUserManager _userManager;
        private readonly IRabbitConfigDal _rabbitConfigDal;
        private readonly ICatalogManager _catalogManager;
        private readonly IUserPartnerRepository _userPartnerRepository;
        private readonly IBillingPartnerRepository _billingPartnerRepository;
        private readonly ICAPartnerRepository _caPartnerRepository;
        private readonly IGroupSettingsManager _groupSettingsManager;
        private readonly IIndexManagerFactory _indexManagerFactory;
        

        public PartnerManager(IPartnerDal partnerDal,
                              IRabbitConnection rabbitConnection,
                              IApplicationConfiguration applicationConfiguration,
                              IUserManager userManager,
                              IRabbitConfigDal rabbitConfigDal,
                              IPricingPartnerRepository pricingDal,
                              ICatalogManager catalogManager,
                              IUserPartnerRepository userPartnerRepository,
                              IBillingPartnerRepository billingPartnerRepository,
                              ICAPartnerRepository caPartnerRepository,
                              IGroupSettingsManager groupSettingsManager,
                              IIndexManagerFactory indexManagerFactory)
        {
            _partnerDal = partnerDal;
            _rabbitConnection = rabbitConnection;
            _applicationConfiguration = applicationConfiguration;
            _userManager = userManager;
            _rabbitConfigDal = rabbitConfigDal;
            _pricingPartnerRepository = pricingDal;
            _catalogManager = catalogManager;
            _userPartnerRepository = userPartnerRepository;
            _billingPartnerRepository = billingPartnerRepository;
            _caPartnerRepository = caPartnerRepository;
            _groupSettingsManager = groupSettingsManager;
            _indexManagerFactory = indexManagerFactory;
        }

        public GenericResponse<Partner> AddPartner(Partner partner, PartnerSetup partnerSetup, long updaterId)
        {
            var response = new GenericResponse<Partner>();
            // validate if partner already exist
            var existingPartners = GetPartners();
            if (existingPartners.HasObjects())
            {
                if (partner.Id.HasValue && existingPartners.Objects.Exists(_ => _.Id == partner.Id.Value))
                {
                    response.SetStatus(eResponseStatus.Error, $"Partner id:{partner.Id} already exist");
                    return response;
                }

                if (existingPartners.Objects.Exists(_ => _.Name == partner.Name))
                {
                    response.SetStatus(eResponseStatus.Error, $"Partner name:{partner.Name} already exist");
                    return response;
                }
            }

            var partnerId = _partnerDal.AddPartner(partner.Id, partner.Name, updaterId);
            if (partnerId <= 0) return response;
            partner.Id = partnerId;

            if (!(_userPartnerRepository.SetupPartnerInDb(partnerId, updaterId) &&
                _partnerDal.SetupPartnerInDb(partnerId, partner.Name, updaterId) &&
                _pricingPartnerRepository.SetupPartnerInDb(partnerId, updaterId) &&
                _billingPartnerRepository.SetupPartnerInDb(partnerId, updaterId) &&
                _caPartnerRepository.SetupPartnerInDb(partnerId, updaterId)))
            {
                response.SetStatus(eResponseStatus.Error, "Failed to create partner basic data");
                return response; // TODO rollback?
            }

            var userId = _userManager.AddAdminUser(partnerId, partnerSetup.AdminUsername, partnerSetup.AdminPassword);
            if (userId <= 0)
            {
                response.SetStatus(eResponseStatus.Error, $"Failed to add first admin user:{partnerSetup.AdminUsername} to partner");
                return response;
            }

            // TODO - WHEN ERRORS HANDLE SOMEHOW
            IterateRabbitQueues(partnerId, RoutingKeyQueueAction.Bind);

            response.SetStatus(eResponseStatus.OK);
            response.Object = partner;
            return response;
        }

        public GenericListResponse<Partner> GetPartners(List<long> partnerIds = null)
        {
            var partners = _partnerDal.GetPartners();

            if (partners.Count > 0)
            {
                if (partnerIds != null && partnerIds.Count > 0)
                {
                    partners = partners.FindAll(p => partnerIds.Contains(p.Id.Value));
                }

                return new GenericListResponse<Partner>(Status.Ok, partners) { TotalItems = partners.Count };
            }

            return new GenericListResponse<Partner>(Status.Ok, new List<Partner>(0));
        }

        public Status Delete(long updaterId, int id)
        {
            Status result = new Status();

            if (!_partnerDal.IsPartnerExists(id))
            {
                result.Set(eResponseStatus.PartnerDoesNotExist, $"Partner {id} does not exist");
                return result;
            }

            IterateRabbitQueues(id, RoutingKeyQueueAction.Unbind);

            // cant delete first admin user that was created because partner-accounts-setup ms does not hold first admin user details (just the app token)

            var failedToDeletedBasicData = false;
            if (!_caPartnerRepository.DeletePartnerBasicDataDb(id, updaterId))
            {
                failedToDeletedBasicData = true;
                Log.Error($"Error while delete partner ConditionalAccess basicData. id: {id}, updaterId: {updaterId}.");
            }

            if (!_billingPartnerRepository.DeletePartnerBasicDataDb(id, updaterId))
            {
                failedToDeletedBasicData = true;
                Log.Error($"Error while delete partner billing basicData. id: {id}, updaterId: {updaterId}.");
            }

            if (!_pricingPartnerRepository.DeletePartnerBasicDataDb(id, updaterId))
            {
                failedToDeletedBasicData = true;
                Log.Error($"Error while delete partner pricing basicData. id: {id}, updaterId: {updaterId}.");
            }

            if (!_partnerDal.DeletePartnerBasicDataDb(id, updaterId))
            {
                failedToDeletedBasicData = true;
                Log.Error($"Error while delete partner tivinci basicData. id: {id}, updaterId: {updaterId}.");
            }

            if (!_userPartnerRepository.DeletePartnerDb(id, updaterId))
            {
                failedToDeletedBasicData = true;
                Log.Error($"Error while delete partner users basicData. id: {id}, updaterId: {updaterId}.");
            }

            if (failedToDeletedBasicData)
            {
                result.Set(eResponseStatus.Error, "Failed to delete partner basic data");
                return result;
            }

            if (!_partnerDal.DeletePartner(id, updaterId))
            {
                Log.Error($"Error while Delete Partner. id: {id},updaterId: {updaterId}.");
                result.Set(eResponseStatus.Error, "Error while Delete Partner");
                return result;
            }

            result.Set(eResponseStatus.OK);

            return result;
        }

        private void IterateRabbitQueues(long groupId, RoutingKeyQueueAction action)
        {
            // Get Queue list and routing key
            Dictionary<string, string> rabbitRoutingKetWithQueueNameDic = _rabbitConfigDal.GetRabbitRoutingBindings();
            if (rabbitRoutingKetWithQueueNameDic.Count < 1)
            {
                Log.Error("Failed to get Rabbit queue bindings from db");
                throw new Exception("CreateNewGroupRabbit error");
            }

            RabbitQueue rabbitQueue = new RabbitQueue(_applicationConfiguration);
            var configurationDataForInitialize = rabbitQueue.CreateRabbitConfigurationData();
            int retryCount = 0;

            if (configurationDataForInitialize == null)
            {
                Log.Error("Error while getting queue TCM configuration");
                throw new Exception("InitializeRabbitInstance error");
            }

            // Need to Initialize before so not all the parallel will try to Initialize at the same time 
            if (!_rabbitConnection.InitializeRabbitInstance(configurationDataForInitialize, QueueAction.Ack, ref retryCount, out var connection) && connection != null)
            {
                Log.Error("Error while initialize rabbit instance");
                throw new Exception("InitializeRabbitInstance error");
            }

            Parallel.ForEach(rabbitRoutingKetWithQueueNameDic.Keys, (queueName) =>
            {
                // in case value is split by ";" --> bind is needed for every routing key
                var routingKeys = rabbitRoutingKetWithQueueNameDic[queueName].Split(';');

                foreach (string routingKey in routingKeys)
                {
                    var configurationData = rabbitQueue.CreateRabbitConfigurationData();

                    if (configurationData == null)
                    {
                        Log.Error("Error while getting queue TCM configuration");
                        throw new Exception("GetRabbitConfigurationData error");
                    }

                    configurationData.QueueName = queueName;
                    configurationData.RoutingKey = routingKey.Replace(ROUTING_PARAMETER_KEY, groupId.ToString()).Trim();

                    if (_rabbitConnection.IterateRoutingKeyQueue(configurationData, action))
                    {
                        Log.Debug($"Succeeded to iterate Rabbit queue: {configurationData.RoutingKey}");
                    }
                    else
                    {
                        Log.Error($"Failed to iterate Rabbit queue: {configurationData.RoutingKey}");
                        throw new Exception("CreateNewGroupRabbit error");
                    }
                }
            });
        }

        public Status CreateIndexes(int groupId)
        {
            if (!_partnerDal.IsPartnerExists(groupId))
            {
                return new Status(eResponseStatus.PartnerDoesNotExist, $"Partner {groupId} does not exist");
            }

            if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache))
            {
                Log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling CreateIndexes", groupId);
                return Status.Error;
            }

            var indexManager = _indexManagerFactory.GetIndexManager(groupId);
            var languages = catalogGroupCache.LanguageMapById.Values.ToList();

            Task<Status>[] taskArray = {
                Task<Status>.Factory.StartNew(() => CreateMediaIndex(indexManager, catalogGroupCache, languages)),
                Task<Status>.Factory.StartNew(() => CreateEpgIndex(groupId, indexManager, catalogGroupCache, languages)),
                Task<Status>.Factory.StartNew(() => CreateRecordingIndex(indexManager, catalogGroupCache, languages)),
                Task<Status>.Factory.StartNew(() => CreateTagsIndex(indexManager)),
                Task<Status>.Factory.StartNew(() => CreateChannelsIndex(indexManager)),
            };

            Task.WaitAll(taskArray);
            var errorTasks = taskArray.Where(t => !t.Result.IsOkStatusCode()).ToList();
            return errorTasks.Count == 0 ? Status.Ok : Status.ErrorMessage(string.Join("; ", errorTasks.Select(_ => _.Result.Message)));
        }

        private Status CreateMediaIndex(IIndexManager indexManager, CatalogGroupCache catalogGroupCache, List<LanguageObj> languages)
        {
            try
            {
                var indexDate = DateTime.UtcNow;
                var result = indexManager.SetupMediaIndex(indexDate);
                if (!result)
                {
                    return new Status(eResponseStatus.Error, "error creating media index");
                }
                indexManager.PublishMediaIndex(indexDate, true, true);
                return Status.Ok;
            }
            catch (Exception ex)
            {
                Log.Error("error creating media index", ex);
                return new Status(eResponseStatus.Error, "error creating media index");
            }
        }

        private Status CreateEpgIndex(long groupId, IIndexManager indexManager, CatalogGroupCache catalogGroupCache, List<LanguageObj> languages)
        {
            try
            {
                var epgFeatureVersion = _groupSettingsManager.GetEpgFeatureVersion((int)groupId);
                if (epgFeatureVersion != EpgFeatureVersion.V1)
                {
                    Log.InfoFormat($"epg feature version is set to:[{epgFeatureVersion}] skipping creation of index, it will be created on the first ingest");
                    return Status.Ok;
                }
                
                var epgIndex = indexManager.SetupEpgIndex(DateTime.UtcNow, isRecording: false);
                if (string.IsNullOrEmpty(epgIndex))
                {
                    Log.Warn("create epg index returned with an empty index name");
                    return new Status(eResponseStatus.Error, "error creating epg index");
                }

                bool publishResult = indexManager.PublishEpgIndex(epgIndex, isRecording: false, true, true);

                if (!publishResult)
                {
                    Log.Warn("create epg index - failed publishing epg index");
                    return new Status(eResponseStatus.Error, "error creating epg index");
                }

                return Status.Ok;
            }
            catch (Exception ex)
            {
                Log.Error("error creating epg index", ex);
                return new Status(eResponseStatus.Error, "error creating epg index");
            }
        }

        private Status CreateRecordingIndex(IIndexManager indexManager, CatalogGroupCache catalogGroupCache, List<LanguageObj> languages)
        {
            try
            {
                var indexName = indexManager.SetupEpgIndex(DateTime.UtcNow, isRecording: true);

                if (string.IsNullOrEmpty(indexName))
                {
                    Log.Warn("create recording index returned with an empty index name");
                    return new Status(eResponseStatus.Error, "error creating recording index");
                }

                bool publishResult = indexManager.PublishEpgIndex(indexName, isRecording: true, true, true);

                if (!publishResult)
                {
                    Log.Warn("create recording index - failed publishing recording index");
                    return new Status(eResponseStatus.Error, "error creating recording index");
                }

                return Status.Ok;
            }
            catch (Exception ex)
            {
                Log.Error("error creating recording index", ex);
                return new Status(eResponseStatus.Error, "error creating recording index");
            }
        }

        private Status CreateTagsIndex(IIndexManager indexManager)
        {
            try
            {
                var indexName = indexManager.SetupTagsIndex(DateTime.UtcNow);
                if (string.IsNullOrEmpty(indexName))
                {
                    Log.Warn("create tags index returned with an empty index name");
                    return new Status(eResponseStatus.Error, "error creating tags index");
                }

                bool publishResult = indexManager.PublishTagsIndex(indexName, true, true);
                if (!publishResult)
                {
                    Log.Warn("create tags index - failed publishing tags index");
                    return new Status(eResponseStatus.Error, "error creating tags index");
                }

                return Status.Ok;
            }
            catch (Exception ex)
            {
                Log.Error("error creating tags index", ex);
                return new Status(eResponseStatus.Error, "error creating tags index");
            }
        }

        private Status CreateChannelsIndex(IIndexManager indexManager)
        {
            try
            {
                var indexName = indexManager.SetupChannelMetadataIndex(DateTime.UtcNow);
                if (string.IsNullOrEmpty(indexName))
                {
                    Log.Warn("create channel index returned with an empty index name");
                    return new Status(eResponseStatus.Error, "error creating channel index");
                }

                indexManager.PublishChannelsMetadataIndex(indexName, true, true);

                return Status.Ok;
            }
            catch (Exception ex)
            {
                Log.Error("error creating channel index", ex);
                return new Status(eResponseStatus.Error, "error creating channel index");
            }
        }
    }
}