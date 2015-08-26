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
        public List<KeyValuePair<string, List<string>>> mediaTags;
        public List<KeyValuePair<string, List<string>>> epgTags;

        public ParentalRulesTagsResponse()
        {
            status = new Status();
            mediaTags = new List<KeyValuePair<string, List<string>>>();
            epgTags = new List<KeyValuePair<string, List<string>>>();
        }
    }
}
