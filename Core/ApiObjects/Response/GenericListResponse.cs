using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ApiObjects.Response
{
    public class GenericListResponse<T>
    {
        public Status Status { get; private set; }
        public List<T> Objects { get; set; }
        public int TotalItems { get; set; }

        public GenericListResponse()
        {
            Status = new Status(eResponseStatus.Error);
            Objects = new List<T>();
            TotalItems = 0;
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

        public bool HasObjects()
        {
            return (IsOkStatusCode() && Objects != null && Objects.Count > 0);
        }

        public bool IsOkStatusCode()
        {
            return (Status != null && Status.IsOkStatusCode());
        }
    }
}
