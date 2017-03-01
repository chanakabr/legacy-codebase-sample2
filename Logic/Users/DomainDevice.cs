using ApiObjects;
using Core.Users;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Users
{
    public class DomainDevice : CoreObject
    {

        public int DomainId { get; set; }

        public int DeviceId { get; set; }

        public string Name { get; set; }

        public string Udid { get; set; }

        public DeviceState ActivataionStatus { get; set; }

        public string ActivationToken { get; set; }

        public int DeviceBrandId { get; set; }

        public DateTime ActivatedOn { get; set; }

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
            return Id > 0;
        }

        protected override bool DoUpdate()
        {
            switch (ActivataionStatus)
            {
                case DeviceState.Pending:
                    return DomainDal.UpdateDomainsDevicesStatus((int)Id, 3, 3);
                case DeviceState.Activated:
                    return DomainDal.UpdateDomainsDevicesStatus((int)Id, 1, 1);
                case DeviceState.UnActivated:
                    return DomainDal.UpdateDomainsDevicesStatus((int)Id, 0, 1);
                default:
                    return false;
            }
        }

        protected override bool DoDelete()
        {
            return DomainDal.UpdateDomainsDevicesStatus(DomainId, GroupId, Udid, 2, 2);
        }

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }
    }
}
