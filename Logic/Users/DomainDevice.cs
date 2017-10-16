using ApiObjects;
using CachingProvider.LayeredCache;
using Core.Users;
using Core.Users.Cache;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Users
{
    public class DomainDevice : CoreObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int DomainId { get; set; }

        public int DeviceId { get; set; }

        public string Name { get; set; }

        public string Udid { get; set; }

        public DeviceState ActivataionStatus { get; set; }

        public string ActivationToken { get; set; }

        public int DeviceBrandId { get; set; }

        public DateTime ActivatedOn { get; set; }

        public long DeviceFamilyId
        {
            get;
            set;
        }

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
                oDomainCache.RemoveDomain(this.DomainId);

                InvalidateDomainDevice();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Insert domain device - Failed to remove domain from cache : m_nDomainID= {0}, UDID= {1}, ex= {2}", this.DomainId, this.Udid, ex);
            }

            return Id > 0;
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
                oDomainCache.RemoveDomain(this.DomainId);

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
                oDomainCache.RemoveDomain(this.DomainId);

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
                    string.Format("invalidationKey_domain_{0}_device_{1}", this.DomainId, this.DeviceId)
                };

            LayeredCache.Instance.InvalidateKeys(invalidationKeys);
        }
    }
}
