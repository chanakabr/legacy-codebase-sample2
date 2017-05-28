using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class UserInterest
    {
        public string DocType { get; set; }
        public int UserId { get; set; }
        public int PartnerId { get; set; }
        public string MetaId { get; set; }
        public UserInterestTopic Topic { get; set; }

        public UserInterest()
        {
            this.DocType = "UserInterest";
        }
    }

    public class UserInterestTopic 
    {
        public string Value { get; set; }
        public UserInterestTopic ParentTopic { get; set; }
    }
}
