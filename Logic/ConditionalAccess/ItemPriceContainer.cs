using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using Core.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    public class WSInt32
    {
        public Int32 m_nInt32;
    }

    public class MediaFileItemPricesContainer
    {
        public Int32 m_nMediaFileID;
        public ItemPriceContainer[] m_oItemPrices;
        public string m_sProductCode;

        public MediaFileItemPricesContainer()
        { }

        public void Initialize(Int32 nMediaFileID, ItemPriceContainer[] oItemPrices)
        {
            m_nMediaFileID = nMediaFileID;
            m_oItemPrices = oItemPrices;
            m_sProductCode = string.Empty;
        }
        public void Initialize(Int32 nMediaFileID, ItemPriceContainer[] oItemPrices, string sProductCode)
        {
            m_nMediaFileID = nMediaFileID;
            m_oItemPrices = oItemPrices;
            m_sProductCode = sProductCode;
        }
    }

    public class SubscriptionsPricesContainer
    {
        public string m_sSubscriptionCode;
        public Price m_oPrice;
        public PriceReason m_PriceReason;

        public SubscriptionsPricesContainer()
        { }

        public void Initialize(string sSubscriptionCode, Price oPrice, PriceReason ePriceReason)
        {
            m_sSubscriptionCode = sSubscriptionCode;
            m_oPrice = oPrice;
            m_PriceReason = ePriceReason;
        }

    }

    public class CollectionsPricesContainer
    {
        public string m_sCollectionCode;
        public Price m_oPrice;
        public PriceReason m_PriceReason;

        public CollectionsPricesContainer()
        { }

        public void Initialize(string sCollectionCode, Price oPrice, PriceReason ePriceReason)
        {
            m_sCollectionCode = sCollectionCode;
            m_oPrice = oPrice;
            m_PriceReason = ePriceReason;
        }

    }

    public class PrePaidPricesContainer
    {
        public string m_sPrePaidCode;
        public Price m_oPrice;
        public PriceReason m_PriceReason;

        public PrePaidPricesContainer()
        { }

        public void Initialize(string sPrePaidCode, Price oPrice, PriceReason ePriceReason)
        {
            m_sPrePaidCode = sPrePaidCode;
            m_oPrice = oPrice;
            m_PriceReason = ePriceReason;
        }

    }

    public class ItemPriceContainer
    {
        public string m_sPPVModuleCode;
        public bool m_bSubscriptionOnly;
        public Price m_oPrice;
        public Price m_oFullPrice;
        public PriceReason m_PriceReason;
        public Subscription m_relevantSub;
        public Collection m_relevantCol;
        public PrePaidModule m_relevantPP;
        public LanguageContainer[] m_oPPVDescription;
        public CouponsStatus m_couponStatus;
        public string m_sFirstDeviceNameFound;
        public bool m_bCancelWindow;
        public string m_sPurchasedBySiteGuid;
        public int m_lPurchasedMediaFileID;
        public int[] m_lRelatedMediaFileIDs;
        public DateTime? m_dtStartDate;
        public DateTime? m_dtEndDate;
        public DateTime? m_dtDiscountEndDate;
        public string m_sProductCode;

        public ItemPriceContainer()
        {
            m_relevantSub = null;
            m_relevantCol = null;
            m_PriceReason = PriceReason.UnKnown;
            m_oPrice = null;
            m_oFullPrice = null;
            m_sPPVModuleCode = string.Empty;
            m_oPPVDescription = null;
            m_bSubscriptionOnly = false;
            m_sFirstDeviceNameFound = string.Empty;
            m_relevantPP = null;
            m_bCancelWindow = false;
            m_sPurchasedBySiteGuid = string.Empty;
            m_lPurchasedMediaFileID = 0;
            m_lRelatedMediaFileIDs = new int[0];
            m_dtStartDate = null;
            m_sProductCode = string.Empty;
        }

        public void Initialize(Price oPrice, Price oFullPrice, string sPPVModuleCode,
            LanguageContainer[] oPPVDescription, PriceReason theReason, Subscription relevantSub,
            Collection relevantCol, bool bSubscriptionOnly, PrePaidModule relevantPP, string sFirstDeviceFound,
            bool bCancelWindow, string purchasedBySiteGuid, int purchasedAsMediaFileID, IEnumerable<int> relatedMediaFileIDs, string productCode, DateTime? dtStartDate = null, 
            DateTime? dtEndDate = null, DateTime? dtDiscountEndDate = null)
        {
            m_oPPVDescription = oPPVDescription;
            m_oPrice = oPrice;
            m_oFullPrice = oFullPrice;
            m_sPPVModuleCode = sPPVModuleCode;
            m_PriceReason = theReason;
            m_relevantSub = relevantSub;
            m_relevantCol = relevantCol;
            m_bSubscriptionOnly = bSubscriptionOnly;
            m_relevantPP = relevantPP;
            m_sFirstDeviceNameFound = sFirstDeviceFound;
            m_bCancelWindow = bCancelWindow;
            m_sPurchasedBySiteGuid = purchasedBySiteGuid;
            m_lPurchasedMediaFileID = purchasedAsMediaFileID;
            if (relatedMediaFileIDs != null && relatedMediaFileIDs.Count() > 0)
            {
                m_lRelatedMediaFileIDs = relatedMediaFileIDs.ToArray<int>();
            }
            else
            {
                m_lRelatedMediaFileIDs = new int[0];
            }

            m_dtStartDate = dtStartDate;
            m_dtEndDate = dtEndDate;
            m_sProductCode = productCode;
            m_dtDiscountEndDate = dtDiscountEndDate;
        }
    }
}
