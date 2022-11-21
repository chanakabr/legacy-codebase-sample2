using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiObjects.Cloudfront;
using Microsoft.Extensions.Logging;
using OTT.Service.CloudfrontInvalidator;
using Polly;
using TaskStatus = OTT.Service.CloudfrontInvalidator.TaskStatus;

namespace CloudfrontInvalidatorGrpcClientWrapper
{
    public class CloudfrontForceInvalidator : ICloudfrontInvalidator
    {
        private readonly CloudfrontInvalidator.CloudfrontInvalidatorClient _client;
        private readonly ILogger<CloudfrontForceInvalidator> _logger;

        public CloudfrontForceInvalidator(
            CloudfrontInvalidator.CloudfrontInvalidatorClient client,
            ILogger<CloudfrontForceInvalidator> logger)
        {
            _client = client;
            _logger = logger;
        }
        
        public async Task<(bool success, IEnumerable<string> failedPaths)> InvalidateAndWaitAsync(int partnerId, string[] paths, WaitConfig waitConfig)
        {
            var invalidated = await Task.WhenAll(paths.Select(path => InvalidateAndWaitAsync(partnerId, path, waitConfig)));
            var failedInvalidations = paths.Where((_, i) => !invalidated[i]).ToList();
            return (failedInvalidations.Count == 0, failedInvalidations);
        }

        private async Task<bool> InvalidateAndWaitAsync(int partnerId, string path, WaitConfig waitConfig)
        {
            var taskId = await CreateInvalidationTaskAsync(partnerId, path);
            _logger.LogInformation("invalidation task created. partner:[{Partner}] path:[{Path}] taskId:[{TaskId}]",
                partnerId, path, taskId);
            
            var status = await Waiter(waitConfig).ExecuteAsync(() => GetInvalidationTaskStatusAsync(taskId));
            _logger.LogInformation(
                "invalidation task finished. partner:[{Partner}] path:[{Path}] taskId:[{TaskId} status:[{Status}]]",
                partnerId, path, taskId, status);

            return status == TaskStatus.Success;
        }

        private static IAsyncPolicy<TaskStatus> Waiter(WaitConfig waitConfig) => Policy
            .HandleResult<TaskStatus>(status => status == TaskStatus.Todo)
            .WaitAndRetryAsync(
                waitConfig.RetriesCount(), 
                _ => waitConfig.SleepDuration);

        private async Task<string> CreateInvalidationTaskAsync(int partnerId, string path)
        {
            var response = await _client.CreateInvalidationTaskAsync(new InvalidationTaskRequest
            {
                PartnerId = partnerId,
                Path = path
            });
            return response.Id;
        }

        private async Task<TaskStatus> GetInvalidationTaskStatusAsync(string taskId)
        {
            var status = await _client.GetInvalidationTaskStatusAsync(new InvalidationTaskStatusRequest
            {
                Id = taskId
            });
            return status.Status;
        }
    }
}