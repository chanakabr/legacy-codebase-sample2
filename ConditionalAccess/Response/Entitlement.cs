using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess.Response
{
    public class Entitlement
    {
        public ApiObjects.Response.Status resp { get; set; }
        public List<Entitlements> entitelments { get; set; }

        public Entitlement()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.InternalError, string.Empty);
            entitelments = new List<Entitlements>();
        }

        public Entitlement(ApiObjects.Response.Status resp, List<Entitlements> entitelments)
        {
            this.resp = resp;
            this.entitelments = entitelments;
        }
    }


    public class Entitlements
    {
        public eTransactionType type { get; set; }
        public string entitlementsId { get; set; }
       
        public Int32 currentUses { get; set; }
        public DateTime endDate { get; set; }
        public DateTime currentDate { get; set; }
        public DateTime lastViewDate { get; set; }
        public DateTime purchaseDate { get; set; }
      
        public Int32 purchaseID { get; set; } // sunscription + collection
        public PaymentMethod paymentMethod { get; set; }

        public string deviceUDID { get; set; }
        public string deviceName { get; set; }
        public bool cancelWindow { get; set; }
 
        public Int32 maxUses { get; set; } // subscription + ppv
        public DateTime nextRenewalDate { get; set; } // subscription only
        public bool recurringStatus { get; set; } // subscription only
        public bool isRenewable { get; set; } // subscription only

        public Int32 mediaFileID; // ppv only
       
        public Entitlements(PermittedSubscriptionContainer item)
        {
            this.type = eTransactionType.Subscription;
            this.entitlementsId = item.m_sSubscriptionCode;
            this.currentUses = item.m_nCurrentUses;
            this.endDate = item.m_dEndDate;
            this.currentDate = item.m_dCurrentDate;
            this.lastViewDate = item.m_dLastViewDate;
            this.purchaseDate = item.m_dPurchaseDate;
            this.purchaseID = item.m_nSubscriptionPurchaseID;
            this.paymentMethod = item.m_paymentMethod;
            this.deviceUDID = item.m_sDeviceUDID;
            this.deviceName = item.m_sDeviceName;
            this.cancelWindow = item.m_bCancelWindow;
            this.maxUses = item.m_nMaxUses;
            this.nextRenewalDate = item.m_dNextRenewalDate;
            this.recurringStatus = item.m_bRecurringStatus;
            this.isRenewable = item.m_bIsSubRenewable;

            this.mediaFileID = 0;
        }

        public Entitlements()
        {           
        }
    }
}
