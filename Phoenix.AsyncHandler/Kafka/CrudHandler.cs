using System;
using System.Threading.Tasks;
using OTT.Lib.Kafka;
using OTT.Lib.Kafka.Extensions;
using SchemaRegistryEvents.Catalog;

namespace Phoenix.AsyncHandler.Kafka
{
    /// <summary>
    /// Long-running handler, which consumes CRUD kafka messages
    /// </summary>
    /// <typeparam name="T">CRUD message</typeparam>
    public abstract class CrudHandler<T> : IKafkaMessageHandler<T>
    {
        public virtual Task<HandleResult> Handle(ConsumeResult<string, T> consumeResult)
        {
            switch (GetOperation(consumeResult.Result.Message.Value))
            {
                case CrudOperationType.CREATE_OPERATION: return Task.FromResult(Create(consumeResult));
                case CrudOperationType.UPDATE_OPERATION: return Task.FromResult(Update(consumeResult));
                case CrudOperationType.DELETE_OPERATION: return Task.FromResult(Delete(consumeResult));
                default: throw new NotImplementedException("unknown crud operation");
            }
        }
        
        /// <summary>
        /// Method, which retrieves CRUD operation type(create/update/delete) from a message
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected abstract long GetOperation(T value);

        protected abstract HandleResult Create(ConsumeResult<string, T> consumeResult);
        protected abstract HandleResult Update(ConsumeResult<string, T> consumeResult);
        protected abstract HandleResult Delete(ConsumeResult<string, T> consumeResult);
    }
}
