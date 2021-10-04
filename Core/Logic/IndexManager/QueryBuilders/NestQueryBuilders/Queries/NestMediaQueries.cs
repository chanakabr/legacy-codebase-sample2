using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.IndexManager.NestData;
using ApiObjects.SearchObjects;
using MoreLinq;
using Nest;
using TVinciShared;

namespace ApiLogic.IndexManager.QueryBuilders
{
    public class NestMediaQueries
    {
        public NestMediaQueries()
        {
        }

        public QueryContainer GetMediaVirtualAssetTerms(UnifiedSearchDefinitions searchDefinitions)
        {
            var anyKsql = (searchDefinitions.ksqlAssetTypes?.Count == 0 ||
                           searchDefinitions.ksqlAssetTypes.Contains("media"))
                          && searchDefinitions.mediaTypes?.Count == 0
                          && searchDefinitions.ObjectVirtualAssetIds?.Count > 0;

            if (!anyKsql)
                return null;

            var queryContainerDescriptor = new QueryContainerDescriptor<NestMedia>();
             queryContainerDescriptor.Terms(x => x
                .Field(f => f.MediaTypeId)
                .Terms(searchDefinitions.ObjectVirtualAssetIds));
             return queryContainerDescriptor;
        }

        public List<QueryContainer> GetMediaParentalRules(UnifiedSearchDefinitions searchDefinitions)
        {
            if (!searchDefinitions.mediaParentalRulesTags.Any())
            {
                return null;
            }
            
            var terms = new List<QueryContainer>();
            // Run on all tags and their values
            foreach (var tagValues in searchDefinitions.mediaParentalRulesTags)
            {
                // Create a Not-in terms for each of the tags
                var termsQueryDescriptor = new QueryContainerDescriptor<NestMedia>();
                var termsValues = tagValues.Value.Select(x => x.ToLower());
                termsQueryDescriptor.Terms(t =>
                    t.Field(f => f.Tags[searchDefinitions.langauge.Code]).Terms(termsValues));
                ;
                
                terms.Add(termsQueryDescriptor);
            }

            return terms;
            
        }

        public QueryContainer GetMediaGeoBlockRules(UnifiedSearchDefinitions searchDefinitions)
        {
            var anyGeoBlock = searchDefinitions.geoBlockRules != null && searchDefinitions.geoBlockRules.Count > 0;
            if (!anyGeoBlock)
            {
                return null;
            }

            var queryContainerDescriptor = new QueryContainerDescriptor<NestMedia>();
            queryContainerDescriptor.Terms(t =>
                t.Field(f => f.GeoBlockRule).Terms(searchDefinitions.geoBlockRules));
            return queryContainerDescriptor;
        }

        public QueryContainer GetMediaRegionTerms(MediaSearchObj searchDefinitions)
        {
            var isAllowedToViewInactiveAssets = true;
            var regionIds = searchDefinitions.regionIds;
            var linearChannelMediaTypes = searchDefinitions.linearChannelMediaTypes;
            
            return GetMediaRegionTerms(isAllowedToViewInactiveAssets, regionIds, linearChannelMediaTypes);
        }
        
        public QueryContainer GetMediaRegionTerms(UnifiedSearchDefinitions searchDefinitions)
        {
            var isAllowedToViewInactiveAssets = searchDefinitions.isAllowedToViewInactiveAssets;
            var regionIds = searchDefinitions.regionIds;
            var linearChannelMediaTypes = searchDefinitions.linearChannelMediaTypes;
            
            return GetMediaRegionTerms(isAllowedToViewInactiveAssets, regionIds, linearChannelMediaTypes);
        }

        private QueryContainer GetMediaRegionTerms(bool isAllowedToViewInactiveAssets, List<int> regionIds,
            List<string> linearChannelMediaTypes)
        {
            var anyRegionIds = !isAllowedToViewInactiveAssets &&
                               regionIds != null &&
                               regionIds.Any();

            if (!anyRegionIds)
                return null;

            var regionCompositeContainer = new QueryContainerDescriptor<NestMedia>();

            regionCompositeContainer.Bool(b =>
                b.Should(
                    s => s.Terms(t => t.Field(f => f.Regions).Terms(regionIds)),
                    s => s.Bool(b2 =>
                        {
                            b2 = b2.Must(m => m.Term(t => t.Field(g => g.Regions).Value(0)));

                            if (linearChannelMediaTypes.Any())
                            {
                                b2 = b2.MustNot(m => m.Terms(t =>
                                    t.Field(f => f.MediaTypeId)
                                        .Terms(linearChannelMediaTypes)));
                            }

                            return b2;
                        }
                    )
                )
            );

            return regionCompositeContainer;
        }

