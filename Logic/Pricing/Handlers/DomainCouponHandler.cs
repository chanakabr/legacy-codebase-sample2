using ApiLogic.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core.Pricing.Handlers
{
    // TODO SHIR - CRUD changes
    // TODO ANAT(BEO-6931) - implement ALL relevant methods for DomainCouponHandler
    public class DomainCouponHandler : ICrudHandler<DomainCoupon> // BaseCrudHandler<DomainCoupon>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<DomainCouponHandler> lazy = new Lazy<DomainCouponHandler>(() => new DomainCouponHandler());

        public static DomainCouponHandler Instance { get { return lazy.Value; }}
        private DomainCouponHandler() { }
       
        public GenericResponse<DomainCoupon> Add(int groupId, DomainCoupon objectToAdd, Dictionary<string, object> extraParams = null)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<DomainCoupon> Update(int groupId, DomainCoupon objectToUpdate, Dictionary<string, object> extraParams = null)
        {
            throw new NotImplementedException();
        }

        public Status Delete(int groupId, long id)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<DomainCoupon> Get(int groupId, long id)
        {
            throw new NotImplementedException();
        }
    }
}