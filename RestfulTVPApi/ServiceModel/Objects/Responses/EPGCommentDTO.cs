using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class EPGCommentDTO
    {
        public int ID { get; set; }
        public DateTime CreateDate { get; set; }
        public int EPGProgramID { get; set; }
        public int Language { get; set; }
        public string LanguageName { get; set; }
        public string ContentText { get; set; }
        public string Header { get; set; }
        public string Writer { get; set; }
    }
}