using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
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
        public TvinciPricing.Price m_oPrice;
        public PriceReason m_PriceReason;

        public SubscriptionsPricesContainer()
        { }

        public void Initialize(string sSubscriptionCode, TvinciPricing.Price oPrice, PriceReason ePriceReason)
        {
            m_sSubscriptionCode = sSubscriptionCode;
            m_oPrice = oPrice;
            m_PriceReason = ePriceReason;
        }

    }

    public class CollectionsPricesContainer
    {
        public string m_sCollectionCode;
        public TvinciPricing.Price m_oPrice;
        public PriceReason m_PriceReason;

        public CollectionsPricesContainer()
        { }

        public void Initialize(string sCollectionCode, TvinciPricing.Price oPrice, PriceReason ePriceReason)
        {
            m_sCollectionCode = sCollectionCode;
            m_oPrice = oPrice;
            m_PriceReason = ePriceReason;
        }

    }

    public class PrePaidPricesContainer
    {
        public string m_sPrePaidCode;
        public TvinciPricing.Price m_oPrice;
        public PriceReason m_PriceReason;

        public PrePaidPricesContainer()
        { }

        public void Initialize(string sPrePaidCode, TvinciPricing.Price oPrice, PriceReason ePriceReason)
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
        public TvinciPricing.Price m_oPrice;
        public TvinciPricing.Price m_oFullPrice;
        public PriceReason m_PriceReason;
        public TvinciPricing.Subscription m_relevantSub;
        public TvinciPricing.Collection m_relevantCol;
        public TvinciPricing.PrePaidModule m_relevantPP;
        public TvinciPricing.LanguageContainer[] m_oPPVDescription;
        public TvinciPricing.CouponsStatus m_couponStatus;
        public string m_sFirstDeviceNameFound;
        public bool m_bCancelWindow;
        public string m_sPurchasedBySiteGuid;
        public int m_lPurchasedMediaFileID;
        public int[] m_lRelatedMediaFileIDs;
        public DateTime? m_dtStartDate;
        public DateTime? m_dtEndDate;
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

        public void Initialize(TvinciPricing.Price oPrice, TvinciPricing.Price oFullPrice, string sPPVModuleCode,
            TvinciPricing.LanguageContainer[] oPPVDescription, PriceReason theReason, TvinciPricing.Subscription relevantSub,
            TvinciPricing.Collection relevantCol, bool bSubscriptionOnly, TvinciPricing.PrePaidModule relevantPP, string sFirstDeviceFound,
            bool bCancelWindow, string purchasedBySiteGuid, int purchasedAsMediaFileID, IEnumerable<int> relatedMediaFileIDs, string productCode, DateTime? dtStartDate = null, DateTime? dtEndDate = null)
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
        }
    }
}
