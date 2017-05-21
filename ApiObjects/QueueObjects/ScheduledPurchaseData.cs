using ApiObjects.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;

namespace ApiObjects
{
    public class ScheduledPurchaseData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_scheduled_purchase";

        private string siteguid;
        private long household;
        private double price;
        private string currency;
        private int contentId;
        private int productId;
        private eTransactionType transactionType;
        private string coupon;
        private string userIp;
        private string deviceName;
        private int paymentGwId;
        private int paymentMethodId;
        private string adapterData;

        public ScheduledPurchaseData(int groupId, string siteguid, long household, double price, string currency, int contentId, int productId, eTransactionType transactionType,
                                    string coupon, string userIp, string deviceName, int paymentGwId, int paymentMethodId, string adapterData, DateTime scheduledPurchaseDate) :
            base(Guid.NewGuid().ToString(), TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.siteguid = siteguid;
            this.household = household;
            this.price = price;
            this.currency = currency;
            this.contentId = contentId;
            this.productId = productId;
            this.transactionType = transactionType;
            this.coupon = coupon;
            this.userIp = userIp;
            this.deviceName = deviceName;
            this.paymentGwId = paymentGwId;
            this.paymentMethodId = paymentMethodId;
            this.adapterData = adapterData;
            this.ETA = scheduledPurchaseDate;

            this.args = new List<object>()
            {
                groupId,
                siteguid,
                household,
                price,
                currency,
                contentId,
                productId,
                transactionType,
                coupon,
                userIp,
                deviceName,
                paymentGwId,
                paymentMethodId,
                adapterData,
                base.RequestId
            };
        }
    }
}
