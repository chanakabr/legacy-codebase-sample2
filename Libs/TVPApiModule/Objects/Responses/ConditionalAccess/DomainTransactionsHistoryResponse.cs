using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApiModule.Objects.Responses.Pricing;

namespace TVPApiModule.Objects.Responses.ConditionalAccess
{
    public class DomainTransactionsHistoryResponse
    {
        [JsonProperty(PropertyName = "status")]
        public Status Status;

        [JsonProperty(PropertyName = "transactions_history")]
        public List<TransactionHistoryContainer> TransactionsHistory;

        [JsonProperty(PropertyName = "transactions_count")]
        public int TransactionsCount;


        /// <summary>
        /// Create an instance of thie response type based on the WS_CAS response
        /// </summary>
        /// <param name="source"></param>
        public DomainTransactionsHistoryResponse(TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.DomainTransactionsHistoryResponse source)
        {
            if (source != null)
            {
                if (source.Status != null)
                {
                    this.Status = new Status(source.Status.Code, source.Status.Message);
                }

                if (source.TransactionsHistory != null && source.TransactionsHistory.Count() > 0)
                {
                    TransactionsHistory = new List<TransactionHistoryContainer>();
                    foreach (TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.TransactionHistoryContainer sourceContainer in source.TransactionsHistory)
                    {
                        TransactionsHistory.Add(new TransactionHistoryContainer(sourceContainer));
                    }
                }

                TransactionsCount = source.TransactionsCount;
            }
        }
    }

    public class TransactionHistoryContainer
    {
        [JsonProperty(PropertyName = "user_id")]
        public string SiteGuid;

        [JsonProperty(PropertyName = "user_full_name")]
        public string UserFullName;

        [JsonProperty(PropertyName = "reciept_code")]
        public string RecieptCode;

        [JsonProperty(PropertyName = "purchased_item_name")]
        public string PurchasedItemName;

        [JsonProperty(PropertyName = "purchased_item_code")]
        public string PurchasedItemCode;

        [JsonProperty(PropertyName = "item_type")]
        public BillingItemsType ItemType;

        [JsonProperty(PropertyName = "billing_action")]
        public BillingAction BillingAction;

        [JsonProperty(PropertyName = "price")]
        public Price Price;

        [JsonProperty(PropertyName = "action_date")]
        public DateTime ActionDate;

        [JsonProperty(PropertyName = "start_date")]
        public DateTime StartDate;

        [JsonProperty(PropertyName = "end_date")]
        public DateTime EndDate;

        [JsonProperty(PropertyName = "payment_method")]
        public PaymentMethod PaymentMethod;

        [JsonProperty(PropertyName = "payment_method_extra_details")]
        public string PaymentMethodExtraDetails;

        [JsonProperty(PropertyName = "is_recurring")]
        public bool IsRecurring;

        [JsonProperty(PropertyName = "billing_provider_ref")]
        public Int32 BillingProviderRef;

        [JsonProperty(PropertyName = "purchase_id")]
        public Int32 PurchaseID;

        [JsonProperty(PropertyName = "remarks")]
        public string Remarks;

        public TransactionHistoryContainer(TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.TransactionHistoryContainer source)
        {
            SiteGuid = source.SiteGuid;
            UserFullName = source.UserFullName;
            RecieptCode = source.m_sRecieptCode;
            PurchasedItemName = source.m_sPurchasedItemName;
            PurchasedItemCode = source.m_sPurchasedItemCode;
            switch (source.m_eItemType)
	        {
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingItemsType.PPV:
                    ItemType = BillingItemsType.PPV;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingItemsType.Subscription:
                    ItemType = BillingItemsType.Subscription;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingItemsType.PrePaid:
                    ItemType = BillingItemsType.PrePaid;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingItemsType.PrePaidExpired:
                    ItemType = BillingItemsType.PrePaidExpired;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingItemsType.Collection:
                    ItemType = BillingItemsType.Collection;
                    break;
                default:
                    ItemType = BillingItemsType.Unknown;
                 break;
	        }
            switch (source.m_eBillingAction)
	        {
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingAction.Purchase:
                    BillingAction = Objects.BillingAction.Purchase;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingAction.RenewPayment:
                    BillingAction = Objects.BillingAction.RenewPayment;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingAction.RenewCancledSubscription:
                    BillingAction = Objects.BillingAction.RenewCancledSubscription;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingAction.CancelSubscriptionOrder:
                    BillingAction = Objects.BillingAction.CancelSubscriptionOrder;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingAction.SubscriptionDateChanged:
                    BillingAction = Objects.BillingAction.SubscriptionDateChanged;
                    break;
                default:
                    BillingAction = Objects.BillingAction.Unknown;
                    break;
	        }            
            Price = new Pricing.Price(source.m_Price);
            ActionDate = source.m_dtActionDate;
            StartDate = source.m_dtStartDate;
            EndDate = source.m_dtEndDate;
            switch (source.m_ePaymentMethod)
	        {
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.CreditCard:
                    PaymentMethod = Objects.PaymentMethod.CreditCard;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.SMS:
                    PaymentMethod = Objects.PaymentMethod.SMS;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.PayPal:
                    PaymentMethod = Objects.PaymentMethod.PayPal;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.DebitCard:
                    PaymentMethod = Objects.PaymentMethod.DebitCard;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.Ideal:
                    PaymentMethod = Objects.PaymentMethod.Ideal;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.Incaso:
                    PaymentMethod = Objects.PaymentMethod.Incaso;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.Gift:
                    PaymentMethod = Objects.PaymentMethod.Gift;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.Visa:
                    PaymentMethod = Objects.PaymentMethod.Visa;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.MasterCard:
                    PaymentMethod = Objects.PaymentMethod.MasterCard;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.InApp:
                    PaymentMethod = Objects.PaymentMethod.InApp;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.M1:
                    PaymentMethod = Objects.PaymentMethod.M1;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.ChangeSubscription:
                    PaymentMethod = Objects.PaymentMethod.ChangeSubscription;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PaymentMethod.Offline:
                    PaymentMethod = Objects.PaymentMethod.Offline;
                    break;
                default:
                    PaymentMethod = Objects.PaymentMethod.Unknown;
                    break;
	        }
            PaymentMethodExtraDetails = source.m_sPaymentMethodExtraDetails;
            IsRecurring = source.m_bIsRecurring;
            BillingProviderRef = source.m_nBillingProviderRef;
            PurchaseID = source.m_nPurchaseID;
            Remarks = source.m_sRemarks;
        }
    }
}