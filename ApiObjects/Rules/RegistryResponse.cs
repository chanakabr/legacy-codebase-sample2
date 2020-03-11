using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Rules
{    
    public class RegistryResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<RegistrySettings> registrySettings {get; set;}
      

         public RegistryResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            registrySettings = new List<RegistrySettings>();
        }

         public RegistryResponse(ApiObjects.Response.Status resp, List<RegistrySettings> registrySettings)
        {
            this.Status = resp;
            this.registrySettings = registrySettings;
        }
    }
   
}
