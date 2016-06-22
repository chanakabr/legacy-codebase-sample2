
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects.SearchObjects;
using KLogMonitor;

namespace ElasticSearch.Searcher
{
    public class ESMediaQueryBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly string AND_CONDITION = "AND";
        public static readonly string OR_CONDITION = "OR";
        public static readonly string METAS = "METAS";
        public static readonly string TAGS = "TAGS";
        protected readonly int MAX_RESULTS;

        public MediaSearchObj oSearchObject { get; set; }

        public eQueryAnalyzer eAnalyzer { get; set; }
        public bool bAnalyzeWildcards { get; set; }
        public int m_nGroupID { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public List<string> ReturnFields { get; protected set; }
        public eQueryType QueryType { get; set; }

        public ESMediaQueryBuilder()
            : this(0, null)
        {
        }

        public ESMediaQueryBuilder(int nGroupID, MediaSearchObj searchObject)
        {
            oSearchObject = searchObject;
            m_nGroupID = nGroupID;
            ReturnFields = new List<string>() { "\"_id\"", "\"_index\"", "\"_type\"", "\"_score\"", "\"group_id\"", "\"media_id\"", "\"name\"", "\"cache_date\"", "\"update_date\"" };

            string sMaxResults = Common.Utils.GetWSURL("MAX_RESULTS");

            if (!int.TryParse(sMaxResults, out MAX_RESULTS))
                MAX_RESULTS = 10000;
        }

        public virtual FilteredQuery BuildChannelFilteredQuery(bool bAddDeviceRuleID = true)
        {
            FilteredQuery query = null;

            if (oSearchObject == null)
            {
                return query;
            }

            query = new FilteredQuery();
            QueryFilter filter = new QueryFilter();

            BaseFilterCompositeType filterParent = new FilterCompositeType(CutWith.AND);

            ESTerm groupTerm = new ESTerm(true) { Key = "group_id", Value = m_nGroupID.ToString() };

            ESTerms permittedWatcFilter = new ESTerms(true);
            if (!string.IsNullOrEmpty(oSearchObject.m_sPermittedWatchRules))
            {
                permittedWatcFilter.Key = "wp_type_id";
                List<string> permittedValues = permittedWatcFilter.Value;
                foreach (string value in oSearchObject.m_sPermittedWatchRules.Split(' '))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        permittedValues.Add(value);
                    }
                }
            }

            ESTerm isActiveTerm = new ESTerm(true) { Key = "is_active", Value = "1" };

            string sNow = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string sMax = DateTime.MaxValue.ToString("yyyyMMddHHmmss");

