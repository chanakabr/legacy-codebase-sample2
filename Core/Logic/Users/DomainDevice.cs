using APILogic.ConditionalAccess;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Rules;
using CachingProvider.LayeredCache;
using Core.Users.Cache;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiLogic.Segmentation;

namespace Core.Users
{
    public class DomainDevice : CoreObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int DomainId { get; set; }

        public long DeviceId { get; set; }

        public string Name { get; set; }

        public string Udid { get; set; }

        public DeviceState ActivataionStatus { get; set; }

        public string ActivationToken { get; set; }

        public int DeviceBrandId { get; set; }

        public DateTime ActivatedOn { get; set; }

        public string ExternalId { get; set; }

        public string MacAddress { get; set; }

        public long DeviceFamilyId { get; set; }

        public string Manufacturer { get; set; }

        public long? ManufacturerId { get; set; }

        public string Model { get; set; }
        
        public long? LastActivityTime { get; set; }

        public List<ApiObjects.KeyValuePair> DynamicData { get; set; }

        public static readonly int DeletedDeviceStatus = 2; 

        protected override bool DoInsert()
        {
            switch (ActivataionStatus)
            {
                case DeviceState.Pending:
                    Id = DomainDal.InsertDeviceToDomain(DeviceId, DomainId, GroupId, 3, 3, ActivationToken);
                    break;
                case DeviceState.Activated:
                    Id = DomainDal.InsertDeviceToDomain(DeviceId, DomainId, GroupId, 1, 1, ActivationToken);
                    break;
                case DeviceState.UnActivated:
                    Id = DomainDal.InsertDeviceToDomain(DeviceId, DomainId, GroupId, 0, 1, ActivationToken);
                    break;
                default:
                    Id = 0;
                    break;
            }

            try
            {
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(GroupId, this.DomainId);

                InvalidateDomainDevice();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Insert domain device - Failed to remove domain from cache : m_nDomainID= {0}, UDID= {1}, ex= {2}", this.DomainId, this.Udid, ex);
            }

            var sucsses = Id > 0;

            if (sucsses)
            {
                ApiLogic.Users.Managers.CampaignManager.Instance.PublishTriggerCampaign(GroupId, DomainId, this, ApiService.DomainDevice, ApiAction.Insert);
            }

            return sucsses; 
        }

        protected override bool DoUpdate()
        {
            bool result = false;

            switch (ActivataionStatus)
            {
                case DeviceState.Pending:
                    {
                        result = DomainDal.UpdateDomainsDevicesStatus((int)Id, 3, 3);
                        break;
                    }
                case DeviceState.Activated:
                    {
                        result = DomainDal.UpdateDomainsDevicesStatus((int)Id, 1, 1);
                        break;
                    }
                case DeviceState.UnActivated:
                    {
                        result = DomainDal.UpdateDomainsDevicesStatus((int)Id, 0, 1);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            try
            {
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(GroupId, this.DomainId);

                InvalidateDomainDevice();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Update domain device - Failed to remove domain from cache : m_nDomainID= {0}, UDID= {1}, ex= {2}", this.DomainId, this.Udid, ex);
            }

            return result;
        }

        protected override bool DoDelete()
        {
            bool result = DomainDal.UpdateDomainsDevicesStatus(DomainId, GroupId, Udid, 2, 2);

            // if the first update done successfully - remove domain from cache
            try
            {
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(GroupId, this.DomainId);

                InvalidateDomainDevice();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("RemoveDeviceFromDomain - Failed to remove domain from cache : m_nDomainID= {0}, UDID= {1}, ex= {2}", this.DomainId, this.Udid, ex);
            }

            return result;
        }
        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        public void InvalidateDomainDevice()
        {
            List<string> invalidationKeys = new List<string>()
            {
                LayeredCacheKeys.GetDomainDeviceInvalidationKey(GroupId, DomainId, DeviceId.ToString())
            };

            LayeredCache.Instance.InvalidateKeys(invalidationKeys);
        }

        public TriggerCampaignConditionScope CreateTriggerCampaignConditionScope(ContextData contextData)
        {
            var userSegments = UserSegmentLogic.List(contextData.GroupId, contextData.UserId.ToString(), out int totalCount);

            var conditionScope = new TriggerCampaignConditionScope()
            {
                GroupId = contextData.GroupId,
                UserId = contextData.UserId.ToString(),
                BrandId = this.DeviceBrandId,
                ManufacturerId = this.ManufacturerId,
                Model = this.Model,
                FamilyId = (int)this.DeviceFamilyId,
                Udid = this.Udid,
                FilterBySegments = true,
                SegmentIds = userSegments
            };

            return conditionScope;
        }
    }
}