        public QueryContainer GetMediaDateRangesTermsWithCountries(UnifiedSearchDefinitions searchDefinitions)
        {
            var mediaDateRanges = GetMediaDateRangesTerms(searchDefinitions);
            var EmptyCountryId = 0;

            if (searchDefinitions.countryId > EmptyCountryId)
            {
                // allowed_countries = 0 and dates filter
                var allowedEmptyAndDateRanges = new QueryContainerDescriptor<NestMedia>();
                var allowedEmptyCountries = new QueryContainerDescriptor<NestMedia>();
                allowedEmptyCountries.Term(t =>
                    t.Field(f => f.AllowedCountries)
                        .Value(EmptyCountryId)
                );
                
                allowedEmptyAndDateRanges.Bool(b =>
                    b.Must(
                        m => allowedEmptyCountries,
                        m => mediaDateRanges)
                );

                // allow empty with date range or not empty
                var allowedEmptyAndDateRangesOrNotEmpty = new QueryContainerDescriptor<NestMedia>();
                var allowedCountries = new QueryContainerDescriptor<NestMedia>();
                allowedCountries.Term(t =>
                    t.Field(f => f.AllowedCountries).Value(searchDefinitions.countryId));

                allowedEmptyAndDateRangesOrNotEmpty.Bool(b => b.Should(
                    s => allowedCountries,
                    s => allowedEmptyAndDateRanges));

                //not blocked and allowedEmptyAndDateRangesOrNotEmpty

                var notBlockedAndAllowedEmptyAndDateRangesOrNotEmpty = new QueryContainerDescriptor<NestMedia>();
              
                var blockedCountries = new QueryContainerDescriptor<NestMedia>();
                blockedCountries.Term(t =>
                    t.Field(f => f.BlockedCountries).Value(searchDefinitions.countryId));

                notBlockedAndAllowedEmptyAndDateRangesOrNotEmpty.Bool(b =>
                    b.Must(
                        m => allowedEmptyAndDateRangesOrNotEmpty,
                        m => m.Bool(b2 => b2.MustNot(blockedCountries))
                    )
                );
              
                return notBlockedAndAllowedEmptyAndDateRangesOrNotEmpty;
            }
            
            
            // No geo availability - handle dates without country consideration
            if (mediaDateRanges!=null)  
                return mediaDateRanges;
            
            return null;
        }

        public QueryContainer GetMediaDateRangesTerms(UnifiedSearchDefinitions searchDefinitions)
        {
            var shouldUseStartDateForMedia = searchDefinitions.shouldUseStartDateForMedia;
            var shouldUseCatalogStartDateForMedia = searchDefinitions.shouldUseCatalogStartDateForMedia;
            var shouldIgnoreEndDate = searchDefinitions.shouldIgnoreEndDate;
            var shouldUseFinalEndDate = searchDefinitions.shouldUseFinalEndDate;

            return GetMediaDateRangesTerms(shouldUseStartDateForMedia, shouldUseCatalogStartDateForMedia,
                shouldIgnoreEndDate, shouldUseFinalEndDate);
        }
        
        public QueryContainer GetMediaDateRangesTerms(MediaSearchObj searchDefinitions)
        {
            var shouldUseStartDateForMedia = searchDefinitions.m_bUseStartDate;
            var shouldUseCatalogStartDateForMedia = false;
            var shouldIgnoreEndDate = false;
            var shouldUseFinalEndDate = searchDefinitions.m_bUseFinalEndDate;

            return GetMediaDateRangesTerms(shouldUseStartDateForMedia, shouldUseCatalogStartDateForMedia,
                shouldIgnoreEndDate, shouldUseFinalEndDate);
        }
        
