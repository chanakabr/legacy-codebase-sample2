using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Response
{
    public static class GenericResponse
    {
        public static GenericResponse<T> Create<T>(Status status, T obj = default)
        {
            return new GenericResponse<T> (status, obj);
        }
    }

    public class GenericResponse<T>
    {
        public Status Status { get; private set; }
        public T Object { get; set; }

        public GenericResponse()
        {
            Status = new Status(eResponseStatus.Error);
            Object = default(T);
        }

        public GenericResponse(Status status, T obj)
        {
            Status = status;
            Object = obj;
        }

        public GenericResponse(eResponseStatus responseStatus, string message = null, List<KeyValuePair> args = null)
        {
            Object = default(T);
            Status = new Status(responseStatus, message, args);
        }

        public GenericResponse(Status status)
        {
            if (status != null)
            {
                Status = status;
            }
            else
            {
                Status = new Status(eResponseStatus.Error);
            }
            
            Object = default(T);
        }

        public static GenericResponse<TResponse> Create<TResponse>(Status status, TResponse obj = default)
        {
            return new GenericResponse<TResponse> { Status = status, Object = obj };
        }

        public void SetStatus(eResponseStatus responseStatus, string message = null, List<KeyValuePair> args = null)
        {
            this.Status.Set(responseStatus, message, args);
        }

        public void SetStatus(int responseStatusCode, string message = null, List<KeyValuePair> args = null)
        {
            this.Status.Set(responseStatusCode, message, args);
        }

        public void SetStatus(Status status)
        {
            this.Status.Set(status);
        }

        public bool IsOkStatusCode()
        {
            
            return Status != null && Status.IsOkStatusCode();
        }

        public bool StatusIs(eResponseStatus expectedStatus) => Status?.Code == (int)expectedStatus;

        public bool HasObject()
        {
            return IsOkStatusCode() && !Object.Equals(default(T)); // TODO NullReferenceException when `Object` is null, ironic, use EqualityComparer instead
        }

        public string ToStringStatus()
        {
            return this.Status != null ? this.Status.Code + " - " + this.Status.Message : string.Empty;
        }

        public T GetOrThrow(Exception customException = null)
        {
            if (IsOkStatusCode()) return Object;
            throw customException ?? new KalturaException(Status.Message, Status.Code);
        }
    }
}
