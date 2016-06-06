using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.IndexBuilders
{
    public class DualMediaIndexBuilder : AbstractIndexBuilder
    {
        public DualMediaIndexBuilder(int groupId)
            : base(groupId)
        {

        }

        public override bool BuildIndex()
        {
            throw new NotImplementedException();
        }
    }
}
