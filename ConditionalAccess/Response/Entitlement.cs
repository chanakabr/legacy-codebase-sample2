using ApiObjects;
using ApiObjects.Response;
using System;
using System.Collections.Generic;

namespace ConditionalAccess.Response
{
    public class Entitlements
    {
        public ApiObjects.Response.Status status { get; set; }
        public List<Entitlement> entitelments { get; set; }

        public Entitlements()
        {
            status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            entitelments = new List<Entitlement>();
        }

        public Entitlements(ApiObjects.Response.Status status, List<Entitlement> entitelments)
        {
            this.status = status;
            this.entitelments = entitelments;
        }
    }

    public class Entitlement
    {
        public eTransactionType type { get; set; }
        public string entitlementId { get; set; }
        public Int32 currentUses { get; set; }
        public DateTime endDate { get; set; }
        public DateTime currentDate { get; set; }
        public DateTime lastViewDate { get; set; }
        public DateTime purchaseDate { get; set; }
        public Int32 purchaseID { get; set; } // subscription + collection 
        public PaymentMethod paymentMethod { get; set; }
        public string deviceUDID { get; set; }
        public string deviceName { get; set; }
        public bool cancelWindow { get; set; }
        public Int32 maxUses { get; set; } // subscription + ppv
        public DateTime nextRenewalDate { get; set; } // subscription only
        public bool recurringStatus { get; set; } // subscription only
        public bool isRenewable { get; set; } // subscription only
        public bool IsInGracePeriod { get; set; } // subscription only
        public Int32 mediaFileID; // ppv only
        public int mediaID { get; set; } // ppv only

        public Entitlement(PermittedSubscriptionContainer item)
        {
            this.type = eTransactionType.Subscription;
            this.entitlementId = item.m_sSubscriptionCode;
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
            this.IsInGracePeriod = item.m_bIsInGracePeriod;

            this.mediaFileID = 0;
        }

        public Entitlement(PermittedCollectionContainer item)
        {
            this.type = eTransactionType.Collection;
            this.entitlementId = item.m_sCollectionCode;
            this.endDate = item.m_dEndDate;
            this.currentDate = item.m_dCurrentDate;
            this.lastViewDate = item.m_dLastViewDate;
            this.purchaseDate = item.m_dPurchaseDate;
            this.purchaseID = item.m_nCollectionPurchaseID;
            this.paymentMethod = item.m_paymentMethod;
            this.deviceUDID = item.m_sDeviceUDID;
            this.deviceName = item.m_sDeviceName;
            this.cancelWindow = item.m_bCancelWindow;
        }

        public Entitlement(PermittedMediaContainer item)
        {
            this.type = eTransactionType.PPV;
            this.entitlementId = item.PPVCode;
            this.currentUses = item.m_nCurrentUses;
            this.endDate = item.m_dEndDate;
            this.currentDate = item.m_dCurrentDate;
            this.lastViewDate = item.m_dLastViewDate;
            this.purchaseDate = item.m_dPurchaseDate;
            this.purchaseID = (int)item.PurchaseID;
            this.paymentMethod = item.m_purchaseMethod;
            this.deviceUDID = item.m_sDeviceUDID;
            this.deviceName = item.m_sDeviceName;
            this.cancelWindow = item.m_bCancelWindow;
            this.maxUses = item.m_nMaxUses;
            this.mediaFileID = item.m_nMediaFileID;
            this.mediaID = item.m_nMediaID;
        }
        
        public Entitlement()
        {
        }
    }
}
