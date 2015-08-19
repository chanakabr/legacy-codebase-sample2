using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearchHandler.IndexBuilders
{
    public static class IndexBuilderFactory
    {
        public static AbstractIndexBuilder CreateIndexBuilder(int nGroupID, ApiObjects.eObjectType eType)
        {
            AbstractIndexBuilder result = null;

            switch (eType)
            {
                case ApiObjects.eObjectType.Media:
                case ApiObjects.eObjectType.Channel:
                    result = new MediaIndexBuilder(nGroupID);
                    break;
                case ApiObjects.eObjectType.EPG:
                    result = new EpgIndexBuilder(nGroupID);
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
