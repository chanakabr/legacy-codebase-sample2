using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;

namespace TVPApiModule.Objects
{
    public class ExtraParameters
    {
        public List<TagMetaIntPairArray> TagDict { get; set; }
        public int mediaID { get; set; }
        public string mediaPicURL { get; set; }
        public string templateEmail { get; set; }

        public ExtraParameters()
        {
            TagDict = new List<TagMetaIntPairArray>();
        }
    }
}
