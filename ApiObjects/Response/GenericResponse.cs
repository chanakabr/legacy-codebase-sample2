
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

        public void SetStatus(eResponseStatus responseStatus, string message)
        {
            this.Status.Code = (int)responseStatus;
            this.Status.Message = message;
        }
    }
}
