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
        public DomainTransactionsHistoryResponse(Core.ConditionalAccess.DomainTransactionsHistoryResponse source)
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

                    foreach (var sourceContainer in source.TransactionsHistory)
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

        public TransactionHistoryContainer(Core.ConditionalAccess.TransactionHistoryContainer source)
        {
            SiteGuid = source.SiteGuid;
            UserFullName = source.UserFullName;
            RecieptCode = source.m_sRecieptCode;
            PurchasedItemName = source.m_sPurchasedItemName;
            PurchasedItemCode = source.m_sPurchasedItemCode;
            switch (source.m_eItemType)
	        {
                case ApiObjects.ConditionalAccess.BillingItemsType.PPV:
                    ItemType = BillingItemsType.PPV;
                    break;
                case ApiObjects.ConditionalAccess.BillingItemsType.Subscription:
                    ItemType = BillingItemsType.Subscription;
                    break;
                case ApiObjects.ConditionalAccess.BillingItemsType.PrePaid:
                    ItemType = BillingItemsType.PrePaid;
                    break;
                case ApiObjects.ConditionalAccess.BillingItemsType.PrePaidExpired:
                    ItemType = BillingItemsType.PrePaidExpired;
                    break;
                case ApiObjects.ConditionalAccess.BillingItemsType.Collection:
                    ItemType = BillingItemsType.Collection;
                    break;
                default:
                    ItemType = BillingItemsType.Unknown;
                 break;
	        }
            switch (source.m_eBillingAction)
	        {
                case ApiObjects.ConditionalAccess.BillingAction.Purchase:
                    BillingAction = Objects.BillingAction.Purchase;
                    break;
                case ApiObjects.ConditionalAccess.BillingAction.RenewPayment:
                    BillingAction = Objects.BillingAction.RenewPayment;
                    break;
                case ApiObjects.ConditionalAccess.BillingAction.RenewCancledSubscription:
                    BillingAction = Objects.BillingAction.RenewCancledSubscription;
                    break;
                case ApiObjects.ConditionalAccess.BillingAction.CancelSubscriptionOrder:
                    BillingAction = Objects.BillingAction.CancelSubscriptionOrder;
                    break;
                case ApiObjects.ConditionalAccess.BillingAction.SubscriptionDateChanged:
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
                case ApiObjects.Billing.ePaymentMethod.CreditCard:
                    PaymentMethod = Objects.PaymentMethod.CreditCard;
                    break;
                case ApiObjects.Billing.ePaymentMethod.SMS:
                    PaymentMethod = Objects.PaymentMethod.SMS;
                    break;
                case ApiObjects.Billing.ePaymentMethod.PayPal:
                    PaymentMethod = Objects.PaymentMethod.PayPal;
                    break;
                case ApiObjects.Billing.ePaymentMethod.DebitCard:
                    PaymentMethod = Objects.PaymentMethod.DebitCard;
                    break;
                case ApiObjects.Billing.ePaymentMethod.Ideal:
                    PaymentMethod = Objects.PaymentMethod.Ideal;
                    break;
                case ApiObjects.Billing.ePaymentMethod.Incaso:
                    PaymentMethod = Objects.PaymentMethod.Incaso;
                    break;
                case ApiObjects.Billing.ePaymentMethod.Gift:
                    PaymentMethod = Objects.PaymentMethod.Gift;
                    break;
                case ApiObjects.Billing.ePaymentMethod.Visa:
                    PaymentMethod = Objects.PaymentMethod.Visa;
                    break;
                case ApiObjects.Billing.ePaymentMethod.MasterCard:
                    PaymentMethod = Objects.PaymentMethod.MasterCard;
                    break;
                case ApiObjects.Billing.ePaymentMethod.InApp:
                    PaymentMethod = Objects.PaymentMethod.InApp;
                    break;
                case ApiObjects.Billing.ePaymentMethod.M1:
                    PaymentMethod = Objects.PaymentMethod.M1;
                    break;
                case ApiObjects.Billing.ePaymentMethod.ChangeSubscription:
                    PaymentMethod = Objects.PaymentMethod.ChangeSubscription;
                    break;
                case ApiObjects.Billing.ePaymentMethod.Offline:
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