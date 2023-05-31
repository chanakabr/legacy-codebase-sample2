using OTT.Lib.Kafka;
using OTT.Lib.Kafka.Utils;
using SchemaRegistryEvents;

namespace Phoenix.AsyncHandler.Kafka
{
    public static class Result
    {
        public static readonly HandleResult Ok = new HandleResult();

        public static TValue GetValue<TKey, TValue>(this ConsumeResult<TKey, TValue> r) => r.Result.Message.Value;
        public static string GetSourceService<TKey, TValue>(this ConsumeResult<TKey, TValue> r) => r.Result.Message.Headers.Get<string>(SourceService.HeaderName);
    }
}