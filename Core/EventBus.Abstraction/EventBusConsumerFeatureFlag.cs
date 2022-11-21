using FeatureFlag;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ott.Lib.FeatureToggle.IocExtensions;

namespace EventBus.Abstraction
{
    public static class PhoenixFeatureFlagExtensions
    {
        public static IServiceCollection AddPhoenixFeatureFlag(this IServiceCollection s)
        {
            return s
                .AddFeatureToggle(new ConfigurationBuilder().AddEnvironmentVariables().Build())
                .AddScoped<IFeatureFlagContext, EventBusConsumerFeatureFlagContext>()
                .AddScoped<IPhoenixFeatureFlag, PhoenixFeatureFlag>();
        }
    }
    
    internal sealed class EventBusConsumerFeatureFlagContext : IFeatureFlagContext
    {
        private readonly IEventContext _eventContext;

        public EventBusConsumerFeatureFlagContext(IEventContext eventContext)
        {
            _eventContext = eventContext;
        }
        
        public long? GetPartnerId()
        {
            return _eventContext.GroupId;
        }

        public long? GetUserId()
        {
            return _eventContext.UserId;
        }
    }
}
