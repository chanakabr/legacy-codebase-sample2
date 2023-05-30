using Core.Users;
using Core.Users.Cache;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Crud.Household;

namespace Phoenix.AsyncHandler
{
    public class HouseholdNpvrAccountHandler : CrudHandler<Household>
    {
        private readonly DomainsCache _domainsCache;
        private readonly ILogger<HouseholdNpvrAccountHandler> _logger;

        public HouseholdNpvrAccountHandler(
            DomainsCache domainsCache,
            ILogger<HouseholdNpvrAccountHandler> logger)
        {
            _domainsCache = domainsCache;
            _logger = logger;
        }

        protected override long GetOperation(Household value) => value.Operation;

        protected override HandleResult Create(ConsumeResult<string, Household> consumeResult)
        {
            var household = consumeResult.GetValue();
            if (household.Source == Source.Phoenix) return Result.Ok;
            
            var groupId = (int)household.PartnerId;
            var domainId = (int)household.Id.Value;
            var dlmId = (int)household.LimitationModuleId.Value;

            var limitationsManager = _domainsCache.GetDLMUnsafe(dlmId, groupId);
            if (limitationsManager == null)
            {
                _logger.LogError("can't find DLM. groupId:[{GroupId}]. dlmId:[{DlmId}]", groupId, dlmId);
                return Result.Ok;
            }

            _logger.LogInformation("attempt to create npvr account for domain:[{DomainId}]", domainId);
            Domain.CreateNpvrAccount(groupId, domainId, limitationsManager.npvrQuotaInSecs);
            
            return Result.Ok;
        }

        protected override HandleResult Update(ConsumeResult<string, Household> consumeResult) => Result.Ok;
        protected override HandleResult Delete(ConsumeResult<string, Household> consumeResult) => Result.Ok;
    }
}