            ESRange startDateRange = new ESRange(false);
            if (oSearchObject.m_bUseStartDate)
            {
                startDateRange.Key = "start_date";
                string sMin = DateTime.MinValue.ToString("yyyyMMddHHmmss");
                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sNow));
            }

            ESRange endDateRange = new ESRange(false);
            endDateRange.Key = (oSearchObject.m_bUseFinalEndDate) ? "final_date" : "end_date";
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sNow));
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));

            ESTerm mediaTerm = new ESTerm(true);
            if (oSearchObject.m_nMediaID > 0)
            {
                mediaTerm.Key = "media_id";
                mediaTerm.Value = oSearchObject.m_nMediaID.ToString();
                mediaTerm.isNot = true;
            }

            ESTerms userTypeTerm = new ESTerms(true);
            userTypeTerm.Key = "user_types";
            userTypeTerm.Value.Add("0");
            if (oSearchObject.m_nUserTypeID > 0)
            {
                userTypeTerm.Value.Add(oSearchObject.m_nUserTypeID.ToString());
            }

            IESTerm mediaTypesTerm = null;
            if (!string.IsNullOrEmpty(oSearchObject.m_sMediaTypes) && !oSearchObject.m_sMediaTypes.Equals("0"))
            {
                mediaTypesTerm = new ESTerms(true);

                log.Debug("Info - media type = " + oSearchObject.m_sMediaTypes);
                (mediaTypesTerm as ESTerms).Key = "media_type_id";
                string[] mediaTypeArr = oSearchObject.m_sMediaTypes.Split(';');
                string trimed;
                foreach (string mediaType in mediaTypeArr)
                {
                    if (!string.IsNullOrWhiteSpace(mediaType))
                    {
                        trimed = mediaType.Trim();
                        if (trimed.Equals("0"))
                        {
                            (mediaTypesTerm as ESTerms).Value.Clear();
                            break;
                        }
                        (mediaTypesTerm as ESTerms).Value.Add(mediaType.Trim());
                    }
                }
            }

            // If there is no media type term, at least check that it is positive
            if (mediaTypesTerm == null || (mediaTypesTerm as ESTerms).Value.Count == 0)
            {
                mediaTypesTerm = new ESRange(true);
                (mediaTypesTerm as ESRange).Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GT, "0"));
            }

            ESTerms deviceRulesTerms = new ESTerms(true) { Key = "device_rule_id" };
            if (bAddDeviceRuleID)
            {
                deviceRulesTerms.Value.Add("0");

                if (oSearchObject.m_nDeviceRuleId != null && oSearchObject.m_nDeviceRuleId.Length > 0)
                {
                    foreach (int deviceRuleId in oSearchObject.m_nDeviceRuleId)
                    {
                        deviceRulesTerms.Value.Add(deviceRuleId.ToString());
                    }
                }
            }

            FilterCompositeType oGroupWPComposite = new FilterCompositeType(CutWith.OR);

            oGroupWPComposite.AddChild(groupTerm);
            oGroupWPComposite.AddChild(permittedWatcFilter);

            filterParent.AddChild(oGroupWPComposite);
            filterParent.AddChild(isActiveTerm);
            filterParent.AddChild(startDateRange);
            filterParent.AddChild(endDateRange);
            filterParent.AddChild(mediaTerm);
            filterParent.AddChild(userTypeTerm);
            filterParent.AddChild(mediaTypesTerm);
            filterParent.AddChild(deviceRulesTerms);

            if (QueryType == eQueryType.EXACT)
            {
                if (oSearchObject.m_oOrder.m_eOrderBy != OrderBy.RELATED)
                {
                    FilterCompositeType andComposite = this.FilterMetasAndTagsConditions(oSearchObject.m_dAnd, CutWith.AND);
                    FilterCompositeType orComposite = this.FilterMetasAndTagsConditions(oSearchObject.m_dOr, CutWith.OR);
                    FilterCompositeType generatedComposite = this.FilterMetasAndTagsConditions(oSearchObject.m_lFilterTagsAndMetas, (CutWith)oSearchObject.m_eFilterTagsAndMetasCutWith);

                    filterParent.AddChild(andComposite);
                    filterParent.AddChild(orComposite);
                    filterParent.AddChild(generatedComposite);
                }
                else
                {
                    BoolQuery oAndBoolQuery = this.QueryRelatedMetasAndTagsConditions(oSearchObject.m_dAnd, CutWith.AND);
                    BoolQuery oOrBoolQuery = this.QueryRelatedMetasAndTagsConditions(oSearchObject.m_dOr, CutWith.OR);
                    BoolQuery oMultiFilterBoolQuery = this.QueryRelatedMetasAndTagsConditions(oSearchObject.m_lFilterTagsAndMetas, (CutWith)oSearchObject.m_eFilterTagsAndMetasCutWith);

                    BoolQuery oBoolQuery = new BoolQuery();
                    oBoolQuery.AddChild(oAndBoolQuery, CutWith.OR);
                    oBoolQuery.AddChild(oOrBoolQuery, CutWith.OR);
                    oBoolQuery.AddChild(oMultiFilterBoolQuery, CutWith.OR);

                    query.Query = oBoolQuery;
                }
            }
            else if (QueryType == eQueryType.BOOLEAN)
            {
                BoolQuery oAndBoolQuery = this.QueryMetasAndTagsConditions(oSearchObject.m_dAnd, CutWith.AND);
                BoolQuery oOrBoolQuery = this.QueryMetasAndTagsConditions(oSearchObject.m_dOr, CutWith.OR);
                BoolQuery oMultiFilterBoolQuery = this.QueryMetasAndTagsConditions(oSearchObject.m_lFilterTagsAndMetas, (CutWith)oSearchObject.m_eFilterTagsAndMetasCutWith);

                BoolQuery oBoolQuery = new BoolQuery();
                oBoolQuery.AddChild(oAndBoolQuery, CutWith.OR);
                oBoolQuery.AddChild(oOrBoolQuery, CutWith.OR);
                oBoolQuery.AddChild(oMultiFilterBoolQuery, CutWith.OR);

                query.Query = oBoolQuery;

            }
            else if (QueryType == eQueryType.PHRASE_PREFIX)
            {
                MultiMatchQuery multiMatch = GetMultiMatchQuery();
                query.Query = multiMatch;
            }

            FillFilterSettings(ref filter, filterParent);
            query.Filter = filter;

            if (PageSize <= 0)
                PageSize = MAX_RESULTS;

            int fromIndex = (PageIndex <= 0) ? 0 : PageSize * PageIndex;
            query.PageSize = PageSize;
            query.PageIndex = PageIndex;
            query.ESSort.Add(new ESOrderObj() { m_eOrderDir = oSearchObject.m_oOrder.m_eOrderDir, m_sOrderValue = FilteredQuery.GetESSortValue(oSearchObject.m_oOrder) });

            return query;
        }

        public virtual string BuildSearchQueryString(bool bIgnoreDeviceRuleID = true, bool bAddActive = true)
        {
            string sResult = string.Empty;

            if (oSearchObject == null)
            {
                return sResult;
            }

            StringBuilder sbFilteredQuery = new StringBuilder();
            string sQuery = string.Empty;

            QueryFilter filter = new QueryFilter();

            BaseFilterCompositeType filterParent = new FilterCompositeType(CutWith.AND);

            ESTerm groupTerm = new ESTerm(true)
            {
                Key = "group_id",
                Value = m_nGroupID.ToString()
            };

            ESTerms permittedWatcFilter = new ESTerms(true);
            if (!string.IsNullOrEmpty(oSearchObject.m_sPermittedWatchRules))
            {
                permittedWatcFilter.Key = "wp_type_id";
                List<string> permittedValues = permittedWatcFilter.Value;
                foreach (string value in oSearchObject.m_sPermittedWatchRules.Split(' '))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        permittedValues.Add(value);
                    }
                }
            }

            ESTerm isActiveTerm = new ESTerm(true)
            {
                Key = "is_active",
                Value = "1"
            };

            string sNow = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string sMax = DateTime.MaxValue.ToString("yyyyMMddHHmmss");

            ESRange startDateRange = new ESRange(false);
            if (oSearchObject.m_bUseStartDate)
            {
                startDateRange.Key = "start_date";
                string sMin = DateTime.MinValue.ToString("yyyyMMddHHmmss");
                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sNow));
            }

            ESRange endDateRange = new ESRange(false);
            endDateRange.Key = (oSearchObject.m_bUseFinalEndDate) ? "final_date" : "end_date";
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sNow));
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));

            ESTerm mediaTerm = new ESTerm(true);
            if (oSearchObject.m_nMediaID > 0)
            {
                mediaTerm.Key = "media_id";
                mediaTerm.Value = oSearchObject.m_nMediaID.ToString();
                mediaTerm.isNot = true;
            }

            ESTerms userTypeTerm = new ESTerms(true);
            userTypeTerm.Key = "user_types";
            userTypeTerm.Value.Add("0");
            if (oSearchObject.m_nUserTypeID > 0)
            {
                userTypeTerm.Value.Add(oSearchObject.m_nUserTypeID.ToString());
            }

            ESTerms mediaTypesTerms = new ESTerms(true);
            if (!string.IsNullOrEmpty(oSearchObject.m_sMediaTypes) && !oSearchObject.m_sMediaTypes.Equals("0"))
            {
                mediaTypesTerms.Key = "media_type_id";
                string[] mediaTypeArr = oSearchObject.m_sMediaTypes.Split(';');
                foreach (string mediaType in mediaTypeArr)
                {
                    if (!string.IsNullOrWhiteSpace(mediaType))
                    {
                        mediaTypesTerms.Value.Add(mediaType.Trim());
                    }
                }
            }

            ESRange positiveMediaTypeRange = new ESRange(true);
            positiveMediaTypeRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GT, "0"));

            if (!bIgnoreDeviceRuleID)
            {
                ESTerms deviceRulesTerms = new ESTerms(true)
                {
                    Key = "device_rule_id"
                };
                {
                    deviceRulesTerms.Value.Add("0");

                    if (oSearchObject.m_nDeviceRuleId != null && oSearchObject.m_nDeviceRuleId.Length > 0)
                    {
                        foreach (int deviceRuleId in oSearchObject.m_nDeviceRuleId)
                        {
                            deviceRulesTerms.Value.Add(deviceRuleId.ToString());
                        }
                    }
                }

                filterParent.AddChild(deviceRulesTerms);
            }

            // region term 
            if (oSearchObject.regionIds != null && oSearchObject.regionIds.Count > 0)
            {

                ESTerms regionsTerms = new ESTerms(true)
                {
                    Key = "regions"
                };

                regionsTerms.Value.AddRange(oSearchObject.regionIds.Select(region => region.ToString()));

                if (oSearchObject.linearChannelMediaTypes == null || oSearchObject.linearChannelMediaTypes.Count == 0)
                {
                    filterParent.AddChild(regionsTerms);
                }
                else
                {
                    FilterCompositeType regionComposite = new FilterCompositeType(CutWith.OR);
                    FilterCompositeType emptyRegionAndComposite = new FilterCompositeType(CutWith.AND);

                    ESTerm emptyRegionTerm = new ESTerm(true)
                    {
                        Key = "regions",
                        Value = "0"
                    };

                    ESTerms linearMediaTypes = new ESTerms(true)
                    {
                        Key = "media_type_id",
                        isNot = true
                    };

                    linearMediaTypes.Value.AddRange(oSearchObject.linearChannelMediaTypes);

                    // region = 0 and it is NOT linear media
                    emptyRegionAndComposite.AddChild(emptyRegionTerm);
                    emptyRegionAndComposite.AddChild(linearMediaTypes);

                    // It is either in the desired region or it is in region 0 and not linear media
                    regionComposite.AddChild(regionsTerms);
                    regionComposite.AddChild(emptyRegionAndComposite);

                    filterParent.AddChild(regionComposite);
                }
            }

            FilterCompositeType oGroupWPComposite = new FilterCompositeType(CutWith.OR);

            oGroupWPComposite.AddChild(groupTerm);
            oGroupWPComposite.AddChild(permittedWatcFilter);

            filterParent.AddChild(oGroupWPComposite);
            
            if (bAddActive)
                filterParent.AddChild(isActiveTerm);

            filterParent.AddChild(startDateRange);
            filterParent.AddChild(endDateRange);
            filterParent.AddChild(mediaTerm);
            filterParent.AddChild(userTypeTerm);
            filterParent.AddChild(mediaTypesTerms);
            filterParent.AddChild(positiveMediaTypeRange);

            if (QueryType == eQueryType.EXACT)
            {
                if (oSearchObject.m_oOrder.m_eOrderBy != OrderBy.RELATED)
                {
                    FilterCompositeType andComposite = this.FilterMetasAndTagsConditions(oSearchObject.m_dAnd, CutWith.AND);
                    FilterCompositeType orComposite = this.FilterMetasAndTagsConditions(oSearchObject.m_dOr, CutWith.OR);
                    FilterCompositeType generatedComposite = this.FilterMetasAndTagsConditions(oSearchObject.m_lFilterTagsAndMetas, (CutWith)oSearchObject.m_eFilterTagsAndMetasCutWith);

                    filterParent.AddChild(andComposite);
                    filterParent.AddChild(orComposite);
                    filterParent.AddChild(generatedComposite);
                }
                else
                {
                    BoolQuery oAndBoolQuery = this.QueryRelatedMetasAndTagsConditions(oSearchObject.m_dAnd, CutWith.AND);
                    BoolQuery oOrBoolQuery = this.QueryRelatedMetasAndTagsConditions(oSearchObject.m_dOr, CutWith.OR);
                    BoolQuery oMultiFilterBoolQuery = this.QueryRelatedMetasAndTagsConditions(oSearchObject.m_lFilterTagsAndMetas, (CutWith)oSearchObject.m_eFilterTagsAndMetasCutWith);

                    BoolQuery oBoolQuery = new BoolQuery();
                    oBoolQuery.AddChild(oAndBoolQuery, CutWith.OR);
                    oBoolQuery.AddChild(oOrBoolQuery, CutWith.OR);
                    oBoolQuery.AddChild(oMultiFilterBoolQuery, CutWith.OR);

                    sQuery = oBoolQuery.ToString();
                }
            }
            else if (QueryType == eQueryType.BOOLEAN)
            {
                BoolQuery oAndBoolQuery = this.QueryMetasAndTagsConditions(oSearchObject.m_dAnd, CutWith.AND);
                BoolQuery oOrBoolQuery = this.QueryMetasAndTagsConditions(oSearchObject.m_dOr, CutWith.OR);
                BoolQuery oMultiFilterBoolQuery = this.QueryMetasAndTagsConditions(oSearchObject.m_lFilterTagsAndMetas, (CutWith)oSearchObject.m_eFilterTagsAndMetasCutWith);

                BoolQuery oBoolQuery = new BoolQuery();
                oBoolQuery.AddChild(oAndBoolQuery, CutWith.AND);
                oBoolQuery.AddChild(oOrBoolQuery, CutWith.AND);
                oBoolQuery.AddChild(oMultiFilterBoolQuery, CutWith.AND);

                sQuery = oBoolQuery.ToString();

            }
            else if (QueryType == eQueryType.PHRASE_PREFIX)
            {
                MultiMatchQuery multiMatch = GetMultiMatchQuery();
                sQuery = multiMatch.ToString();
            }

            FillFilterSettings(ref filter, filterParent);

            if (PageSize <= 0)
                PageSize = MAX_RESULTS;

            int fromIndex = (PageIndex <= 0) ? 0 : PageSize * PageIndex;

            sbFilteredQuery.Append("{");
            sbFilteredQuery.AppendFormat(" \"size\": {0}, ", PageSize);
            sbFilteredQuery.AppendFormat(" \"from\": {0}, ", fromIndex);

            string sSort = (QueryType == eQueryType.EXACT) ? GetSort(oSearchObject.m_oOrder, false) : GetSort(oSearchObject.m_oOrder, true);

            if (ReturnFields.Count > 0)
            {
                sbFilteredQuery.Append("\"fields\": [");

                sbFilteredQuery.Append(ReturnFields.Aggregate((current, next) => string.Format("{0}, {1}", current, next)));

                sbFilteredQuery.Append("], ");
            }

            sbFilteredQuery.AppendFormat("{0}, ", sSort);
            sbFilteredQuery.Append(" \"query\": { \"filtered\": {");

            if (!string.IsNullOrEmpty(sQuery))
            {
                sbFilteredQuery.AppendFormat(" \"query\": {0},", sQuery.ToString());
            }
            sbFilteredQuery.Append(filter.ToString());
            sbFilteredQuery.Append(" } } }");
            sResult = sbFilteredQuery.ToString();

            return sResult;
        }

        private void FillFilterSettings(ref QueryFilter filter, BaseFilterCompositeType filterSettings)
        {
            if (IsUseIPNOFiltering())
            {
                filter.FilterSettings = new IPNOFilterCompositeType(filterSettings, oSearchObject.m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt,
                    oSearchObject.m_lOrMediaNotInAnyOfTheseChannelsDefinitions, CutWith.AND, CutWith.OR, CutWith.AND,
                    CutWith.OR, false, true);
            }
            else
            {
                filter.FilterSettings = filterSettings;
            }

        }

        private bool IsUseIPNOFiltering()
        {
            return oSearchObject != null && oSearchObject.m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt != null && (oSearchObject.m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt.Count > 0 || (oSearchObject.m_lOrMediaNotInAnyOfTheseChannelsDefinitions != null && oSearchObject.m_lOrMediaNotInAnyOfTheseChannelsDefinitions.Count > 0));
        }

        public string BuildMultiChannelQuery(List<MediaSearchObj> searchObj)
        {
            string sResult = string.Empty;





            return sResult;
        }

        public string GetDocumentsByIdsQuery<T>(List<T> lMediaIDs, OrderObj oOrder) where T : struct, IComparable,
            IComparable<T>, IConvertible, IEquatable<T>, IFormattable // constraints for numeric.
        {
            if (lMediaIDs == null || lMediaIDs.Count == 0)
                return string.Empty;

            StringBuilder sQuery = new StringBuilder();
            QueryFilter oQueryFilter = new QueryFilter();

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.OR);
            ESTerms mediaIdTerms = new ESTerms(true);

            mediaIdTerms.Key = "media_id";

            foreach (T id in lMediaIDs)
            {
                mediaIdTerms.Value.Add(id.ToString());
            }

            filter.AddChild(mediaIdTerms);
            FillFilterSettings(ref oQueryFilter, filter);

            sQuery.Append("{");

            string sSort = GetSort(oOrder, false);

            sQuery.AppendFormat(" \"size\": {0}, ", MAX_RESULTS);
            if (ReturnFields.Count > 0)
            {
                sQuery.Append("\"fields\": [");

                sQuery.Append(ReturnFields.Aggregate((current, next) => string.Format("{0}, {1}", current, next)));

                sQuery.Append("], ");
            }


            sQuery.AppendFormat("{0}, ", sSort);
            sQuery.Append(" \"query\": { \"filtered\": {");

            sQuery.Append(oQueryFilter.ToString());
            sQuery.Append(" } } }");

            return sQuery.ToString();
        }

        private MultiMatchQuery GetMultiMatchQuery()
        {

            MultiMatchQuery multiMatchQuery = new MultiMatchQuery();

            foreach (SearchValue searchValue in oSearchObject.m_dOr)
            {
                multiMatchQuery.Fields.Add(Common.Utils.GetKeyNameWithPrefix(searchValue.m_sKey, searchValue.m_sKeyPrefix));
            }

            if (multiMatchQuery.Fields.Count > 0)
                multiMatchQuery.Query = oSearchObject.m_sName.ToLower();

            return multiMatchQuery;


        }

        public string BuildMediaAutoCompleteQuery()
        {
            FilteredQuery filteredQuery = new FilteredQuery();

            ESTerm isActiveTerm = new ESTerm(true) { Key = "is_active", Value = "1" };
            QueryFilter filter = new QueryFilter();
            BaseFilterCompositeType filterParent = new FilterCompositeType(CutWith.AND);
            filterParent.AddChild(isActiveTerm);
            //filter.FilterSettings.AddChild(isActiveTerm);

            string sNow = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string sMax = DateTime.MaxValue.ToString("yyyyMMddHHmmss");

            ESRange startDateRange = new ESRange(false);
            if (oSearchObject.m_bUseStartDate)
            {
                startDateRange.Key = "start_date";
                string sMin = DateTime.MinValue.ToString("yyyyMMddHHmmss");
                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sNow));
            }

            ESRange endDateRange = new ESRange(false);
            endDateRange.Key = (oSearchObject.m_bUseFinalEndDate) ? "final_date" : "end_date";
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sNow));
            endDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));

            ESTerms mediaTypesTerms = new ESTerms(true);
            if (!string.IsNullOrEmpty(oSearchObject.m_sMediaTypes) && !oSearchObject.m_sMediaTypes.Equals("0"))
            {
                mediaTypesTerms.Key = "media_type_id";
                string[] mediaTypeArr = oSearchObject.m_sMediaTypes.Split(';');
                foreach (string mediaType in mediaTypeArr)
                {
                    if (!string.IsNullOrWhiteSpace(mediaType))
                    {
                        mediaTypesTerms.Value.Add(mediaType.Trim());
                    }
                }
            }

            // region term 
            if (oSearchObject.regionIds != null && oSearchObject.regionIds.Count > 0)
            {
                FilterCompositeType regionComposite = new FilterCompositeType(CutWith.OR);
                FilterCompositeType emptyRegionAndComposite = new FilterCompositeType(CutWith.AND);

                ESTerms regionsTerms = new ESTerms(true)
                {
                    Key = "regions"
                };

                regionsTerms.Value.AddRange(oSearchObject.regionIds.Select(region => region.ToString()));

                ESTerm emptyRegionTerm = new ESTerm(true)
                {
                    Key = "regions",
                    Value = "0"
                };

                ESTerms linearMediaTypes = new ESTerms(true)
                {
                    Key = "media_type_id",
                    isNot = true
                };

                linearMediaTypes.Value.AddRange(oSearchObject.linearChannelMediaTypes);

                // region = 0 and it is NOT linear media
                emptyRegionAndComposite.AddChild(emptyRegionTerm);
                emptyRegionAndComposite.AddChild(linearMediaTypes);

                // It is either in the desired region or it is in region 0 and not linear media
                regionComposite.AddChild(regionsTerms);
                regionComposite.AddChild(emptyRegionAndComposite);

                filterParent.AddChild(regionComposite);
            }

            filterParent.AddChild(isActiveTerm);
            filterParent.AddChild(startDateRange);
            filterParent.AddChild(endDateRange);
            filterParent.AddChild(mediaTypesTerms);
            FillFilterSettings(ref filter, filterParent);

            MultiMatchQuery multiMatchQuery = new MultiMatchQuery();
            foreach (SearchValue searchValue in oSearchObject.m_dOr)
            {
                multiMatchQuery.Fields.Add(Common.Utils.GetKeyNameWithPrefix(searchValue.m_sKey.ToLower(), searchValue.m_sKeyPrefix.ToLower()));
            }

            multiMatchQuery.Query = oSearchObject.m_sName.ToLower();

            filteredQuery.Filter = filter;
            filteredQuery.Query = multiMatchQuery;
            filteredQuery.PageIndex = oSearchObject.m_nPageIndex;
            filteredQuery.PageSize = oSearchObject.m_nPageSize;

            return filteredQuery.ToString();

        }

        private string GetSort(OrderObj oOrderObj, bool bOrderByScore)
        {
            StringBuilder sSort = new StringBuilder();
            sSort.Append(" \"sort\": [{");

            if (oOrderObj.m_eOrderBy == OrderBy.META)
            {
                string sAnalyzedMeta = string.Format("metas.{0}", oOrderObj.m_sOrderValue.ToLower());
                sSort.AppendFormat("\"{0}\": ", sAnalyzedMeta);
                ReturnFields.Add(string.Format("\"{0}\"", sAnalyzedMeta));

            }
            else if (oOrderObj.m_eOrderBy == OrderBy.ID)
            {
                sSort.Append(" \"_id\": ");
            }
            else if (oOrderObj.m_eOrderBy == OrderBy.RELATED)
            {
                sSort.Append(" \"_score\": ");
            }
            else if (oOrderObj.m_eOrderBy == OrderBy.VOTES_COUNT)
            {
                sSort.Append(" \"votes\": ");
            }
            else
            {
                sSort.AppendFormat(" \"{0}\": ", Enum.GetName(typeof(OrderBy), oOrderObj.m_eOrderBy).ToLower());
            }

            if (sSort.Length > 0)
            {
                sSort.Append(" {");
                sSort.AppendFormat("\"order\": \"{0}\"", oOrderObj.m_eOrderDir.ToString().ToLower());
                sSort.Append("}}");
            }

            //we always add the score at the end of the sorting so that our records will be in best order when using wildcards in the query itself
            if (bOrderByScore)
                sSort.Append(", \"_score\"");

            // Always add sort by _id to avoid ES weirdness of same sort-value 
            sSort.Append(", { \"_id\": { \"order\": \"desc\" } }");

            sSort.Append(" ]");

            return sSort.ToString();
        }

        private FilterCompositeType FilterMetasAndTagsConditions(List<SearchValue> oSearchList, CutWith oAndOrCondition)
        {
            if (oSearchList == null)
                return null;
            FilterCompositeType parent = new FilterCompositeType(oAndOrCondition);

            foreach (SearchValue searchValue in oSearchList)
            {
                string sSearchKey = Common.Utils.GetKeyNameWithPrefix(searchValue.m_sKey.ToLower(), searchValue.m_sKeyPrefix);

                if (searchValue.m_eInnerCutWith == ApiObjects.SearchObjects.CutWith.AND)
                {
                    FilterCompositeType composite = new FilterCompositeType(CutWith.AND);
                    foreach (string value in searchValue.m_lValue)
                    {
                        composite.AddChild(new ESTerm(false) { Key = sSearchKey, Value = value.ToLower() });
                    }
                    parent.AddChild(composite);
                }
                else if (searchValue.m_eInnerCutWith == ApiObjects.SearchObjects.CutWith.OR)
                {
                    FilterCompositeType composite = new FilterCompositeType(CutWith.OR);
                    if (searchValue.m_lValue.Count > 0)
                    {
                        ESTerms terms = new ESTerms(false);
                        terms.Key = sSearchKey;
                        terms.Value.AddRange(searchValue.m_lValue.ConvertAll(val => val.ToLower()));
                        composite.AddChild(terms);
                    }
                    parent.AddChild(composite);
                }
            }

            return parent;

        }

        private static readonly char[] splitChars = new char[] { ' ', ';' };

        protected BoolQuery QueryMetasAndTagsConditions(List<SearchValue> oSearchList, CutWith oAndOrCondition)
        {
            BoolQuery oBoolQuery = new BoolQuery();

            List<string> lMetasAndTagConditions = null;

            if (oSearchList != null && oSearchList.Count > 0)
            {
                lMetasAndTagConditions = new List<string>();

                foreach (SearchValue searchValue in oSearchList)
                {
                    BoolQuery oValueBoolQuery = new BoolQuery();
                    if (!string.IsNullOrEmpty(searchValue.m_sKey))
                    {
                        string sSearchKey = string.Concat(Common.Utils.GetKeyNameWithPrefix(searchValue.m_sKey.ToLower(), searchValue.m_sKeyPrefix),
                                                          ".analyzed");

                        foreach (string sValue in searchValue.m_lValue)
                        {
                            if (string.IsNullOrEmpty(sValue))
                                continue;

                            ESMatchQuery matchQuery = new ESMatchQuery(null) { eOperator = CutWith.AND, Field = sSearchKey, Query = sValue };
                            oValueBoolQuery.AddChild(matchQuery, searchValue.m_eInnerCutWith);
                        }
                    }

                    oBoolQuery.AddChild(oValueBoolQuery, oAndOrCondition);
                }
            }

            return oBoolQuery;
        }

        protected BoolQuery QueryRelatedMetasAndTagsConditions(List<SearchValue> oSearchList, CutWith oAndOrCondition)
        {
            BoolQuery oBoolQuery = new BoolQuery();

            List<string> lMetasAndTagConditions = null;

            if (oSearchList != null && oSearchList.Count > 0)
            {
                lMetasAndTagConditions = new List<string>();

                foreach (SearchValue searchValue in oSearchList)
                {
                    BoolQuery oValueBoolQuery = new BoolQuery();
                    if (!string.IsNullOrEmpty(searchValue.m_sKey))
                    {
                        string sSearchKey = (string.IsNullOrEmpty(searchValue.m_sKeyPrefix)) ?
                            searchValue.m_sKey.ToLower() : string.Format("{0}.{1}", searchValue.m_sKeyPrefix.ToLower(), searchValue.m_sKey.ToLower());

                        if (searchValue.m_eInnerCutWith == ApiObjects.SearchObjects.CutWith.AND)
                        {

                            FilterCompositeType composite = new FilterCompositeType(CutWith.AND);
                            foreach (string value in searchValue.m_lValue)
                            {
                                oValueBoolQuery.AddChild(new ESTerm(false) { Key = sSearchKey, Value = value.ToLower() }, CutWith.AND);
                            }
                        }
                        else if (searchValue.m_eInnerCutWith == ApiObjects.SearchObjects.CutWith.OR)
                        {
                            FilterCompositeType composite = new FilterCompositeType(CutWith.OR);
                            if (searchValue.m_lValue.Count > 0)
                            {

                                ESTerms terms = new ESTerms(false);
                                terms.Key = sSearchKey;
                                terms.Value.AddRange(searchValue.m_lValue.ConvertAll(val => val.ToLower()));
                                oValueBoolQuery.AddChild(terms, CutWith.OR);
                            }
                        }
                    }

                    oBoolQuery.AddChild(oValueBoolQuery, oAndOrCondition);
                }
            }

            return oBoolQuery;
        }
    }

    public enum eQueryAnalyzer
    {
        NOT_ANALYZED = 0,
        WHITESPACE = 1
    }

    public enum eQueryType
    {
        EXACT = 0,
        BOOLEAN = 1,
        PHRASE_PREFIX = 2
    }
}
