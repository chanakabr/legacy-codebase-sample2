//using ApiLogic.Pricing.Handlers;
//using ApiObjects;
//using ApiObjects.Pricing;
//using ApiObjects.Response;
//using Core.Pricing;
//using KLogMonitor;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using WebAPI.ClientManagers.Client;
//using WebAPI.Clients;
//using WebAPI.Exceptions;
//using WebAPI.Managers.Models;
//using WebAPI.Managers.Scheme;
//using WebAPI.Models.ConditionalAccess;
//using WebAPI.Models.General;
//using WebAPI.Models.Pricing;
//using WebAPI.Utils;

//namespace WebAPI.Controllers
//{
//    [Service("premiumService")]
//    public class PremiumServiceController : IKalturaController
//    {
//        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

//        /// <summary>
//        /// update existing PriceDetails
//        /// </summary>
//        /// <param name="id">id of priceDetails</param>
//        /// <param name="priceDetails">priceDetails to update</param>
//        /// <returns></returns>
//        [Action("update")]
//        [ApiAuthorize]
//        [Throws(eResponseStatus.InvalidCurrency)]
//        [Throws(eResponseStatus.CountryNotFound)]
//        [Throws(eResponseStatus.PriceDetailsDoesNotExist)]
//        [SchemeArgument("id", MinLong = 1)]
//        static public KalturaPriceDetails Update(long id, KalturaPriceDetails priceDetails)
//        {
//            KalturaPriceDetails result = null;
//            var contextData = KS.GetContextData();
//            priceDetails.ValidateForUpdate();

//            try
//            {
//                priceDetails.Id = (int)id;
//                Func<PriceDetails, GenericResponse<PriceDetails>> updateFunc = (PriceDetails priceDetailsToUpdate) =>
//                        PriceDetailsManager.Instance.Update(contextData, priceDetailsToUpdate);

//                result = ClientUtils.GetResponseFromWS<KalturaPriceDetails, PriceDetails>(priceDetails, updateFunc);
//            }
//            catch (ClientException ex)
//            {
//                ErrorUtils.HandleClientException(ex);
//            }

//            return result;
//        }
//    }
//}