        private QueryContainer GetMediaDateRangesTerms(bool shouldUseStartDateForMedia,
            bool shouldUseCatalogStartDateForMedia, bool shouldIgnoreEndDate, bool shouldUseFinalEndDate)
        {
            var mustContainer = new List<QueryContainer>();
            var now = SystemDateTime.UtcNow;
            now = now.AddSeconds(-now.Second);

            if (shouldUseStartDateForMedia)
            {
                var dateRangeQueryDescriptor = new DateRangeQueryDescriptor<NestMedia>();

                if (shouldUseCatalogStartDateForMedia)
                {
                    dateRangeQueryDescriptor.Field(f => f.CatalogStartDate);
                }
                else
                {
                    dateRangeQueryDescriptor.Field(f => f.StartDate);
                }

                dateRangeQueryDescriptor
                    .GreaterThanOrEquals(DateTime.MinValue)
                    .LessThanOrEquals(now);

                var containerDescriptor = new QueryContainerDescriptor<NestMedia>();
                containerDescriptor.DateRange(x => dateRangeQueryDescriptor);
                mustContainer.Add(containerDescriptor);
            }

            if (!shouldIgnoreEndDate)
            {
                var dateRangeQueryDescriptor = new DateRangeQueryDescriptor<NestMedia>();

                if (shouldUseFinalEndDate)
                {
                    dateRangeQueryDescriptor.Field(f => f.FinalEndDate);
                }
                else
                {
                    dateRangeQueryDescriptor.Field(f => f.EndDate);
                }

                dateRangeQueryDescriptor
                    .GreaterThanOrEquals(now)
                    .LessThanOrEquals(DateTime.MaxValue);

                mustContainer.Add(new QueryContainerDescriptor<NestMedia>().DateRange(x => dateRangeQueryDescriptor));
            }

            if (!mustContainer.Any())
                return null;

            var queryContainerDescriptor = new QueryContainerDescriptor<NestMedia>();
             queryContainerDescriptor.Bool(x => x.Must(mustContainer.ToArray()));
            return queryContainerDescriptor;
        }

        public QueryContainer GetMediaDeviceRulesTerms(UnifiedSearchDefinitions searchDefinitions)
        {
            var shouldIgnoreDeviceRuleId = searchDefinitions.shouldIgnoreDeviceRuleID;
            var deviceRuleId = searchDefinitions.deviceRuleId;
            return GetMediaDeviceRulesTerms(shouldIgnoreDeviceRuleId, deviceRuleId);
        }
        
        public QueryContainer GetMediaDeviceRulesTerms(MediaSearchObj searchDefinitions)
        {
            var shouldIgnoreDeviceRuleId = searchDefinitions.m_bIgnoreDeviceRuleId;
            var deviceRuleId = searchDefinitions.m_nDeviceRuleId;
            return GetMediaDeviceRulesTerms(shouldIgnoreDeviceRuleId, deviceRuleId);
            
        }
        
        private QueryContainer GetMediaDeviceRulesTerms(bool shouldIgnoreDeviceRuleId, int[] deviceRuleId)
        {
            if (shouldIgnoreDeviceRuleId)
                return null;

            var deviceRuleIds = new HashSet<int>() { 0 };

            if (deviceRuleId != null)
            {
                deviceRuleIds.UnionWith(deviceRuleId);
            }

            var queryContainerDescriptor = new QueryContainerDescriptor<NestMedia>();
             queryContainerDescriptor
                .Terms(t => t
                    .Field(f => f.DeviceRuleId).Terms(deviceRuleIds)
                );
            return queryContainerDescriptor;
        }

        public QueryContainer GetMediaWatchPermissionRules(UnifiedSearchDefinitions searchDefinitions)
        {
            var groupId = searchDefinitions.groupId;
            var permittedWatchRules = searchDefinitions.permittedWatchRules;
            return GetMediaWatchPermissionRules(permittedWatchRules, groupId);
        }

        public QueryContainer GetMediaWatchPermissionRules(MediaSearchObj searchDefinitions)
        {
            var groupId = searchDefinitions.m_nGroupId;
            var permittedWatchRules = searchDefinitions.m_sPermittedWatchRules;
            return GetMediaWatchPermissionRules(permittedWatchRules, groupId);
        }
        
