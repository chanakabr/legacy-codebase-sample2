using ApiLogic.Pricing.Handlers;
using ApiObjects.Pricing;
using ApiObjects.Response;
using System;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.Controllers
{
    [Service("partnerPremiumServices")]
    public class PartnerPremiumServicesController : IKalturaController
    {
        /// <summary>
        /// Returns list of services
        /// </summary>
        /// <returns></returns>
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public KalturaPartnerPremiumServices Get()
        {
            Func<PartnerPremiumServices> getListFunc = () => PartnerPremiumServicesManager.Instance.Get(KS.GetFromRequest().GroupId);
            return ClientUtils.GetResponseFromWS<KalturaPartnerPremiumServices, PartnerPremiumServices>(getListFunc);
        }

        /// <summary>
        /// update partnerPremiumServices
        /// </summary>
        /// <param name="partnerPremiumServices">partnerPremiumServices to update</param>
        /// <returns></returns>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.PremiumServiceDoesNotExist)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public KalturaPartnerPremiumServices Update(KalturaPartnerPremiumServices partnerPremiumServices)
        {
            partnerPremiumServices.ValidateForUpdate();

            Func<PartnerPremiumServices, GenericResponse<PartnerPremiumServices>> updateFunc = req =>
                        PartnerPremiumServicesManager.Instance.Update(KS.GetContextData(), req);
            return ClientUtils.GetResponseFromWS(partnerPremiumServices, updateFunc);
        }
    }
}