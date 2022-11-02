using System;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using Core.ConditionalAccess;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Kronos;
using Phoenix.Generated.Tasks.Scheduled.renewSubscription;

namespace Phoenix.AsyncHandler.ConditionalAccess
{
    public class RenewHandler : IKronosTaskHandler
    {
        private readonly ILogger<RenewHandler> _logger;
        public RenewHandler(ILogger<RenewHandler> logger)
        {
            _logger = logger;
        }
        public Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            RenewSubscription renewData = JsonConvert.DeserializeObject<RenewSubscription>(Encoding.UTF8.GetString(request.TaskBody.ToByteArray()));
            if (!renewData.PartnerId.HasValue || !renewData.EndDate.HasValue)
            {
                _logger.LogError("Renew - needed information is missing");
                return Task.FromResult(new ExecuteTaskResponse
                {
                    IsSuccess = false,
                    Message = "Renew process failed - needed information is missing."
                });
            }
            _logger.LogInformation($"Execute renew task PartnerId: {renewData.PartnerId.Value}, EndDate: {renewData.EndDate.Value}");
            switch ((eSubscriptionRenewRequestType)renewData.RenewalType.Value)
            {
                case eSubscriptionRenewRequestType.Renew:
                    {
                        if (!renewData.PurchaseId.HasValue)
                        {
                            _logger.LogError("Renew - purchaseId missing");
                            return Task.FromResult(new ExecuteTaskResponse
                            {
                                IsSuccess = false,
                                Message = "Renew process failed - PurchaseId missing."
                            });
                        }
                        
                        Module.Renew((int)renewData.PartnerId.Value, renewData.UserId,
                            renewData.PurchaseId.Value, renewData.BillingGuid, renewData.EndDate.Value, isKronos: true);

                        return Task.FromResult(new ExecuteTaskResponse
                        {
                            IsSuccess = true,
                            Message = "Renew process is completed."
                        });
                    }
                case eSubscriptionRenewRequestType.RenewalReminder:
                    {
                        if (!renewData.ProcessId.HasValue || !renewData.HouseholdId.HasValue)
                        {
                            _logger.LogError("RenewalReminder - needed information is missing");
                            return Task.FromResult(new ExecuteTaskResponse
                            {
                                IsSuccess = false,
                                Message = "Renew process failed - needed information is missing."
                            });
                        }
                        if (renewData.ProcessId > 0)
                        {
                            Module.UnifiedRenewalReminder((int)renewData.PartnerId.Value, renewData.HouseholdId.Value, 
                                renewData.ProcessId.Value, renewData.EndDate.Value, isKronos: true);
                        }
                        else if (renewData.PurchaseId > 0)
                        {
                            Module.RenewalReminder((int)renewData.PartnerId.Value, renewData.UserId, renewData.PurchaseId.Value, renewData.EndDate.Value);
                        }

                        return Task.FromResult(new ExecuteTaskResponse
                        {
                            IsSuccess = true,
                            Message = "RenewalReminder process is completed."
                        });
                    }
                case eSubscriptionRenewRequestType.SubscriptionEnds:
                    {
                        if (!renewData.HouseholdId.HasValue || !renewData.PurchaseId.HasValue)
                        {
                            _logger.LogError("SubscriptionEnds - needed information is missing");
                            return Task.FromResult(new ExecuteTaskResponse
                            {
                                IsSuccess = false,
                                Message = "Renew process failed - needed information is missing."
                            });
                        }
                        Module.SubscriptionEnds((int)renewData.PartnerId.Value, renewData.UserId,
                            renewData.HouseholdId.Value, renewData.PurchaseId.Value, renewData.EndDate.Value, true);

                        return Task.FromResult(new ExecuteTaskResponse
                        {
                            IsSuccess = true,
                            Message = "SubscriptionEnds process is completed."
                        });
                    }
                case eSubscriptionRenewRequestType.RenewUnifiedTransaction:
                    {
                        Module.RenewUnifiedTransaction((int)renewData.PartnerId.Value, renewData.HouseholdId.Value,
                            renewData.ProcessId.Value, renewData.EndDate.Value, isKronos: true);

                        return Task.FromResult(new ExecuteTaskResponse
                        {
                            IsSuccess = true,
                            Message = "RenewUnifiedTransaction process is completed."
                        });
                    }
                default:
                    _logger.LogError($"SubscriptionRenewRequestType:{renewData.RenewalType.Value} not exists.");
                    return Task.FromResult(new ExecuteTaskResponse
                    {
                        IsSuccess = false,
                        Message = $"SubscriptionRenewRequestType:{renewData.RenewalType.Value} not exists."
                    });
            }
        }
    }
}