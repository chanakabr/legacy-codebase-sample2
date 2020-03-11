using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class UserPurhcasedAssetsResponse
    {
        public List<ApiObjects.KeyValuePair> assets;
        public Status status;
    }
}
