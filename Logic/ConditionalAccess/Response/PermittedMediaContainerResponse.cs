using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess.Response
{
    public class PermittedMediaContainerResponse
    {
        public PermittedMediaContainer[] PermittedMediaContainer { get; set; }

        public ApiObjects.Response.Status Status { get; set; }

    }
}
