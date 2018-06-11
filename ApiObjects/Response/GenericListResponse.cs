
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

        public bool HasObjects()
        {
            return (Status.Code == (int)eResponseStatus.OK && Objects != null && Objects.Count > 0);
        }
    }
}
