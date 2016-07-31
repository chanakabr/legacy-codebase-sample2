using ElasticSearch.Common;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler
{
    public class AbstractIndexRebaser
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        protected int groupId;
        protected ElasticSearchApi api;
        protected BaseESSeralizer serializer;

        public AbstractIndexRebaser(int groupId)
        {
            this.groupId = groupId;
            api = new ElasticSearchApi();
            serializer = new ESSerializerV2();
        }

        public virtual bool Rebase()
        {
            bool result = false;
            
            return result;
        }
    }
}
