using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using AdapaterCommon.Models;
using PGAdapter.Models;

namespace PGAdapter
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AdapterStatus SetConfiguration(int paymentGatewayId, string transactUrl, string statusUrl, string renewUrl, List<KeyValue> connectionSettings, int partnerId, long timeStamp, string signature);

        [OperationContract]
        ConfigurationResponse GetConfiguration(int paymentGatewayId, string intent, List<KeyValue> extra, long timeStamp, string signature);

        [OperationContract]
        TransactionResponse Transact(int paymentGatewayId, string userId, string householdChargeId, double price, string currency, string productId, eTransactionType transactionType, string contentId, string ip,
            string paymentMethodExternalId, string adapterData, long timeStamp, string signature);

        [OperationContract]
        TransactionResponse VerifyPendingTransaction(int paymentGatewayId, string transactionID, long timeStamp, string signature);

        [OperationContract]
        TransactionResponse VerifyTransaction(int paymentGatewayId, string userId, string ip, string productId, string productCode, eTransactionType transactionType,
            string purchaseToken, long timeStamp, string signature, string contentId, string adapterData);

        [OperationContract]
        TransactionResponse ProcessRenewal(int paymentGatewayId, string userId, string productId, string productCode, string transactionId, int gracePeriodMinutes, double price, string currency, string chargeId, string paymentMethodExternalId, long timeStamp, string signature);

        [OperationContract]
        PaymentMethodResponse RemovePaymentMethod(int paymentGatewayId, string householdChargeId, string paymentMethodExternalId, long timeStamp, string signature);

        [OperationContract]
        PaymentMethodResponse RemoveAccount(int paymentGatewayId, string householdChargeId, List<string> paymentMethodExternalIds, long timeStamp, string signature);

        [OperationContract]
        TransactionResponse UnifiedProcessRenewal(int paymentGatewayId, long housholdId, string chargeId, string paymentMethodExternalId, string currency,
                 double totalPrice, List<TransactionProductDetails> renewSubscription, long timeStamp, string signature);
    }
}
