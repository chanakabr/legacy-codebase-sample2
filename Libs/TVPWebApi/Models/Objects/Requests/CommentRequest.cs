using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPWebApi.Models.Objects
{
    public class CommentRequest : BaseRequest
    {
        public int media_type { get; set; }
        public string writer { get; set; }
        public string header { get; set; }
        public string sub_header { get; set; }
        public string content { get; set; }
        public bool is_auto_active { get; set; }
    }
}