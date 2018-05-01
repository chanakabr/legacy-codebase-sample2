using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Response
{
    public class GenericResponse<T>
    {
        public Status Status { get; set; }
        public T Object { get; set; }        

        public GenericResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Object = default(T);
        }
    }
}
