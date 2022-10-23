using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ApiObjects.CanaryDeployment.Microservices;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using CouchbaseManager;
using Phx.Lib.Log;
using RedisManager;

namespace CanaryDeploymentManager
{
    public interface IMicroservicesCanaryDeploymentManager
    {
        GenericResponse<MicroservicesCanaryDeploymentConfiguration> GetGroupConfiguration(int groupId);
        Status DeleteGroupConfiguration(int groupId);
        Status SetRoutingAction(int groupId, CanaryDeploymentRoutingAction routingAction, MicroservicesCanaryDeploymentRoutingService routingService);
        Status SetAllRoutingActionsToMs(int groupId, bool enableMs);       
        Status SetAllMigrationEventsStatus(int groupId, bool status);
        Status EnableMigrationEvent(int groupId, CanaryDeploymentMigrationEvent migrationEvent);
        Status DisableMigrationEvent(int groupId, CanaryDeploymentMigrationEvent migrationEvent);
        bool IsDataOwnershipFlagEnabled(int groupId, CanaryDeploymentDataOwnershipEnum ownershipFlag);
        bool IsEnabledMigrationEvent(int groupId, CanaryDeploymentMigrationEvent migrationEvent);
    }

    public class MicroservicesMicroservicesCanaryDeploymentManager : IMicroservicesCanaryDeploymentManager
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        private static readonly Lazy<MicroservicesMicroservicesCanaryDeploymentManager> lazy = new Lazy<MicroservicesMicroservicesCanaryDeploymentManager>(() => new MicroservicesMicroservicesCanaryDeploymentManager(LayeredCache.Instance, RedisClientManager.PersistenceInstance,
                                                                                                                                        new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS)),
                                                                                                                                        LazyThreadSafetyMode.PublicationOnly);

        internal static MicroservicesMicroservicesCanaryDeploymentManager Instance { get { return lazy.Value; } }

        private readonly ILayeredCache _layeredCache;
        private readonly RedisClientManager _redisCM;
        private readonly CouchbaseManager.CouchbaseManager _cbManager;


        private MicroservicesMicroservicesCanaryDeploymentManager(ILayeredCache layeredCache, RedisClientManager redisCM, CouchbaseManager.CouchbaseManager cbManager)
        {
            _layeredCache = layeredCache;
            _redisCM = redisCM;
            _cbManager = cbManager;
        }

        public GenericResponse<MicroservicesCanaryDeploymentConfiguration> GetGroupConfiguration(int groupId)
        {
            GenericResponse<MicroservicesCanaryDeploymentConfiguration> res = new GenericResponse<MicroservicesCanaryDeploymentConfiguration>() { Object = null };
            try
            {
                if (groupId == 0)
                {
                    var cdc = GetCanaryDeploymentConfigurationFromLayeredCache(0, false);
                    {
                        res.Object = cdc;
                        res.SetStatus(eResponseStatus.OK);
                    }
                }
                else if (_cbManager.IsKeyExists(GetCanaryConfigurationKey(groupId)))
                {
                    // don't check if key exists again in GetCanaryDeploymentConfigurationFromLayeredCache method
                    MicroservicesCanaryDeploymentConfiguration cdc = GetCanaryDeploymentConfigurationFromLayeredCache(groupId, false);
                    if (cdc != null)
                    {
                        res.Object = cdc;
                        res.SetStatus(eResponseStatus.OK);
                    }
                }
                else
                {
                    res.SetStatus(eResponseStatus.GroupCanaryDeploymentConfigurationNotSetYet, "Group canary deployment configuration not set yet, check groupId 0 instead to see default value");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get canary deployment configuration for groupId {groupId}", ex);
            }

            return res;
        }

        public Status DeleteGroupConfiguration(int groupId)
        {
            Status res = new Status(eResponseStatus.FailedToDeleteGroupCanaryDeploymentConfiguration, $"Failed To delete canary deployment configuration for groupId {groupId}");
            try
            {
                string key = GetCanaryConfigurationKey(groupId);
                // if key doesn't exist or it was deleted successfully
                if ((!_cbManager.IsKeyExists(key) || _cbManager.Remove(key)) && (!_redisCM.IsKeyExists(key) || _redisCM.Delete(key)))
                {
                    SetInvalidationKey(groupId);
                    res.Set(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed to delete canary deployment configuration for groupId {groupId}", ex);
            }

            return res;
        }

        public Status SetRoutingAction(int groupId, CanaryDeploymentRoutingAction routingAction, MicroservicesCanaryDeploymentRoutingService routingService)
        {
            Status res = new Status(eResponseStatus.Error, $"Failed To set canary deployment routing action {routingAction} to {routingService} for groupId {groupId}");
            try
            {
                res = ValidateAndSetRoutingAction(groupId, routingAction, routingService);
            }
            catch (Exception ex)
            {
                log.Error($"Failed To set canary deployment routing action {routingAction} to {routingService} for groupId {groupId}", ex);
            }

            return res;
        }

        public Status SetAllRoutingActionsToMs(int groupId, bool enableMs)
        {
            Status res = new Status(eResponseStatus.FailedToSetAllRoutingActions, $"Failed To set all canary deployment routing actions with enableMs set to {enableMs} for groupId {groupId}");
            try
            {
                foreach (CanaryDeploymentRoutingAction routingAction in CanaryDeploymentRoutingActionLists.RoutingActionsToMsRoutingService.Keys)
                {
                    // if enableMs is true take routingService from RoutingActionsToMsRoutingService, otherwise set to phoenix
                    MicroservicesCanaryDeploymentRoutingService routingService = enableMs
                        ? CanaryDeploymentRoutingActionLists.RoutingActionsToMsRoutingService[routingAction]
                        : MicroservicesCanaryDeploymentRoutingService.Phoenix;
                    res = ValidateAndSetRoutingAction(groupId, routingAction, routingService);
                    if (res.Code != (int)eResponseStatus.OK)
                    {
                        return res;
                    }
                }

                // if we didn't fail on all routing actions, set res status to OK
                res.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"Failed To set all canary deployment routing actions with enableMs set to {enableMs} for groupId {groupId}", ex);
            }

            return res;
        }

        public Status SetAllMigrationEventsStatus(int groupId, bool status)
        {
            Status res = new Status(eResponseStatus.FailedToSetAllGroupCanaryDeploymentMigrationEventsStatus, $"Failed to set status {status} to all canary deployment migration events for groupId {groupId}");
            try
            {
                string key = GetCanaryConfigurationKey(groupId);
                MicroservicesCanaryDeploymentConfiguration cdc = GetCanaryDeploymentConfiguration(groupId);
                if (cdc != null)
                {
                    if (SetStatusForAllCanaryDeploymentConfigurationMigrationEvents(groupId, status))
                    {
                        res.Set(eResponseStatus.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed to set status {status} to all canary deployment migration events for groupId {groupId}", ex);
            }

            return res;
        }

        public Status EnableMigrationEvent(int groupId, CanaryDeploymentMigrationEvent migrationEvent)
        {
            Status res = new Status(eResponseStatus.FailedToEnableCanaryDeploymentMigrationEvent, $"Failed To enable canary deployment migration event {migrationEvent} for groupId {groupId}");
            try
            {
                if (SetMigrationEventValue(groupId, migrationEvent, true))
                {
                    res.Set(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed To enable canary deployment migration event {migrationEvent} for groupId {groupId}", ex);
            }

            return res;
        }

        public Status DisableMigrationEvent(int groupId, CanaryDeploymentMigrationEvent migrationEvent)
        {
            Status res = new Status(eResponseStatus.FailedToDisableCanaryDeploymentMigrationEvent, $"Failed To disable canary deployment migration event {migrationEvent} for groupId {groupId}");
            try
            {
                if (SetMigrationEventValue(groupId, migrationEvent, false))
                {
                    res.Set(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed To disable canary deployment migration event {migrationEvent} for groupId {groupId}", ex);
            }

            return res;
        }

        public bool IsDataOwnershipFlagEnabled(int groupId, CanaryDeploymentDataOwnershipEnum ownershipFlag)
        {
            try
            {
                var cdc = GetCanaryDeploymentConfigurationFromLayeredCache(groupId);
                if (cdc != null)
                {
                    switch (ownershipFlag)
                    {
                        case CanaryDeploymentDataOwnershipEnum.AuthenticationUserLoginHistory:
                            return cdc.DataOwnership.AuthenticationMsOwnership.UserLoginHistory;
                        case CanaryDeploymentDataOwnershipEnum.AuthenticationDeviceLoginHistory:
                            return cdc.DataOwnership.AuthenticationMsOwnership.DeviceLoginHistory;
                        case CanaryDeploymentDataOwnershipEnum.AuthenticationSSOAdapterProfiles:
                            return cdc.DataOwnership.AuthenticationMsOwnership.SSOAdapterProfiles;
                        case CanaryDeploymentDataOwnershipEnum.AuthenticationRefreshToken:
                            return cdc.DataOwnership.AuthenticationMsOwnership.RefreshToken;
                        case CanaryDeploymentDataOwnershipEnum.AuthenticationDeviceLoginPin:
                            return cdc.DataOwnership.AuthenticationMsOwnership.DeviceLoginPin;
                        case CanaryDeploymentDataOwnershipEnum.AuthenticationSessionRevocation:
                            return cdc.DataOwnership.AuthenticationMsOwnership.SessionRevocation;
                        case CanaryDeploymentDataOwnershipEnum.Segmentation:
                            return cdc.DataOwnership.SegmentationMsOwnership.Segmentation;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(ownershipFlag), ownershipFlag, null);
                    }
                   
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed checking if ownership flag {ownershipFlag} is enabled for groupId {groupId}", ex);
            }

            return false;
        }

        public bool IsEnabledMigrationEvent(int groupId, CanaryDeploymentMigrationEvent migrationEvent)
        {
            bool res = false;
            try
            {
                MicroservicesCanaryDeploymentConfiguration cdc = GetCanaryDeploymentConfigurationFromLayeredCache(groupId);
                if (cdc != null)
                {
                    switch (migrationEvent)
                    {
                        case CanaryDeploymentMigrationEvent.AppToken:
                            res = cdc.MigrationEvents.AppToken;
                            break;
                        case CanaryDeploymentMigrationEvent.RefreshSession:
                            res = cdc.MigrationEvents.RefreshToken;
                            break;
                        case CanaryDeploymentMigrationEvent.DevicePinCode:
                            res = cdc.MigrationEvents.DevicePinCode;
                            break;
                        case CanaryDeploymentMigrationEvent.SessionRevocation:
                            res = cdc.MigrationEvents.SessionRevocation;
                            break;
                        case CanaryDeploymentMigrationEvent.UserLoginHistory:
                            res = cdc.MigrationEvents.UserLoginHistory;
                            break;
                        case CanaryDeploymentMigrationEvent.DeviceLoginHistory:
                            res = cdc.MigrationEvents.DeviceLoginHistory;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed checking if migration event {migrationEvent} is enabled for groupId {groupId}", ex);
            }

            return res;
        }

        #region private methods

        private MicroservicesCanaryDeploymentConfiguration GetCanaryDeploymentConfigurationFromLayeredCache(int groupId, bool shouldCheckIfGroupKeyExists = true)
        {
            MicroservicesCanaryDeploymentConfiguration cdc = null;
            var invalidationKeys = new List<string>() { LayeredCacheKeys.GetMicroserviceCanaryDeploymentConfigurationInvalidationKey(groupId) };

            // group id 0 also affects other group ids so we should consider its invalidation key as well
            if (groupId != 0)
            {
                invalidationKeys.Add(LayeredCacheKeys.GetMicroserviceCanaryDeploymentConfigurationInvalidationKey(0));
            }

            if (!_layeredCache.Get<MicroservicesCanaryDeploymentConfiguration>(
                    LayeredCacheKeys.GetMicroservicesCanaryDeploymentConfigurationKey(groupId), ref cdc, GetGroupConfigurationFromSource,
                    new Dictionary<string, object>() { 
                        { "groupId", groupId }, 
                        { "shouldCheckIfGroupKeyExists", shouldCheckIfGroupKeyExists } },
                    groupId,
                    LayeredCacheConfigNames.GET_MICROSERVICES_CANARY_CONFIGURATION, invalidationKeys
                    ))
            {
                log.Error($"Failed getting canary deployment configuration from layeredCache");
            }

            return cdc;
        }

        private Tuple<MicroservicesCanaryDeploymentConfiguration, bool> GetGroupConfigurationFromSource(Dictionary<string, object> funcParams)
        {
            int? groupId = null;
            bool? shouldCheckIfGroupKeyExists = null;
            MicroservicesCanaryDeploymentConfiguration cdc = null;
            try
            {                
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("shouldCheckIfGroupKeyExists"))
                {
                    groupId = funcParams["groupId"] as int?;
                    shouldCheckIfGroupKeyExists = funcParams["shouldCheckIfGroupKeyExists"] as bool?;
                    if (groupId.HasValue && groupId.Value >= 0 && shouldCheckIfGroupKeyExists.HasValue)
                    {
                        if (!shouldCheckIfGroupKeyExists.Value ||  (shouldCheckIfGroupKeyExists.Value && _cbManager.IsKeyExists(GetCanaryConfigurationKey(groupId.Value))))
                        {
                            // don't check if key exists again in GetCanaryDeploymentConfiguration method
                            // in special case where groupId==0 we do not check if the key exists so we will ask the actual get method to do that
                            // otherwise we already did the check so no point in doing that again
                            cdc = GetCanaryDeploymentConfiguration(groupId.Value, shouldCheckIfGroupKeyExists: groupId == 0);
                        }
                        // fall-back is to use groupId = 0
                        else
                        {
                            cdc = GetCanaryDeploymentConfiguration(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupConfigurationFromSource for groupId {0}", groupId.HasValue ? groupId.Value : -1), ex);
            }

            return new Tuple<MicroservicesCanaryDeploymentConfiguration, bool>(cdc, cdc != null);
        }

        private bool SetStatusForAllCanaryDeploymentConfigurationMigrationEvents(int groupId, bool status)
        {
            bool res = false;
            try
            {
                string key = GetCanaryConfigurationKey(groupId);
                MicroservicesCanaryDeploymentConfiguration cdc = GetCanaryDeploymentConfiguration(groupId);
                if (cdc != null)
                {
                    // set status for all migration events
                    cdc.MigrationEvents = new MicroservicesCanaryDeploymentMigrationEvents(status);
                    res = SetCanaryDeploymentConfiguration(groupId, cdc);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed SetStatusForAllCanaryDeploymentConfigurationMigrationEvents for groupId {groupId} and status {status}", ex);
            }

            return res;
        }

        private Status ValidateAndSetRoutingAction(int groupId, CanaryDeploymentRoutingAction routingAction, MicroservicesCanaryDeploymentRoutingService routingService)
        {            
            MicroservicesCanaryDeploymentConfiguration cdc = GetCanaryDeploymentConfiguration(groupId);
            bool isRoutingToPhoenixRestProxy = routingService == MicroservicesCanaryDeploymentRoutingService.PhoenixRestProxy;

            // continue only if we are setting routing back to Phoenix or if it's valid to be routed to phoenix rest proxy
            Status validateStatus = ValidateRoutingAction(cdc, routingAction, routingService);
            if (validateStatus.Code != (int)eResponseStatus.OK)
            {
                return validateStatus;
            }

            Status res = new Status(eResponseStatus.Error, "Failed updating CanaryDeploymentConfiguration");
            List<string> apisToRoute = new List<string>();
            switch (routingAction)
            {
                case CanaryDeploymentRoutingAction.AppTokenController:
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.AppTokenControllerRouting);
                    break;
                case CanaryDeploymentRoutingAction.UserLoginPinController:
                    cdc.DataOwnership.AuthenticationMsOwnership.UserLoginHistory = isRoutingToPhoenixRestProxy;
                    cdc.DataOwnership.AuthenticationMsOwnership.DeviceLoginHistory = isRoutingToPhoenixRestProxy;
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.UserLoginPinControllerRouting);
                    break;
                case CanaryDeploymentRoutingAction.SsoAdapterProfileController:
                    cdc.DataOwnership.AuthenticationMsOwnership.SSOAdapterProfiles = isRoutingToPhoenixRestProxy;
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.SsoAdapterProfileControllerRouting);
                    break;
                case CanaryDeploymentRoutingAction.SessionController:
                    cdc.DataOwnership.AuthenticationMsOwnership.UserLoginHistory = isRoutingToPhoenixRestProxy;
                    cdc.DataOwnership.AuthenticationMsOwnership.DeviceLoginHistory = isRoutingToPhoenixRestProxy;
                    cdc.DataOwnership.AuthenticationMsOwnership.SessionRevocation = isRoutingToPhoenixRestProxy;
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.SessionControllerRouting);
                    break;
                case CanaryDeploymentRoutingAction.HouseHoldDevicePinActions:
                    cdc.DataOwnership.AuthenticationMsOwnership.DeviceLoginPin = isRoutingToPhoenixRestProxy;
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.HouseHoldDevicePinActionsRouting);
                    break;
                case CanaryDeploymentRoutingAction.RefreshSession:
                    cdc.DataOwnership.AuthenticationMsOwnership.RefreshToken = isRoutingToPhoenixRestProxy;
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.RefreshSessionRouting);
                    break;
                case CanaryDeploymentRoutingAction.Login:
                    cdc.DataOwnership.AuthenticationMsOwnership.UserLoginHistory = isRoutingToPhoenixRestProxy;
                    cdc.DataOwnership.AuthenticationMsOwnership.DeviceLoginHistory = isRoutingToPhoenixRestProxy;
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.LoginRouting);
                    break;
                case CanaryDeploymentRoutingAction.Logout:
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.LogoutRouting);
                    break;
                case CanaryDeploymentRoutingAction.AnonymousLogin:
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.AnonymousLoginRouting);
                    break;
                case CanaryDeploymentRoutingAction.MultiRequestController:
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.MultiRequestControllerRouting);
                    break;
                case CanaryDeploymentRoutingAction.HouseholdUser:
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.HouseholdUserRouting);
                    break;
                case CanaryDeploymentRoutingAction.PlaybackController:
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.PlaybackControllerRouting);
                    break;
                case CanaryDeploymentRoutingAction.Segmentation:
                    cdc.DataOwnership.SegmentationMsOwnership.Segmentation = isRoutingToPhoenixRestProxy;
                    apisToRoute.AddRange(CanaryDeploymentRoutingActionLists.SegmentationRouting);
                    break;
                default:
                    break;
            }

            if (cdc.RoutingConfiguration == null)
                cdc.RoutingConfiguration = new Dictionary<string, MicroservicesCanaryDeploymentRoutingService>(StringComparer.OrdinalIgnoreCase);

            foreach (string api in apisToRoute)
            {
                cdc.RoutingConfiguration[api] = routingService;
            }

            if (SetCanaryDeploymentConfiguration(groupId, cdc))
            {
                res.Set(eResponseStatus.OK);
            }

            return res;
        }

        private Status ValidateRoutingAction(MicroservicesCanaryDeploymentConfiguration cdc,
            CanaryDeploymentRoutingAction routingAction, 
            MicroservicesCanaryDeploymentRoutingService routingService)
        {
            Status res = new Status(eResponseStatus.Error, "Failed validating routing action can be sent to phoenix proxy");
            Status okStatus = new Status(eResponseStatus.OK);

            var expectedService = CanaryDeploymentRoutingActionLists.RoutingActionsToMsRoutingService[routingAction];
            if (routingService != MicroservicesCanaryDeploymentRoutingService.Phoenix && expectedService != routingService)
            {
                return new Status(eResponseStatus.FailedToSetAllRoutingActions, $"{routingAction} could be routed to {expectedService} or Phoenix");
            }

            switch (routingAction)
            {
                case CanaryDeploymentRoutingAction.AppTokenController:
                    res = cdc.MigrationEvents.AppToken ? okStatus : new Status(eResponseStatus.FailedToSetRouteAppTokenController, "AppToken migration event has to be enabled first");
                    break;
                case CanaryDeploymentRoutingAction.UserLoginPinController:
                    res = cdc.MigrationEvents.UserPinCode ? okStatus : new Status(eResponseStatus.FailedToSetRouteUserLoginPinController, "UserPinCode migration event has to be enabled first");
                    break;
                case CanaryDeploymentRoutingAction.SessionController:
                    res = cdc.MigrationEvents.SessionRevocation ? okStatus : new Status(eResponseStatus.FailedToSetRouteSessionController, "SessionRevocation migration event has to be enabled first");
                    break;
                case CanaryDeploymentRoutingAction.HouseHoldDevicePinActions:
                    res = cdc.MigrationEvents.DevicePinCode ? okStatus : new Status(eResponseStatus.FailedToSetRouteHouseHoldDevicePinActions, "DevicePinCode migration event has to be enabled first");
                    break;
                case CanaryDeploymentRoutingAction.RefreshSession:
                    res = cdc.MigrationEvents.RefreshToken ? okStatus : new Status(eResponseStatus.FailedToSetRouteRefreshToken, "RefreshToken migration event has to be enabled first");
                    break;              
                case CanaryDeploymentRoutingAction.Login:
                case CanaryDeploymentRoutingAction.AnonymousLogin:
                // TODO: remove userLoginHistory and DeviceLoginHistory from migration events if we don't want them

                // Currently commenting this out since we decided not to migrate user and device login history
                    //res = cdc.MigrationEvents.UserLoginHistory && cdc.MigrationEvents.DeviceLoginHistory
                    //break;
                // all actions that don't require migration events can continue                              
                case CanaryDeploymentRoutingAction.Logout:
                case CanaryDeploymentRoutingAction.SsoAdapterProfileController:
                    res = okStatus;
                    break;
                case CanaryDeploymentRoutingAction.MultiRequestController:
                    res = okStatus;
                    break;
                case CanaryDeploymentRoutingAction.HouseholdUser:
                    res = okStatus;
                    break;
                case CanaryDeploymentRoutingAction.PlaybackController:
                    res = okStatus;
                    break;
                case CanaryDeploymentRoutingAction.Segmentation:
                    res = okStatus;
                    break;
                default:
                    break;
            }

            return res;
        }

        private bool SetMigrationEventValue(int groupId, CanaryDeploymentMigrationEvent migrationEvent, bool val)
        {
            bool res = false;
            MicroservicesCanaryDeploymentConfiguration cdc = GetCanaryDeploymentConfiguration(groupId);
            if (cdc != null)
            {
                switch (migrationEvent)
                {
                    case CanaryDeploymentMigrationEvent.AppToken:
                        cdc.MigrationEvents.AppToken = val;
                        break;
                    case CanaryDeploymentMigrationEvent.RefreshSession:
                        cdc.MigrationEvents.RefreshToken = val;
                        break;
                    case CanaryDeploymentMigrationEvent.DevicePinCode:
                        cdc.MigrationEvents.DevicePinCode = val;
                        break;
                    case CanaryDeploymentMigrationEvent.SessionRevocation:
                        cdc.MigrationEvents.SessionRevocation = val;
                        break;
                    case CanaryDeploymentMigrationEvent.UserLoginHistory:
                        cdc.MigrationEvents.UserLoginHistory = val;
                        break;
                    case CanaryDeploymentMigrationEvent.DeviceLoginHistory:
                        cdc.MigrationEvents.DeviceLoginHistory = val;
                        break;
                    default:
                        break;
                }

                if (SetCanaryDeploymentConfiguration(groupId, cdc))
                {
                    res = true;
                }
            }

            return res;
        }

        private MicroservicesCanaryDeploymentConfiguration GetCanaryDeploymentConfiguration(int groupId, bool shouldCheckIfGroupKeyExists = true)
        {
            MicroservicesCanaryDeploymentConfiguration cdc = null;
            string key = GetCanaryConfigurationKey(groupId);     
            
            // if group key does not exist (or we shouldn't check key again since it was already found to be missing) then fetch groupId 0 configuration
            if (shouldCheckIfGroupKeyExists && !_cbManager.IsKeyExists(key))
            {
                // if we got here with groupId = 0 then it means no configuration has ever been set and we use code default
                if (groupId == 0)
                    cdc = new MicroservicesCanaryDeploymentConfiguration();
                // otherwise we return groupId 0 configuration
                else
                    cdc = GetCanaryDeploymentConfiguration(0);
            }
            // if group key exists then bring it's configuration
            else
            {
                cdc = _cbManager.Get<MicroservicesCanaryDeploymentConfiguration>(key);
            }

            return cdc;
        }

        private bool SetCanaryDeploymentConfiguration(int groupId, MicroservicesCanaryDeploymentConfiguration cdc)
        {
            bool res = false;
            string key = GetCanaryConfigurationKey(groupId);
            if (_cbManager.Set<MicroservicesCanaryDeploymentConfiguration>(key, cdc, 0) && _redisCM.Set(key, cdc.RoutingConfiguration, 0))
            {
                SetInvalidationKey(groupId);
                res = true;
            }

            return res;
        }

        private string GetCanaryConfigurationKey(int groupId)
        {
            return $"canary_configuration_{groupId}";
        }

        private void SetInvalidationKey(int groupId)
        {
            string invalidationKey = LayeredCacheKeys.GetMicroserviceCanaryDeploymentConfigurationInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.Error($"Failed to set invalidation key for canary deployment configuration with invalidationKey: {invalidationKey}");
            }
        }

        #endregion

    }

}
