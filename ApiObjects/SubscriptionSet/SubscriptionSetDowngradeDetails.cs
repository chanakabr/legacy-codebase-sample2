using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.SubscriptionSet
{
    [Serializable]
    public class SubscriptionSetDowngradeDetails : SubscriptionSetModifyDetails
    {

        public double Price { get; set; }

        public string CurrencyCode { get; set; }

        public string CouponCode { get; set; }

        public int PaymentGatewayId { get; set; }

        public int PaymentMethodId { get; set; }

        public string AdapterData { get; set; }

        public DateTime StartDate { get; set; }

        public SubscriptionSetDowngradeDetails()
            : base()
        {
            Price = 0;
            CurrencyCode = string.Empty;
            CouponCode = string.Empty;
            PaymentGatewayId = 0;
            PaymentMethodId = 0;
            AdapterData = string.Empty;
            StartDate = DateTime.MinValue;
        }

        public SubscriptionSetDowngradeDetails(long id, int groupId, string userId, long domainId, long subscriptionId, string previousSubscriptionId, string udid, string userIp, double price,
                                                string currencyCode, string couponCode, int paymentGatewayId, int paymentMethodId, string adapterData, DateTime startDate)
            : base(id, groupId, userId, domainId, subscriptionId, previousSubscriptionId, udid, userIp, SubscriptionSetModifyType.Downgrade)
        {
            Price = price;
            CurrencyCode = currencyCode;
            CouponCode = couponCode;
            PaymentGatewayId = paymentGatewayId;
            PaymentMethodId = paymentMethodId;
            AdapterData = adapterData;
            StartDate = startDate;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.AppendFormat("Price: {0}, ", Price);
            sb.AppendFormat("CurrencyCode: {0}, ", string.IsNullOrEmpty(CurrencyCode) ? string.Empty : CurrencyCode);
            sb.AppendFormat("CouponCode: {0}, ", string.IsNullOrEmpty(CouponCode) ? string.Empty : CouponCode);
            sb.AppendFormat("PaymentGatewayId: {0}, ", PaymentGatewayId);
            sb.AppendFormat("PaymentMethodId: {0}, ", PaymentMethodId);
            sb.AppendFormat("AdapterData: {0}, ", string.IsNullOrEmpty(AdapterData) ? string.Empty : AdapterData);
            sb.AppendFormat("StartDate: {0}, ", StartDate.ToString());

            return sb.ToString();
        }

    }
}
