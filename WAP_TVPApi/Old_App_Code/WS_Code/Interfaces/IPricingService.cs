using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Context;
using Core.Pricing;
using ApiObjects.Pricing;

namespace TVPApiServices
{
    [ServiceContract]
    public interface IPricingService
    {
        [OperationContract]
        PPVModule GetPPVModuleData(InitializationObject initObj, int ppvCode);

        [OperationContract]
        Subscription[] GetSubscriptionsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID);

        [OperationContract]
        CouponData GetCouponStatus(InitializationObject initObj, string sCouponCode);

        [OperationContract]
        CouponsStatus SetCouponUsed(InitializationObject initObj, string sCouponCode);

        [OperationContract]
        Campaign[] GetCampaignsByType(InitializationObject initObj, CampaignTrigger trigger, bool isAlsoInactive);

        [OperationContract]
        List<Subscription> GetSubscriptionData(InitializationObject initObj, int[] subIDs);

        [OperationContract]
        int[] GetSubscriptionIDsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID);

        [OperationContract]
        List<Subscription>  GetSubscriptionsContainingUserTypes(InitializationObject initObj, int isActive, int[] userTypesIDs);

        [OperationContract]
        Collection GetCollectionData(InitializationObject initObj, string collectionId, string countryCd2, string languageCode3, string deviceName, bool bGetAlsoUnActive);
    }
}
