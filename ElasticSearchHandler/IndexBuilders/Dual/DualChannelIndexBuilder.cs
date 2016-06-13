using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.IndexBuilders
{
    public class DualChannelIndexBuilder : AbstractIndexBuilder
    {
        public DualChannelIndexBuilder(int groupId)
            : base(groupId)
        {

        }

        public override bool BuildIndex()
        {
            throw new NotImplementedException();
        }
    }
}
