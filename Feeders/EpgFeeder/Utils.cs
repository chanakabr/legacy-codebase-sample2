using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using EpgBL;
using KLogMonitor;

namespace EpgFeeder
{
    public static class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static string GetValueByKey(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }



        public static bool ParseEPGStrToDate(string dateStr, ref DateTime theDate)
        {
            if (string.IsNullOrEmpty(dateStr) || dateStr.Length < 14)
                return false;

            string format = "yyyyMMddHHmmss";
            bool res = DateTime.TryParseExact(dateStr.Substring(0, 14), format, null, System.Globalization.DateTimeStyles.None, out theDate);
            return res;
        }

        public static Dictionary<string, List<string>> GetEpgProgramMetas(List<FieldTypeEntity> FieldEntityMapping)
        {
            Dictionary<string, List<string>> dMetas = new Dictionary<string, List<string>>();

            var MetaFieldEntity = from item in FieldEntityMapping
                                  where item.FieldType == enums.FieldTypes.Meta && item.XmlReffName.Capacity > 0 && item.Value != null && item.Value.Count > 0
                                  select item;

            foreach (var item in MetaFieldEntity)
            {
                foreach (var value in item.Value)
                {
                    if (dMetas.ContainsKey(item.Name))
                    {
                        dMetas[item.Name].AddRange(item.Value);
                    }
                    else
                    {
                        dMetas.Add(item.Name, item.Value);
                    }
                }
            }
            return dMetas;
        }

        public static Dictionary<string, List<string>> GetEpgProgramTags(List<FieldTypeEntity> FieldEntityMapping)
        {
            Dictionary<string, List<string>> dTags = new Dictionary<string, List<string>>();
            var TagFieldEntity = from item in FieldEntityMapping
                                 where item.FieldType == enums.FieldTypes.Tag && item.XmlReffName.Capacity > 0 && item.Value != null && item.Value.Count > 0
                                 select item;


            foreach (var item in TagFieldEntity)
            {
                if (dTags.ContainsKey(item.Name))
                {
                    dTags[item.Name].AddRange(item.Value);
                }
                else
                {
                    dTags.Add(item.Name, item.Value);
                }

            }
            return dTags;
        }


        /*Build query by channelId and spesipic dates*/
        private static string BuildDeleteQuery(int channelID, List<DateTime> lDates)
        {
            string sQuery = string.Empty;

            ESTerm epgChannelTerm = new ESTerm(true) { Key = "epg_channel_id", Value = channelID.ToString() };

            BoolQuery oBoolQuery = new BoolQuery();


            BoolQuery oBoolQueryDates = new BoolQuery();
            foreach (DateTime date in lDates)
            {
                string sMaxtDate = date.AddDays(1).AddMilliseconds(-1).ToString("yyyyMMddHHmmss");

                ESRange startDateRange = new ESRange(false);

                startDateRange.Key = "start_date";
                string sMin = date.ToString("yyyyMMddHHmmss");
                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMaxtDate));

                oBoolQueryDates.AddChild(startDateRange, ApiObjects.SearchObjects.CutWith.OR);
            }

            oBoolQuery.AddChild(epgChannelTerm, ApiObjects.SearchObjects.CutWith.AND); // channel must be equel to channelID
            oBoolQuery.AddChild(oBoolQueryDates, ApiObjects.SearchObjects.CutWith.AND);// and start date must be in lDates list (with or between dates)

            sQuery = oBoolQuery.ToString();


            return sQuery;

        }

        internal static bool DeleteEPGDocFromES(string parentGroupID, int channelID, List<DateTime> lDates)
        {
            bool resDelete = false;
            try
            {
                ElasticSearchApi oESApi = new ElasticSearchApi();

                string sQuery = BuildDeleteQuery(channelID, lDates);
                string sIndex = string.Format("{0}_{1}", parentGroupID, "epg");
                resDelete = oESApi.DeleteDocsByQuery(sIndex, "epg", ref sQuery);
                return resDelete;
            }
            catch (Exception ex)
            {
                log.Error("DeleteDocFromES - " + string.Format("channelID = {0},ex = {1}", channelID, ex.Message), ex);
                return false;
            }
        }


        internal static void GenerateTagsAndValues(ApiObjects.EpgCB epg, List<FieldTypeEntity> FieldEntityMapping, ref Dictionary<int, List<string>> tagsAndValues)
        {
            foreach (string tagType in epg.Tags.Keys)
            {
                string tagTypel = tagType.ToLower();
                int tagTypeID = 0;
                List<FieldTypeEntity> tagField = FieldEntityMapping.Where(x => x.FieldType == enums.FieldTypes.Tag && x.Name.ToLower() == tagTypel).ToList();
                if (tagField != null && tagField.Count > 0)
                {
                    tagTypeID = tagField[0].ID;
                }
                else
                {
                    log.Debug("UpdateExistingTagValuesPerEPG - " + string.Format("Missing tag Definition in FieldEntityMapping of tag:{0} in EPG:{1}", tagType, epg.EpgID));
                    continue;//missing tag definition in DB (in FieldEntityMapping)                        
                }

                if (!tagsAndValues.ContainsKey(tagTypeID))
                {
                    tagsAndValues.Add(tagTypeID, new List<string>());
                }
                foreach (string tagValue in epg.Tags[tagType])
                {
                    if (!tagsAndValues[tagTypeID].Contains(tagValue.ToLower()))
                        tagsAndValues[tagTypeID].Add(tagValue.ToLower());
                }
            }
        }
    }
}
