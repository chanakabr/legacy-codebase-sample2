using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Logical.PersonalActivityCleanup;
using Phoenix.Generated.Api.Events.Logical.PersonalActivityCleanupComplete;
using TVinciShared;
using Result = Phoenix.AsyncHandler.Kafka.Result;

namespace Phoenix.AsyncHandler.Pricing
{
    public class PersonalActivityCleanupHandler: IHandler<PersonalActivityCleanup>
    {
        private readonly ILogger<PersonalActivityCleanupHandler> _logger;
        private readonly IPersonalActivityCleanupCompletePublisher _publisher;

        public PersonalActivityCleanupHandler(ILogger<PersonalActivityCleanupHandler> logger, IPersonalActivityCleanupCompletePublisher personalActivityCleanupCompletePublisher)
        {
            _logger = logger;
            _publisher = personalActivityCleanupCompletePublisher;
        }
        
        public HandleResult Handle(ConsumeResult<string, PersonalActivityCleanup> consumeResult)
        {
            if (!consumeResult.Result.Message.Value.PartnerId.HasValue ||
               !consumeResult.Result.Message.Value.RetentionPeriodDays.HasValue ||
               consumeResult.Result.Message.Value.RetentionPeriodDays.Value == 0)
            {
                _logger.LogError("Invalid params");
                _publisher.Publish(consumeResult.Result.Message.Value.PartnerId.HasValue ? consumeResult.Result.Message.Value.PartnerId.Value : 0, PersonalActivityCleanupStatus.Fail, "Invalid params");
                return Result.Ok;
            }
            
            long partnerId = consumeResult.Result.Message.Value.PartnerId.Value;
            long retentionPeriodDays = consumeResult.Result.Message.Value.RetentionPeriodDays.Value;

            DateTime endDate = DateTime.Now.AddDays(-1 * retentionPeriodDays);
            PersonalActivityCleanupStatus status = PersonalActivityCleanupStatus.Success;
            string description = "PersonalActivityCleanup delete all data successfully";
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
                status = PersonalActivityCleanupStatus.Fail;
                description = "failed to delete " + string.Join(", ", errors);
            }
            
            _publisher.Publish(partnerId, status, description);
            return Result.Ok;
        }
    }
}