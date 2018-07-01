using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    public class CouponsGroupResponse
    {
        public CouponsGroup CouponsGroup { get; set; }

        public ApiObjects.Response.Status Status { get; set; }

        public CouponsGroupResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }

    }

    public class CouponsGroupsResponse
    {
        public List<CouponsGroup> CouponsGroups { get; set; }

        public ApiObjects.Response.Status Status { get; set; }

        public CouponsGroupsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }

    }
}
