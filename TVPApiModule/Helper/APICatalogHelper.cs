using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Context;
using TVPApiModule.Manager;
using TVPApiModule.Objects.Responses;

namespace TVPApiModule.Helper
{
    public class APICatalogHelper
    {
        public static List<Media> MediaObjToMedias(List<BaseObject> medias, string picSize, int totalItems, int groupID, PlatformType platform)
        {
            List<Media> retVal = new List<Media>();
            Media media;
            foreach (MediaObj mediaObj in medias)
            {
                media = new Media(mediaObj, picSize, totalItems, groupID, platform);
                retVal.Add(media);
            }
            return retVal;
        }

        public static List<Channel> ChannelObjToChannel(List<channelObj> channels, string picSize)
        {
            List<Channel> retVal = new List<Channel>();
            Channel channel;
            foreach (channelObj channelObj in channels)
            {
                channel = new Channel(channelObj, picSize);
                retVal.Add(channel);
            }
            return retVal;
        }

        public static List<KeyValue> GetMetasTagsFromConfiguration(string type, string value, int groupID, PlatformType platform)
        {
            List<KeyValue> retVal = new List<KeyValue>();
            string[] mediaInfoStructNames;
            switch (type)
            {
                case "meta":
                    mediaInfoStructNames = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.SearchValues.Metadata.ToString().Split(new Char[] { ';' });
                    break;
                case "tag":
                    mediaInfoStructNames = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.SearchValues.Tags.ToString().Split(new Char[] { ';' });
                    break;
                default:
                    mediaInfoStructNames = new string[0];
                    break;
            }

            foreach (string name in mediaInfoStructNames)
            {
                retVal.Add(new KeyValue() { m_sKey = name, m_sValue = value });
            }
            return retVal;
        }

        public static Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy GetCatalogOrderBy(TVPApiModule.Context.OrderBy orderBy)
        {
            Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy retVal;
            switch (orderBy)
            {
                case TVPApiModule.Context.OrderBy.None:
                    retVal = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.CREATE_DATE;
                    break;
                case TVPApiModule.Context.OrderBy.Added:
                    retVal = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.START_DATE;
                    break;
                case TVPApiModule.Context.OrderBy.Views:
                    retVal = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.VIEWS;
                    break;
                case TVPApiModule.Context.OrderBy.Rating:
                    retVal = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.RATING;
                    break;
                case TVPApiModule.Context.OrderBy.ABC:
                    retVal = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.NAME;
                    break;
                case TVPApiModule.Context.OrderBy.Meta:
                    retVal = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.META;
                    break;
                default:
                    retVal = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.CREATE_DATE;
                    break;
            }
            return retVal;
        }
    }
}
