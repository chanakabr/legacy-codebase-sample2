using ApiObjects.CanaryDeployment;
using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Reflection;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.CanaryDeployment.Microservices;
using CanaryDeploymentManager;
using WebAPI.ClientManagers.Client;
using WebAPI.Models.CanaryDeployment;
using WebAPI.Models.CanaryDeployment.Elasticsearch;
using WebAPI.Models.CanaryDeployment.Microservices;
using WebAPI.ObjectsConvertor.Mapping;

namespace WebAPI.Clients
{
    public class CanaryDeploymentClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public CanaryDeploymentClient()
        {
        }

        public KalturaMicroservicesCanaryDeploymentConfiguration GetMicroservicesCanaryDeploymentConfiguration(int groupId)
        {
            Func<GenericResponse<MicroservicesCanaryDeploymentConfiguration>> getGroupConfiguration = () => CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().GetGroupConfiguration(groupId);
            return ClientUtils.GetResponseFromWS<KalturaMicroservicesCanaryDeploymentConfiguration, MicroservicesCanaryDeploymentConfiguration>(getGroupConfiguration);
        }

        public bool SetMicroservicesMigrationEventStatus(int groupId, KalturaCanaryDeploymentMicroservicesMigrationEvent microservicesMigrationEvent, bool status)
        {
            Func<Status> setMigrationEvent = null;
            CanaryDeploymentMigrationEvent canaryDeploymentMigrationEvent = CanaryDeploymentMapping.ConvertMigrationEvent(microservicesMigrationEvent);
            if (status)
            {
                setMigrationEvent = () => CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().EnableMigrationEvent(groupId, canaryDeploymentMigrationEvent);
            }
            else
            {
                setMigrationEvent = () => CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().DisableMigrationEvent(groupId, canaryDeploymentMigrationEvent);
            }

            return ClientUtils.GetResponseStatusFromWS(setMigrationEvent);
        }

        public bool SetAllMicroservicesMigrationEventsStatus(int groupId, bool status)
        {
            Func<Status> setAllMigrationEvents = () => CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().SetAllMigrationEventsStatus(groupId, status);
            return ClientUtils.GetResponseStatusFromWS(setAllMigrationEvents);
        }

        public bool SetMicroservicesRoutingAction(int groupId, KalturaCanaryDeploymentMicroservicesRoutingAction microservicesRoutingAction, KalturaCanaryDeploymentMicroservicesRoutingService microservicesRoutingService)
        {
            CanaryDeploymentRoutingAction canaryDeploymentRoutingAction = CanaryDeploymentMapping.ConvertRoutingAction(microservicesRoutingAction);
            MicroservicesCanaryDeploymentRoutingService microservicesCanaryDeploymentRoutingService = CanaryDeploymentMapping.ConvertRoutingService(microservicesRoutingService);
            Func<Status> setRoutingAction = () => CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().SetRoutingAction(groupId, canaryDeploymentRoutingAction, microservicesCanaryDeploymentRoutingService);         

            return ClientUtils.GetResponseStatusFromWS(setRoutingAction);
        }

        public bool SetAllMicroservicesRoutingActionsToMs(int groupId, bool enableMs)
        {
            Func<Status> setAllRoutingActionsToMs = () => CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().SetAllRoutingActionsToMs(groupId, enableMs);

            return ClientUtils.GetResponseStatusFromWS(setAllRoutingActionsToMs);
        }

        public bool DeleteMicroservicesCanaryDeploymentConfiguration(int groupId)
        {
            Func<Status> deleteGroupConfiguration = () => CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().DeleteGroupConfiguration(groupId);
            return ClientUtils.GetResponseStatusFromWS(deleteGroupConfiguration);
        }
        
        
        
        public KalturaElasticsearchCanaryDeploymentConfiguration GetElasticsearchCanaryDeploymentConfiguration(int groupId)
        {
            Func<GenericResponse<ElasticsearchCanaryDeploymentConfiguration>> getGroupConfiguration = () => CanaryDeploymentFactory.Instance.GetElasticsearchCanaryDeploymentManager().GetPartnerConfiguration(groupId);
            return ClientUtils.GetResponseFromWS<KalturaElasticsearchCanaryDeploymentConfiguration, ElasticsearchCanaryDeploymentConfiguration>(getGroupConfiguration);
        }

        public bool SetElasticsearchMigrationEventsStatus(int groupId, bool status)
        {
            var res = CanaryDeploymentFactory.Instance.GetElasticsearchCanaryDeploymentManager().SetMigrationEventsStatus(groupId, status);
            return res.IsOkStatusCode();
        }

        public bool SetElasticsearchActiveVersion(int groupId, KalturaElasticsearchVersion activeVersion)
        {
            var activeVersionToSet = (ElasticsearchVersion)((int) activeVersion);
            var res = CanaryDeploymentFactory.Instance.GetElasticsearchCanaryDeploymentManager().SetElasticsearchActiveVersion(groupId, activeVersionToSet);
            return res.IsOkStatusCode();
        }

        public bool DeleteElasticsearchCanaryDeploymentConfiguration(int groupId)
        {
            var res = CanaryDeploymentFactory.Instance.GetElasticsearchCanaryDeploymentManager().DeleteCanaryDeploymentConfiguration(groupId);
            return res.IsOkStatusCode();
        }
    }
}
