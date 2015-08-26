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
        public List<TagPair> mediaTags;
        public List<TagPair> epgTags;

        public ParentalRulesTagsResponse()
        {
            status = new Status();
            mediaTags = new List<TagPair>();
            epgTags = new List<TagPair>();
        }
    }
}
