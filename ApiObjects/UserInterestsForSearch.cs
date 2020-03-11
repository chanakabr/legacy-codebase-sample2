using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class UserInterestsMetasAndTags
    {
        public Dictionary<string, List<string>> Metas { get; set; }
        public Dictionary<string, List<string>> Tags { get; set; }

        public UserInterestsMetasAndTags()
        {
            Metas = new Dictionary<string, List<string>>();
            Tags = new Dictionary<string, List<string>>();
        }
    }
}