        private QueryContainer GetMediaWatchPermissionRules(string permittedWatchRules, int groupId)
        {
            if (string.IsNullOrEmpty(permittedWatchRules))
            {
                return null;
            }

            var termsQueryDescriptor = new TermsQueryDescriptor<NestMedia>();
            // group_id = parent groupd id OR media has permitted watch filter 
            var watchRules = permittedWatchRules.Split(' ').Where(x => x != null)
                .Select(int.Parse);

            termsQueryDescriptor.Field(f => f.WPTypeID).Terms(watchRules);
            var groupTerm = new TermQueryDescriptor<NestMedia>()
                .Field(f => f.GroupID)
                .Value(groupId);

            var queryContainerDescriptor = new QueryContainerDescriptor<NestMedia>();
            queryContainerDescriptor
                .Bool(b => b.Should(
                    s => s.Term(t => groupTerm),
                    s => s.Terms(t => termsQueryDescriptor)
                ));
            return queryContainerDescriptor;
        }

        public QueryContainer GetMediaTypeTerms(UnifiedSearchDefinitions searchDefinitions)
        {
            var mediaTypes = searchDefinitions.mediaTypes;
            return GetMediaTypeTerms(mediaTypes);
        }
        
        public QueryContainer GetMediaTypeTerms(MediaSearchObj searchDefinitions)
        {
            if (string.IsNullOrEmpty(searchDefinitions.m_sMediaTypes))
                return null;
            
            var mediaTypes = searchDefinitions.m_sMediaTypes.Split(';')
                .Select(x => x.Trim())
                .Select(int.Parse)
                .Where(x => x != 0).ToList();
            
            return GetMediaTypeTerms(mediaTypes);
        }

        private QueryContainer GetMediaTypeTerms(List<int> mediaTypes)
        {
            if (!mediaTypes.Any())
                return null;

            var queryContainerDescriptor = new QueryContainerDescriptor<NestMedia>();
            queryContainerDescriptor.Terms(t => t.Field(f => f.MediaTypeId).Terms(mediaTypes));
            return queryContainerDescriptor;
        }

        public QueryContainer GetMediaUserTypeTerms(UnifiedSearchDefinitions searchDefinitions)
        {
            var userTypeId = searchDefinitions.userTypeID;
            return GetMediaUserTypeTerms(userTypeId);
        }
        
        public QueryContainer GetMediaUserTypeTerms(MediaSearchObj searchDefinitions)
        {
            var userTypeId = searchDefinitions.m_nUserTypeID;
            return GetMediaUserTypeTerms(userTypeId);
        }

        private QueryContainer GetMediaUserTypeTerms(int userTypeId)
        {
            if (userTypeId < 0)
                return null;

            var queryContainerDescriptor = new QueryContainerDescriptor<NestMedia>();
            queryContainerDescriptor.Terms(t => t.Field(f => f.UserTypes).Terms(new HashSet<int>() { 0, userTypeId }));
            return queryContainerDescriptor;
        }

        public QueryContainer GetMediaPrefixQuery()
        {
            return new QueryContainerDescriptor<NestMedia>().Exists(x => x.Field(f => f.MediaId));
        }

        public QueryContainer GetMediaIdTerm(MediaSearchObj definitions)
        {
            if (definitions.m_nMediaID <= 0)
                return null;

            var queryContainerDescriptor = new QueryContainerDescriptor<NestMedia>();
            queryContainerDescriptor.Term(x =>
                    x.Field(f => f.MediaId).Value(definitions.m_nMediaID));

            return queryContainerDescriptor;
        }


        public QueryContainer GetSearchValueOrMultiMatch(MediaSearchObj definitions)
        {
            if (definitions.m_dOr == null || !definitions.m_dOr.Any())
            {
                return null;
            }

            var searchValues = definitions.m_dOr.Select(searchValue =>
                ElasticSearch.Common.Utils.GetKeyNameWithPrefix(searchValue.m_sKey.ToLower(),
                    searchValue.m_sKeyPrefix.ToLower())).ToArray();

            var queryContainerDescriptor = new QueryContainerDescriptor<NestMedia>();
             queryContainerDescriptor.MultiMatch(mm => mm.Fields(f => f.Fields(searchValues)));
             return queryContainerDescriptor;
        }
    }
}