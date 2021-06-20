using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Response;
using ConfigurationManager;
using DAL;
using KLogMonitor;
using QueueWrapper;
using QueueWrapper.Queues;

namespace Core.GroupManagers
{
    public class PartnerManager
    {
        private const string ROUTING_PARAMETER_KEY = "partner_id";

        private static readonly Lazy<PartnerManager> LazyInstance = new Lazy<PartnerManager>(() => new PartnerManager(PartnerDal.Instance,
            RabbitConnection.Instance, ApplicationConfiguration.Current, UserManager.Instance, RabbitConfigDal.Instance, PricingDAL.Instance), LazyThreadSafetyMode.PublicationOnly);
        public static PartnerManager Instance => LazyInstance.Value;

        private static readonly KLogger Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly IPartnerDal _partnerDal;
        private readonly IPartnerRepository _pricingDal;
        private readonly IRabbitConnection _rabbitConnection;
        private readonly IApplicationConfiguration _applicationConfiguration;
        private readonly IUserManager _userManager;
        private readonly IRabbitConfigDal _rabbitConfigDal;
        private static readonly List<KeyValuePair<long, long>> usersModuleIdList = new List<KeyValuePair<long, long>>{
            new KeyValuePair<long, long>(1, 1), new KeyValuePair<long, long>(2, 1)};
        private static readonly List<KeyValuePair<long, long>> pricingModuleIdList = new List<KeyValuePair<long, long>>{
            new KeyValuePair<long, long>(1, 1), new KeyValuePair<long, long>(2, 1), new KeyValuePair<long, long>(3, 1),
        new KeyValuePair<long, long>(4, 1), new KeyValuePair<long, long>(5, 1), new KeyValuePair<long, long>(6, 1)};

        public PartnerManager(IPartnerDal partnerDal, IRabbitConnection rabbitConnection, IApplicationConfiguration applicationConfiguration, 
            IUserManager userManager, IRabbitConfigDal rabbitConfigDal, IPartnerRepository pricingDal)
        {
            _partnerDal = partnerDal;
            _rabbitConnection = rabbitConnection;
            _applicationConfiguration = applicationConfiguration;
            _userManager = userManager;
            _rabbitConfigDal = rabbitConfigDal;
            _pricingDal = pricingDal;
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

            if (!(_partnerDal.SetupPartnerInUsersDb(partnerId, usersModuleIdList, updaterId) &&
                _pricingDal.SetupPartnerInPricingDb(partnerId, pricingModuleIdList, updaterId)))
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
            CreateNewGroupRabbit(partnerId);

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
                
                return new GenericListResponse<Partner>(Status.Ok, partners) {TotalItems = partners.Count};
            }

            return new GenericListResponse<Partner>(Status.Ok, new List<Partner>(0));
        }

        private void CreateNewGroupRabbit(long groupId)
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
                throw new Exception("CreateNewGroupRabbit error");
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
                        throw new Exception("CreateNewGroupRabbit error");
                    }

                    configurationData.QueueName = queueName;
                    configurationData.RoutingKey = routingKey.Replace(ROUTING_PARAMETER_KEY, groupId.ToString()).Trim();

                    if (_rabbitConnection.AddRoutingKeyToQueue(configurationData))
                    {
                        Log.Debug($"Succeeded to add queue: {configurationData.RoutingKey}");
                    }
                    else
                    {
                        Log.Error($"Failed to add queue: {configurationData.RoutingKey}");
                        throw new Exception("CreateNewGroupRabbit error");
                    }
                }
            });
        }
    }
}