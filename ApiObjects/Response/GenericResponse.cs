using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Response
{
    public class GenericResponse<T>
    {
        public Status Status { get; private set; }
        public T Object { get; set; }

        public GenericResponse(Status status, T obj)
        {
            Status = status;
            Object = obj;
        }

        public GenericResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Object = default(T);
        }

        public GenericResponse(Status status)
        {
            if (status != null)
            {
                Status = status;
            }
            else
            {
                Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            
            Object = default(T);
        }

        public void SetStatus(eResponseStatus responseStatus, string message = null)
        {
            this.Status.Code = (int)responseStatus;

            if (string.IsNullOrEmpty(message))
            {
                this.Status.Message = responseStatus.ToString();
            }
            else
            {
                this.Status.Message = message;
            }
        }

        public void SetStatus(int responseStatusCode, string message)
        {
            this.Status.Code = responseStatusCode;
            this.Status.Message = message;
        }

        public void SetStatus(Status status)
        {
            if (status != null)
            {
                this.Status.Code = status.Code;
                this.Status.Message = status.Message;
            }
        }

        public bool IsOkStatusCode()
        {
            return Status != null && Status.IsOkStatusCode() == true;
        }

        public bool HasObject()
        {
            return IsOkStatusCode() && !Object.Equals(default(T));
        }

        public string ToStringStatus()
        {
            return this.Status != null ? this.Status.Code + " - " + this.Status.Message : string.Empty;
        }
    }
}
