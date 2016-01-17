using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class UserPurhcasedAssetsResponse
    {
        public List<KeyValuePair<eAssetTypes, List<string>>> assets;
        public Status status;
    }
}
