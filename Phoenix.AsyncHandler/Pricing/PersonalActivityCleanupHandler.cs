using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using OTT.Lib.Kafka.Extensions;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Logical.PersonalActivityCleanup;
using TVinciShared;
using Result = Phoenix.AsyncHandler.Kafka.Result;

namespace Phoenix.AsyncHandler.Pricing
{
    public class PersonalActivityCleanupHandler: IKafkaMessageHandler<PersonalActivityCleanup>
    {
        private readonly ILogger<PersonalActivityCleanupHandler> _logger;
        private readonly IPersonalActivityCleanupCompletePublisher _publisher;

        public PersonalActivityCleanupHandler(ILogger<PersonalActivityCleanupHandler> logger, IPersonalActivityCleanupCompletePublisher personalActivityCleanupCompletePublisher)
        {
            _logger = logger;
            _publisher = personalActivityCleanupCompletePublisher;
        }
        
        public Task<HandleResult> Handle(ConsumeResult<string, PersonalActivityCleanup> consumeResult)
        {
            var cleanup = consumeResult.GetValue(); 
            if (!cleanup.PartnerId.HasValue || cleanup.RetentionPeriodDays <= 0)
            {
                _logger.LogError("Invalid params. partnerId:[{PartnerId}], retentionPeriodDays:[{RetentionPeriodDays}]",
                    cleanup.PartnerId, cleanup.RetentionPeriodDays);
                _publisher.Publish(cleanup.PartnerId ?? 0, cleanup.Key); // mark cleanup as finished successfully, because we could do nothing with invalid events
                return Task.FromResult(Result.Ok);
            }
            
            long partnerId = cleanup.PartnerId.Value;

            DateTime endDate = DateTime.Now.AddDays(-1 * cleanup.RetentionPeriodDays);

            List<string> errors = new List<string>();
            if (!DAL.ConditionalAccessDAL.Instance.DeletePpvPurchasesThatOutOfRetentionPeriod(partnerId, endDate))
            {
                errors.Add("ppv purchases");
            }
            
            if (!DAL.ConditionalAccessDAL.Instance.DeleteCollectionPurchasesThatOutOfRetentionPeriod(partnerId, endDate))
            {
                errors.Add("collection purchases");
            }
            
            if (!DAL.ConditionalAccessDAL.Instance.DeletePagoPurchasesThatOutOfRetentionPeriod(partnerId, endDate))
            {
                errors.Add("program asset group offer purchases");
            }  
            if (!DAL.ConditionalAccessDAL.Instance.DeleteSubscriptionsPurchasesThatOutOfRetentionPeriod(partnerId, endDate))
            {
                errors.Add("subscriptions purchases");
            }
            if (!DAL.ConditionalAccessDAL.Instance.DeleteSubscriptionsBillingTransactionsThatOutOfRetentionPeriod(partnerId, endDate))
            {
                errors.Add("subscriptions billing transactions");
            }
            
            if (!errors.IsEmpty())
            {
                _logger.LogError("Failed to cleanup. partnerId:[{PartnerId}]. errors in:[{Description}]",
                    cleanup.PartnerId, errors);
                throw new Exception("Failed to cleanup");
            }
            
            _publisher.Publish(partnerId, cleanup.Key);
            return Task.FromResult(Result.Ok);
        }
    }
}
