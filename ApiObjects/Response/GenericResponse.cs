
namespace ApiObjects.Response
{
    public class GenericResponse<T>
    {
        public Status Status { get; private set; }
        public T Object { get; set; }

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

        public void SetStatus(eResponseStatus responseStatus, string message)
        {
            this.Status.Code = (int)responseStatus;
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
    }
}
