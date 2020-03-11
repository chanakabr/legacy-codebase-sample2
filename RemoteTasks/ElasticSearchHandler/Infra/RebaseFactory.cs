using ElasticSearchHandler.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler
{
    public class RebaseFactory
    {
        public static AbstractIndexRebaser CreateRebaser(int groupId, ApiObjects.eObjectType type)
        {
            AbstractIndexRebaser rebaser = null;

            switch (type)
            {
                case ApiObjects.eObjectType.Unknown:
                break;
                case ApiObjects.eObjectType.Media:
                {
                    rebaser = new MediaRebaser(groupId);
                }
                break;
                case ApiObjects.eObjectType.Channel:
                break;
                case ApiObjects.eObjectType.EPG:
                {
                    rebaser = new EPGRebaser(groupId);
                    break;
                }
                case ApiObjects.eObjectType.EpgChannel:
                break;
                case ApiObjects.eObjectType.Recording:
                break;
                default:
                {
                    rebaser = new AbstractIndexRebaser(groupId);
                    break;
                }
            }

            return rebaser;
        }
    }
}
