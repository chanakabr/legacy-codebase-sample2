using ApiLogic.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;

namespace Core.Pricing.Handlers
{
    // TODO ANAT(BEO-6931) - implement ALL relevant methods for DomainCouponHandler
    public class DomainCouponHandler : ICrudHandler<DomainCoupon>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public GenericResponse<DomainCoupon> Add(int groupId, DomainCoupon objectToAdd)
        {
            var response = new GenericResponse<DomainCoupon>();
            
            try
            {
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in DomainCoupon. groupId:{0}, object to add details:{1}.",
                                        groupId, objectToAdd), ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        public GenericResponse<DomainCoupon> Update(int groupId, DomainCoupon objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public Status Delete(long id)
        {
            throw new NotImplementedException();
        }
    }
}