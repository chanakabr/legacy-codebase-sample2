using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class UsageModule
    {
        public UsageModule()
        {
            m_nObjectID = 0;
            m_sVirtualName = "";
            m_nMaxNumberOfViews = 1;
            m_tsViewLifeCycle = 86400;
            m_tsMaxUsageModuleLifeCycle = 86400;
            m_ext_discount_id = 0;
            m_internal_discount_id = 0;
            m_pricing_id = 0;
            m_coupon_id = 0;
            m_type = 0;
            m_subscription_only = 0;
            m_is_renew = 0;
            m_num_of_rec_periods = 0;
            m_device_limit_id = 0;

            m_bWaiver = false;
            m_nWaiverPeriod = 0;
            m_bIsOfflinePlayBack = false;
        }


      
        public void Initialize(Int32 nMaxNumberOfViews, Int32 tsViewLifeCycle, Int32 tsMaxUsageModuleLifeCycle, Int32 nObjectID, string sVirtualName, 
            bool bWaiver = false, int nWaiverPeriod = 0, bool bIsOfflinePlayback = false)
        {
            m_nObjectID = nObjectID;
            m_sVirtualName = sVirtualName;
            m_nMaxNumberOfViews = nMaxNumberOfViews;
            m_tsViewLifeCycle = tsViewLifeCycle;
            m_tsMaxUsageModuleLifeCycle = tsMaxUsageModuleLifeCycle;
            m_bWaiver = bWaiver;
            m_nWaiverPeriod = nWaiverPeriod;

            m_bIsOfflinePlayBack = bIsOfflinePlayback;
        }

        public void Initialize(Int32 nMaxNumberOfViews, Int32 tsViewLifeCycle, Int32 tsMaxUsageModuleLifeCycle, Int32 nObjectID, string sVirtualName,
            Int32 next_discount_id, Int32 ninternal_discount_id, Int32 npricing_id, Int32 ncoupon_id, Int32 ntype, Int32 nsubscription_only, Int32 nis_renew, 
            Int32 nnum_of_rec_periods, Int32 ndevice_limit_id, bool bWaiver = false, int nWaiverPeriod = 0, bool bIsOfflinePlayback = false)
        {                                                                                                                                            
            m_nObjectID = nObjectID;                                                                                                                 
            m_sVirtualName = sVirtualName;                                                                                                          
            m_nMaxNumberOfViews = nMaxNumberOfViews;                                                                                                                                                      
            m_tsViewLifeCycle = tsViewLifeCycle;                                                                                                    
            m_tsMaxUsageModuleLifeCycle = tsMaxUsageModuleLifeCycle;
            m_ext_discount_id = next_discount_id;
            m_internal_discount_id = ninternal_discount_id;
            m_pricing_id = npricing_id;
            m_coupon_id = ncoupon_id;
            m_type = ntype;
            m_subscription_only = nsubscription_only;
            m_is_renew = nis_renew;
            m_num_of_rec_periods = nnum_of_rec_periods;
            m_device_limit_id = ndevice_limit_id;

            m_bWaiver = bWaiver;
            m_nWaiverPeriod = nWaiverPeriod;

            m_bIsOfflinePlayBack = bIsOfflinePlayback;
        }

        public Int32 m_nObjectID;
        public string m_sVirtualName;
        public Int32 m_nMaxNumberOfViews;
        public Int32 m_tsViewLifeCycle;
        public Int32 
            m_tsMaxUsageModuleLifeCycle;
        //new paramter supported multi usage module
        public Int32 m_ext_discount_id;
        public Int32 m_internal_discount_id;
        public Int32 m_pricing_id;
        public Int32 m_coupon_id;
        public Int32 m_type;
        public Int32 m_subscription_only;
        public Int32 m_is_renew;
        public Int32 m_num_of_rec_periods;
        public Int32 m_device_limit_id;

        //Regulation cancelation
        public bool m_bWaiver;       //  indicating whether the cancellation is subject to be waivered
        public int m_nWaiverPeriod; //	Cancellation Window  - timespan variable indicating the timeframe from the transaction in which the transaction may be cancelled

        public bool m_bIsOfflinePlayBack;

    }
}
