using System;
using OTT.Lib.Kafka;
using SchemaRegistryEvents.Catalog;

namespace Phoenix.AsyncHandler.Kafka
{
    /// <summary>
    /// Long-running handler, which consumes CRUD kafka messages
    /// </summary>
    /// <typeparam name="T">CRUD message</typeparam>
    public abstract class CrudHandler<T> : IHandler<T>
    {
        public virtual HandleResult Handle(ConsumeResult<string, T> consumeResult)
        {
            switch (GetOperation(consumeResult.Result.Message.Value))
            {
                case CrudOperationType.CREATE_OPERATION: return Create(consumeResult);
                case CrudOperationType.UPDATE_OPERATION: return Update(consumeResult);
                case CrudOperationType.DELETE_OPERATION: return Delete(consumeResult);
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
