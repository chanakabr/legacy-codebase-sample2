using System;
using System.Collections.Generic;
using System.Text;

namespace RedisManager
{
    public class RedisClientResponse<T>
    {

        public bool IsSuccess { get; set; }
        public T Result { get; set; }

        public RedisClientResponse()
        {
            this.IsSuccess = false;
            this.Result = default(T);
        }

        public RedisClientResponse(bool isSuccess, T result)
        {
            this.IsSuccess = isSuccess;
            this.Result = result;
        }

        public void SetResponse(bool isSuccess, T result)
        {
            this.IsSuccess = isSuccess;
            this.Result = result;
        }

    }

}
