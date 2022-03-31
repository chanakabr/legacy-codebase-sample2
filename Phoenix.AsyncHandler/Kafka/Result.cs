using OTT.Lib.Kafka;

namespace Phoenix.AsyncHandler.Kafka
{
    public static class Result
    {
        public static readonly HandleResult Ok = new HandleResult();

        public static TValue GetValue<TKey, TValue>(this ConsumeResult<TKey, TValue> r) => r.Result.Message.Value;
    }
}