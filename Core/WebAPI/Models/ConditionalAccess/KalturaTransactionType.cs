using System;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public enum KalturaTransactionType
    {  
        ppv,
        subscription,
        collection,
        programAssetGroupOffer
    }
}