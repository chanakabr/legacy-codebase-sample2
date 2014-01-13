using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class MediaFileItemPricesContainer
    {
        public int m_nMediaFileID { get; set; }
       
        public ItemPriceContainer[] m_oItemPrices { get; set; }
       
        public string m_sProductCode { get; set; }
    }


    public class ItemPriceContainer
    {
        public string m_sPPVModuleCode { get; set; }
            
        public bool m_bSubscriptionOnly { get; set; }

        public Price m_oPrice { get; set; }

        public Price m_oFullPrice { get; set; }

        public PriceReason m_PriceReason { get; set; }

        public Subscription m_relevantSub { get; set; }

        public PrePaidModule m_relevantPP { get; set; }

        public LanguageContainer[] m_oPPVDescription { get; set; }
       
        public CouponsStatus m_couponStatus { get; set; }
    }


    public class Subscription
    {
        public SubscriptionCodeContainer[] m_sCodes { get; set; }

        public System.DateTime m_dStartDate { get; set; }

        public System.DateTime m_dEndDate { get; set; }

        public int[] m_sFileTypes { get; set; }

        public bool m_bIsRecurring { get; set; }

        public int m_nNumberOfRecPeriods { get; set; }

        public PriceCode m_oSubscriptionPriceCode { get; set; }

        public DiscountModule m_oExtDisountModule { get; set; }

        public LanguageContainer[] m_sName { get; set; }

        public UsageModule m_oSubscriptionUsageModule { get; set; }

        public int m_fictivicMediaID { get; set; }

        public long m_Priority { get; set; }

        public string m_ProductCode { get; set; }

        public string m_SubscriptionCode { get; set; }

        public UsageModule[] m_MultiSubscriptionUsageModule { get; set; }

        public int n_GeoCommerceID { get; set; }

        public bool m_bIsInfiniteRecurring { get; set; }

        public PreviewModule m_oPreviewModule { get; set; }
    }

    public class PrePaidModule
    {
        public PriceCode m_PriceCode { get; set; }

        public PriceCode m_CreditValue { get; set; }

        public UsageModule m_UsageModule { get; set; }

        public DiscountModule m_DiscountModule { get; set; }

        public CouponsGroup m_CouponsGroup { get; set; }

        public LanguageContainer[] m_Description { get; set; }

        public int m_ObjectCode { get; set; }

        public string m_Title { get; set; }

        public bool m_isFixedCredit { get; set; }
    }

    public class LanguageContainer
    {
        public string m_sLanguageCode3 { get; set; }
        
        public string m_sValue { get; set; }
    }

    public class SubscriptionCodeContainer
    {
        public string m_sCode { get; set; }
        
        public string m_sName { get; set; }
    }

    public class PriceCode
    {
        public string m_sCode { get; set; }

        public Price m_oPrise { get; set; }

        public int m_nObjectID { get; set; }

        public LanguageContainer[] m_sDescription { get; set; }
    }

    public class DiscountModule
    {
        public double m_dPercent { get; set; }
        
        public RelationTypes m_eTheRelationType { get; set; }

        public System.DateTime m_dStartDate { get; set; }

        public System.DateTime m_dEndDate { get; set; }

        public WhenAlgo m_oWhenAlgo { get; set; }
    }

    public class UsageModule
    {
        public int m_nObjectID { get; set; }

        public string m_sVirtualName { get; set; }

        public int m_nMaxNumberOfViews { get; set; }

        public int m_tsViewLifeCycle { get; set; }

        public int m_tsMaxUsageModuleLifeCycle { get; set; }

        public int m_ext_discount_id { get; set; }

        public int m_internal_discount_id { get; set; }

        public int m_pricing_id { get; set; }

        public int m_coupon_id { get; set; }

        public int m_type { get; set; }

        public int m_subscription_only { get; set; }

        public int m_is_renew { get; set; }

        public int m_num_of_rec_periods { get; set; }
        
        public int m_device_limit_id { get; set; }
    }

    public class PreviewModule
    {
        public long m_nID { get; set; }

        public string m_sName { get; set; }

        public int m_tsFullLifeCycle { get; set; }

        public int m_tsNonRenewPeriod { get; set; }
    }

    public class CouponsGroup
    {
        public DiscountModule m_oDiscountCode { get; set; }

        public string m_sDiscountCode { get; set; }

        public LanguageContainer[] m_sDescription { get; set; }

        public System.DateTime m_dStartDate { get; set; }

        public System.DateTime m_dEndDate { get; set; }

        public int m_nMaxUseCountForCoupon { get; set; }

        public string m_sGroupCode { get; set; }

        public string m_sGroupName { get; set; }

        public int m_nFinancialEntityID { get; set; }

        public int m_nMaxRecurringUsesCountForCoupon { get; set; }
    }

    public class WhenAlgo
    {
        public WhenAlgoType m_eAlgoType { get; set; }

        public int m_nNTimes { get; set; }
    }
        
       

    public enum PriceReason
    {

        /// <remarks/>
        PPVPurchased,

        /// <remarks/>
        Free,

        /// <remarks/>
        ForPurchaseSubscriptionOnly,

        /// <remarks/>
        SubscriptionPurchased,

        /// <remarks/>
        ForPurchase,

        /// <remarks/>
        UnKnown,

        /// <remarks/>
        SubscriptionPurchasedWrongCurrency,

        /// <remarks/>
        PrePaidPurchased,

        /// <remarks/>
        GeoCommerceBlocked,

        /// <remarks/>
        EntitledToPreviewModule,
    }

     public enum CouponsStatus {
        
        /// <remarks/>
        Valid,
        
        /// <remarks/>
        NotExists,
        
        /// <remarks/>
        AllreadyUsed,
        
        /// <remarks/>
        IrrelevantCode,
        
        /// <remarks/>
        Expired,
        
        /// <remarks/>
        NotActive,
    }

     public enum RelationTypes
     {

         /// <remarks/>
         And,

         /// <remarks/>
         Or,
     }

     public enum WhenAlgoType
     {

         /// <remarks/>
         N_FIRST_TIMES,

         /// <remarks/>
         EVERY_N_TIMES,
     }
}
