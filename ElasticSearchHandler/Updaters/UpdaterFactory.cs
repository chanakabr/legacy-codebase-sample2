using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearchHandler.Updaters
{
    public class UpdaterFactory
    {
        public static IUpdateable CreateUpdater(int nGroupID, ApiObjects.eObjectType eType)
        {
            IUpdateable result = null;

            switch (eType)
            {
                case ApiObjects.eObjectType.Media:
                    result = new MediaUpdater(nGroupID);
                    break;
                case ApiObjects.eObjectType.Channel:
                    result = new ChannelUpdater(nGroupID);
                    break;
                case ApiObjects.eObjectType.EPG:
                    result = new EpgUpdater(nGroupID);
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
