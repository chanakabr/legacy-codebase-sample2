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
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Objects = new List<T>();
            TotalItems = 0;
        }

        public void SetStatus(eResponseStatus responseStatus, string message)
        {
            this.Status.Code = (int)responseStatus;
            this.Status.Message = message;
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

        public bool HasObjects()
        {
            return (Status.Code == (int)eResponseStatus.OK && Objects != null && Objects.Count > 0);
        }

    }
}
