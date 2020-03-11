using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using Core.Catalog;
using ApiObjects.SearchObjects;
using Core.Catalog.Request;
using Core.Catalog.Response;
using OrderBy = ApiObjects.SearchObjects.OrderBy;

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

        public static OrderBy GetCatalogOrderBy(TVPApi.OrderBy orderBy)
        {
            OrderBy retVal;
            switch (orderBy)
            {
                case TVPApi.OrderBy.None:
                    retVal = OrderBy.CREATE_DATE;
                    break;
                case TVPApi.OrderBy.Added:
                    retVal = OrderBy.START_DATE;
                    break;
                case TVPApi.OrderBy.Views:
                    retVal = OrderBy.VIEWS;
                    break;
                case TVPApi.OrderBy.Rating:
                    retVal = OrderBy.RATING;
                    break;
                case TVPApi.OrderBy.ABC:
                    retVal = OrderBy.NAME;
                    break;
                case TVPApi.OrderBy.Meta:
                    retVal = OrderBy.META;
                    break;
                default:
                    retVal = OrderBy.CREATE_DATE;
                    break;
            }
            return retVal;
        }
    }
}
