using ApiObjects;
using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Common
{
    public class ESSerializer
    {
        private static readonly string META_DOUBLE_SUFFIX = "_DOUBLE";
        private static readonly string META_BOOL_SUFFIX = "_BOOL";

        private static readonly string JSON_ANALYZED_TAG_AND_META = " { \"type\": \"string\", \"analyzer\": \"whitespace\", \"null_value\": \"\" }";
        private static readonly string JSON_TAG_AND_META = " { \"type\": \"string\", \"index\": \"not_analyzed\", \"null_value\": \"\" },";

        public ESSerializer()
        {
        }

        public string SerializeMediaObject(Media oMedia)
        {
            
            StringBuilder sRecord = new StringBuilder();
            sRecord.Append("{ ");
            sRecord.AppendFormat("\"media_id\": {0}, \"group_id\": {1}, \"media_type_id\": {2}, \"wp_type_id\": {3}, \"is_active\": {4}, \"device_rule_id\": {5}, \"like_counter\": {6}, \"views\": {7}, \"rating\": {8}, \"votes\": {9}, \"start_date\": \"{10}\", \"end_date\": \"{11}\", \"final_date\": \"{12}\", \"create_date\": \"{13}\", \"update_date\": \"{14}\", \"name\": \"{15}\", \"name.analyzed\": \"{15}\", \"description\": \"{16}\", \"description.analyzed\": \"{16}\", \"cache_date\": \"{17}\", ",
                            oMedia.m_nMediaID, oMedia.m_nGroupID, oMedia.m_nMediaTypeID, oMedia.m_nWPTypeID, oMedia.m_nIsActive, oMedia.m_nDeviceRuleId, oMedia.m_nLikeCounter, oMedia.m_nViews, oMedia.m_dRating, oMedia.m_nVotes, oMedia.m_sStartDate, oMedia.m_sEndDate, oMedia.m_sFinalEndDate, oMedia.m_sCreateDate, oMedia.m_sUpdateDate, EscapeValues(ref oMedia.m_sName), EscapeValues(ref oMedia.m_sDescription), DateTime.UtcNow.ToString("yyyyMMddHHmmss"));

            #region add media file types

            sRecord.Append(" \"media_file_types\": [");
            if (!string.IsNullOrWhiteSpace(oMedia.m_sMFTypes))
            {
                string[] lMFTypesSplited = oMedia.m_sMFTypes.Split(';');
                List<string> lFileTypes = new List<string>();

                for (int i = 0; i < lMFTypesSplited.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(lMFTypesSplited[i]))
                        lFileTypes.Add(lMFTypesSplited[i]);
                }


                if (lFileTypes.Count > 0)
                {
                    sRecord.Append(
                       lFileTypes.Aggregate((current, next) => current + "," + next)
                       );
                }
            }

            sRecord.Append("],");
            #endregion

            #region add user types
            List<string> lUserTypes = new List<string>();
            sRecord.Append(" \"user_types\": [");
            if (!string.IsNullOrEmpty(oMedia.m_sUserTypes))
            {
                string[] lUserTypesSplited = oMedia.m_sUserTypes.Split(';');

                foreach (string ut in lUserTypesSplited)
                {
                    if (!string.IsNullOrWhiteSpace(ut))
                        lUserTypes.Add(ut);
                }
                if (lUserTypes.Count > 0)
                {

                }
            }
            else
            {
                lUserTypes.Add("0");
            }
            sRecord.Append(string.Join(",", lUserTypes));
            sRecord.Append("],");
            #endregion

            #region add metas
            sRecord.Append(" \"metas\": {");

            if (oMedia.m_oMeatsValues != null && oMedia.m_oMeatsValues.Count > 0)
            {
                List<string> metaNameValues = new List<string>();

                foreach (string sMetaName in oMedia.m_oMeatsValues.Keys)
                {
                    if (!string.IsNullOrWhiteSpace(sMetaName))
                    {
                        string sMetaValue = oMedia.m_oMeatsValues[sMetaName];
                        if (!string.IsNullOrWhiteSpace(sMetaValue))
                        {
                            metaNameValues.Add(string.Format(" \"{0}\": \"{1}\"", sMetaName.ToLower(), EscapeValues(ref sMetaValue)));
                            metaNameValues.Add(string.Format(" \"{0}.analyzed\": \"{1}\"", sMetaName.ToLower(), EscapeValues(ref sMetaValue)));
                        }
                    }
                }

                if (metaNameValues.Count > 0)
                    sRecord.Append(metaNameValues.Aggregate((current, next) => current + "," + next));
            }
            sRecord.Append("},");
            #endregion

            #region add tags
            sRecord.Append(" \"tags\": {");

            if (oMedia.m_oTagsValues != null && oMedia.m_oTagsValues.Count > 0)
            {
                List<string> tagNameValues = new List<string>();

                foreach (string sTagName in oMedia.m_oTagsValues.Keys)
                {
                    if (!string.IsNullOrEmpty(sTagName))
                    {
                        string sTagValue = oMedia.m_oTagsValues[sTagName];
                        if (!string.IsNullOrWhiteSpace(sTagValue))
                        {
                            string[] lSplitTagVals = sTagValue.Split(';');
                            
                            List<string> lSplitedTags = new List<string>();
                            foreach (string splitTagVal in lSplitTagVals)
                            {
                                string sTrimed = splitTagVal.Trim();
                                lSplitedTags.Add(string.Format("\"{0}\"", EscapeValues(ref sTrimed)));
                            }
                            

                            tagNameValues.Add(string.Format(" \"{0}\": [ {1} ]", sTagName.ToLower(), lSplitedTags.Aggregate((current, next) => current + "," + next)));
                            tagNameValues.Add(string.Format(" \"{0}.analyzed\": \"{1}\"", sTagName.ToLower(), EscapeValues(ref sTagValue)));
                        }
                    }
                }
                if (tagNameValues.Count > 0)
                    sRecord.Append(tagNameValues.Aggregate( (current, next) => current + "," + next ));
            }

            sRecord.Append(" }");

            #endregion

            sRecord.Append(" }");

            return sRecord.ToString();
            //add m_sMFTypes , metas and tags

        }

        public string CreateMediaMapping(Dictionary<int, Dictionary<string, string>> oMetasValuesByGroupId, Dictionary<int, string> oGroupTags)
        {
            if (oMetasValuesByGroupId == null || oGroupTags == null)
                return string.Empty;

            ESMappingObj mappingObj = new ESMappingObj("media");

            #region Add basic type mappings - (e.g. media_id, group_id, description etc)
            mappingObj.AddProperty(new BasicMappingProperty() { name = "media_id", type = eESFieldType.LONG, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "group_id", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "media_type_id", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "wp_type_id", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "is_active", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "device_rule_id", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "like_counter", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "views", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "rating", type = eESFieldType.DOUBLE, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "votes", type = eESFieldType.INTEGER, analyzed = false, null_value = "0" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "start_date", type = eESFieldType.DATE, analyzed = false});
            mappingObj.AddProperty(new BasicMappingProperty() { name = "end_date", type = eESFieldType.DATE, analyzed = false});
            mappingObj.AddProperty(new BasicMappingProperty() { name = "final_date", type = eESFieldType.DATE, analyzed = false});
            mappingObj.AddProperty(new BasicMappingProperty() { name = "create_date", type = eESFieldType.DATE, analyzed = false});
            mappingObj.AddProperty(new BasicMappingProperty() { name = "update_date", type = eESFieldType.DATE, analyzed = false});
            mappingObj.AddProperty(new BasicMappingProperty() { name = "cache_date", type = eESFieldType.DATE, analyzed = false });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "user_types", type = eESFieldType.INTEGER, analyzed = false });
            mappingObj.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = "name", type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = false });
            mappingObj.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = "name.analyzed", type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = true, analyzer = "whitespace" });
            mappingObj.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = "description", type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = false });
            mappingObj.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = "description.analyzed", type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = true, analyzer = "whitespace" });

            #endregion            

            #region Add tags mapping
            InnerMappingProperty tags = new InnerMappingProperty() { name = "tags" };

            if (oGroupTags.Count > 0)
            {
                foreach (int tagID in oGroupTags.Keys)
                {
                    string sTagName = oGroupTags[tagID];

                    if (!string.IsNullOrEmpty(sTagName))
                    {
                        tags.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = sTagName.ToLower(), type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = false });
                        tags.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = string.Format("{0}.analyzed", sTagName.ToLower()), type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = true, analyzer = "whitespace" });          
                    }
                }
            }
            #endregion

            #region Add metas mapping
            InnerMappingProperty metas = new InnerMappingProperty() { name = "metas" };
            if (oMetasValuesByGroupId != null)
            {
                foreach (int groupID in oMetasValuesByGroupId.Keys)
                {
                    Dictionary<string, string> dMetas = oMetasValuesByGroupId[groupID];
                    if (dMetas != null)
                    {
                        foreach (string sMeta in dMetas.Keys)
                        {
                            string sMetaName = dMetas[sMeta];

                            if (!string.IsNullOrEmpty(sMetaName))
                            {
                                string sNullValue;
                                eESFieldType eMetaType;

                                GetMetaType(sMeta, out eMetaType, out sNullValue);
                                metas.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = sMetaName.ToLower(), type = eMetaType, null_value = sNullValue, analyzed = false });
                                metas.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = string.Format("{0}.analyzed", sMetaName.ToLower()), type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = true, analyzer = "whitespace" });
                            }
                        }
                    }
                }
            }

            #endregion

            mappingObj.AddProperty(tags);
            mappingObj.AddProperty(metas);

            return mappingObj.ToString();
        }

        private void GetMetaType(string sMeta, out eESFieldType sMetaType, out string sNullValue)
        {
            sMetaType = eESFieldType.STRING;
            sNullValue = string.Empty;

            if (sMeta.Contains(META_BOOL_SUFFIX))
            {
                sMetaType = eESFieldType.INTEGER;
                sNullValue = "0";
            }
            else if (sMeta.Contains(META_DOUBLE_SUFFIX))
            {
                sMetaType = eESFieldType.DOUBLE;
                sNullValue = "0.0";
            }
        }

        protected string EscapeValues(ref string values)
        {
            string sRes = string.Empty;

            if (!string.IsNullOrWhiteSpace(values))
            {
                
                sRes = values.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r\n", " ").Replace('\n', ' ').Replace('\b', ' ').Replace('\f', ' ').Replace('\t',' ').ToLower();
            }

            return sRes;
        }

        public string CreateEpgMapping(List<string> lMetasNames, List<string> lTags)
        {
            if (lMetasNames == null || lTags == null)
                return string.Empty;

            ESMappingObj mappingObj = new ESMappingObj("epg");

            ESRouting routing = new ESRouting() { path = "date_routing", required = true };
            mappingObj.SetRoting(routing);

            #region Add basic type mappings - (e.g. epg_id, group_id, description etc)
            mappingObj.AddProperty(new BasicMappingProperty() { name = "epg_id", analyzed = false, type = eESFieldType.LONG });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "group_id", analyzed = false, type = eESFieldType.INTEGER });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "epg_channel_id", analyzed = false, type = eESFieldType.INTEGER });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "is_active", analyzed = false, type = eESFieldType.INTEGER });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "start_date", analyzed = false, type = eESFieldType.DATE });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "end_date", analyzed = false, type = eESFieldType.DATE });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "date_routing", analyzed = false, type = eESFieldType.DATE });
            mappingObj.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = "name", type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = false });
            mappingObj.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = "name.analyzed", type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = true, analyzer = "whitespace" });
            mappingObj.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = "description", type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = false });
            mappingObj.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = "description.analyzed", type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = true, analyzer = "whitespace" });
            mappingObj.AddProperty(new BasicMappingProperty() { name = "cache_date", analyzed = false, type = eESFieldType.DATE });
            #endregion

            #region Add tags mapping
            InnerMappingProperty tags = new InnerMappingProperty() { name = "tags" };
            foreach (string sTagName in lTags)
            {
                if (!string.IsNullOrEmpty(sTagName))
                {
                    tags.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = sTagName.ToLower(), type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = false });
                    tags.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = string.Format("{0}.analyzed", sTagName.ToLower()), type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = true, analyzer = "whitespace" });

                }
            }
            #endregion

            #region Add metas mapping
            InnerMappingProperty metas = new InnerMappingProperty() { name = "metas" };

            foreach (string sMetaName in lMetasNames)
            {
                if (!string.IsNullOrEmpty(sMetaName))
                {
                    metas.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = sMetaName.ToLower(), type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = false });
                    metas.AddProperty(new ElasticSearch.Common.BasicMappingProperty() { name = string.Format("{0}.analyzed", sMetaName.ToLower()), type = ElasticSearch.Common.eESFieldType.STRING, null_value = "", analyzed = true, analyzer = "whitespace" });
                }
            }

            #endregion

            mappingObj.AddProperty(metas);
            mappingObj.AddProperty(tags);

            return mappingObj.ToString();

        }

        public string SerializeEpgObject(EpgCB oEpg)
        {
            StringBuilder sRecord = new StringBuilder();
            sRecord.Append("{ ");
            string name = oEpg.Name;
            string description = oEpg.Description;

            sRecord.AppendFormat("\"epg_id\": {0}, \"group_id\": {1}, \"epg_channel_id\": {2}, \"is_active\": {3}, \"start_date\": \"{4}\", \"end_date\": \"{5}\", \"name\": \"{6}\", \"name.analyzed\": \"{6}\", \"description\": \"{7}\", \"description.analyzed\": \"{7}\", \"cache_date\": \"{8}\", \"date_routing\": \"{9}\",",
                oEpg.EpgID, oEpg.GroupID, oEpg.ChannelID, (oEpg.isActive) ? 1 : 0, oEpg.StartDate.ToString("yyyyMMddHHmmss"), oEpg.EndDate.ToString("yyyyMMddHHmmss"), EscapeValues(ref name), EscapeValues(ref description), DateTime.UtcNow.ToString("yyyyMMddHHmmss"), oEpg.StartDate.ToString("yyyyMMdd000000"));

            #region add metas
            sRecord.Append(" \"metas\": {");

            if (oEpg.Metas != null && oEpg.Metas.Keys.Count > 0)
            {
                List<string> metaNameValues = new List<string>();
                string sTrimed;
                foreach (string sMetaName in oEpg.Metas.Keys)
                {
                    if (!string.IsNullOrWhiteSpace(sMetaName))
                    {
                        List<string> lMetaValues = oEpg.Metas[sMetaName];
                        if (lMetaValues != null && lMetaValues.Count > 0)
                        {
                            for (int i = 0; i < lMetaValues.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(lMetaValues[i]))
                                {
                                    sTrimed = lMetaValues[i].Trim();
                                    lMetaValues[i] = string.Format("\"{0}\"", EscapeValues(ref sTrimed));
                                }
                            }

                            metaNameValues.Add(string.Format(" \"{0}\": [ {1} ]", sMetaName.ToLower(), lMetaValues.Aggregate((current, next) => current + "," + next)));
                            metaNameValues.Add(string.Format(" \"{0}.analyzed\": [ {1} ]", sMetaName.ToLower(), lMetaValues.Aggregate((current, next) => current + "," + next)));
                        }
                    }
                }

                if (metaNameValues.Count > 0)
                    sRecord.Append(metaNameValues.Aggregate((current, next) => current + "," + next));
            }
            sRecord.Append("},");
            #endregion

            #region add tags
            sRecord.Append(" \"tags\": {");

            if (oEpg.Tags != null && oEpg.Tags.Keys.Count > 0)
            {
                List<string> tagNameValues = new List<string>();
                string sTrimed;
                foreach (string sTagName in oEpg.Tags.Keys)
                {
                    if (!string.IsNullOrEmpty(sTagName))
                    {
                        List<string> lTagValues = oEpg.Tags[sTagName];
                        if (lTagValues != null && lTagValues.Count > 0)
                        {                            
                            for (int i = 0; i < lTagValues.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(lTagValues[i]))
                                {
                                    sTrimed = lTagValues[i].Trim();
                                    lTagValues[i] = string.Format("\"{0}\"", EscapeValues(ref sTrimed));
                                }
                            }

                            tagNameValues.Add(string.Format(" \"{0}\": [ {1} ]", sTagName.ToLower(), lTagValues.Aggregate((current, next) => current + "," + next)));
                            tagNameValues.Add(string.Format(" \"{0}.analyzed\": [ {1} ]", sTagName.ToLower(), lTagValues.Aggregate((current, next) => current + "," + next)));
                        }
                    }
                }
                if (tagNameValues.Count > 0)
                    sRecord.Append(tagNameValues.Aggregate((current, next) => current + "," + next));
            }

            sRecord.Append(" }");

            #endregion

            sRecord.Append(" }");

            return sRecord.ToString();
        }

        
    }
}
