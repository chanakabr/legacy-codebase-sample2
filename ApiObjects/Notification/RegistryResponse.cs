using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects.Notification
{
    public class RegistryResponse
    {
        public Status Status { get; set; }
        public List<RegistryParameter> Items { get; set; }
    }
}
