using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class ParentalRulesTagsResponse
    {
        public Status status;
        public List<IdValuePair> mediaTags;
        public List<IdValuePair> epgTags;

        public ParentalRulesTagsResponse()
        {
            status = new Status();
            mediaTags = new List<IdValuePair>();
            epgTags = new List<IdValuePair>();
        }
    }

    public class IdValuePair
    {
        public int id;
        public string value;
    }
}
