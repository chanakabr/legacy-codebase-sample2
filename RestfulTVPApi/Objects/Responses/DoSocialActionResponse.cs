using RestfulTVPApi.Objects.Responses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class DoSocialActionResponse
    {
        public SocialActionResponseStatus action_response_status_intern { get; set; }

        public SocialActionResponseStatus action_response_status_extern { get; set; }
    }
}
