using ApiObjects.SearchObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearch.Searcher
{
    public class SortUtils
    {

        /// <summary>
        /// Returns the sort string for the query
        /// </summary>
        /// <returns></returns>
        public static string GetSort(OrderObj order, List<string> returnFields, bool functionScoreSort = false)
        {
            if (returnFields == null)
            {
                returnFields = new List<string>();
            }

            bool hasScoreSort = false;
            JArray sortArray = new JArray();

            if (functionScoreSort)
            {
                JObject scoreSort = new JObject();
                scoreSort["_score"] = "desc";
                sortArray.Add(scoreSort);
                hasScoreSort = true;
            }

            string primaryOrderField = string.Empty;

            if (order.m_eOrderBy == OrderBy.META)
            {
                string metaFieldName = string.Empty;
                metaFieldName = GetMetaSortField(order);

                primaryOrderField = metaFieldName;
                returnFields.Add(string.Format("\"{0}\"", metaFieldName));
            }
            else if (order.m_eOrderBy == OrderBy.ID)
            {
                primaryOrderField = "_uid";
            }
            else if (order.m_eOrderBy == OrderBy.RELATED || order.m_eOrderBy == OrderBy.NONE)
            {
                if (!hasScoreSort)
                {
                    primaryOrderField = "_score";
                    hasScoreSort = true;
                }
            }
            else
            {
                primaryOrderField = Enum.GetName(typeof(OrderBy), order.m_eOrderBy).ToLower();
            }

            ////if (order.m_eOrderBy != OrderBy.META && !order.shouldPadString && sortBuilder.Length > 0)
            //if (sortBuilder.Length > 0)
            if (!string.IsNullOrWhiteSpace(primaryOrderField))
            {
                JObject primaryOrder = new JObject();
                primaryOrder[primaryOrderField] = new JObject();
                primaryOrder[primaryOrderField]["order"] = JToken.FromObject((order.m_eOrderDir.ToString().ToLower()));

                sortArray.Add(primaryOrder);
            }

            //we always add the score at the end of the sorting so that our records will be in best order when using wildcards in the query itself
            if (order.m_eOrderBy != OrderBy.ID && order.m_eOrderBy != OrderBy.RELATED && order.m_eOrderBy != OrderBy.NONE && !hasScoreSort)
            {
                JObject scoreSort = new JObject();
                scoreSort["_score"] = "desc";
                sortArray.Add(scoreSort);
                hasScoreSort = true;
            }

            if (order.m_eOrderBy != OrderBy.ID)
            {
                // Always add sort by _id to avoid ES weirdness of same sort-value 
                JObject idOrder = new JObject();
                idOrder["_uid"] = new JObject();
                idOrder["_uid"]["order"] = JToken.FromObject("desc");

                sortArray.Add(idOrder);
            }

            return string.Format("\"sort\" : {0}", sortArray.ToString(Newtonsoft.Json.Formatting.None));
        }

        public static string GetMetaSortField(OrderObj order)
        {
            if (order.m_eOrderBy != OrderBy.META) return null;
            return order.shouldPadString
                ? string.Format("metas.padded_{0}", order.m_sOrderValue.ToLower())
                : string.Format("metas.{0}", order.m_sOrderValue.ToLower());
        }

    }
}
