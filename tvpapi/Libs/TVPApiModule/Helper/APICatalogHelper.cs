using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;

namespace TVPApiModule.Helper
{
    public class APICatalogHelper
    {
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

        public static Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy GetCatalogOrderBy(TVPApi.OrderBy orderBy)
        {
            Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy retVal;
            switch (orderBy)
            {
                case TVPApi.OrderBy.None:
                    retVal = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.CREATE_DATE;
                    break;
                case TVPApi.OrderBy.Added:
                    retVal = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.START_DATE;
                    break;
                case TVPApi.OrderBy.Views:
                    retVal = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.VIEWS;
                    break;
                case TVPApi.OrderBy.Rating:
                    retVal = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.RATING;
                    break;
                case TVPApi.OrderBy.ABC:
                    retVal = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.NAME;
                    break;
                case TVPApi.OrderBy.Meta:
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
