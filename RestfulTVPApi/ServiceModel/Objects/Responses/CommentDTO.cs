using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class CommentDTO
    {
        public string Author { get; set; }
        public string Header { get; set; }
        public string AddedDate { get; set; }
        public string Content { get; set; }
    }
}