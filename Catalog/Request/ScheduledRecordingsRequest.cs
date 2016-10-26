using ApiObjects.Response;
using Catalog.Response;
using Catalog.ws_cas;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Request
{
    [DataContract]
    public class ScheduledRecordingsRequest : BaseRequest, IRequestImp
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public List<long> channelIds;

        [DataMember]
        public ApiObjects.ScheduledRecordingAssetType scheduledRecordingAssetType;

        [DataMember]
        public ApiObjects.SearchObjects.OrderObj orderBy;

        [DataMember]
        public DateTime? startDate;

        [DataMember]
        public DateTime? endDate;

        public ScheduledRecordingsRequest()
            : base()
        {
        }


        public BaseResponse GetResponse(BaseRequest baseRequest)
        {
            UnifiedSearchResponse response = new UnifiedSearchResponse() { status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };

            try
            {
                ScheduledRecordingsRequest request = baseRequest as ScheduledRecordingsRequest;

                if (request == null)
                {
                    throw new ArgumentNullException("request object is null or required variable is null");
                }

                if (request.m_nGroupID == 0)
                {
                    response.status.Code = (int)eResponseStatus.BadSearchRequest;
                    response.status.Message = "No group Id was sent in request";

                    return response;
                }

                CheckSignature(baseRequest);

                bool isSingle = scheduledRecordingAssetType == ApiObjects.ScheduledRecordingAssetType.SINGLE;                
                SeriesRecording[] series = null;
                string wsUserName, wsPassword;
                GetWsCredentials(out wsUserName, out wsPassword);
                // should we get the followed series
                if (!isSingle)
                {
                    using (ws_cas.module cas = new ws_cas.module())
                    {
                        string url = Utils.GetWSURL("ws_cas");
                        if (!string.IsNullOrEmpty(url))
                        {
                            cas.Url = url;
                        }
                        else
                        {
                            throw new Exception("CAS url is null or empty");
                        }

                        SeriesResponse seriesResponse = cas.GetFollowSeries(wsUserName, wsPassword, m_sSiteGuid, domainId, new SeriesRecordingOrderObj());
                        if (seriesResponse != null && seriesResponse.Status != null && (seriesResponse.Status.Code == (int)eResponseStatus.OK || seriesResponse.Status.Code == (int)eResponseStatus.SeriesRecordingNotFound))
                        {
                            if (channelIds != null && channelIds.Count > 0)
                            {
                                seriesResponse.SeriesRecordings = seriesResponse.SeriesRecordings.Where(x => channelIds.Contains(x.EpgChannelId)).ToArray();
                            }

                            series = seriesResponse.SeriesRecordings.ToArray();
                        }
                        else
                        {
                            throw new Exception(string.Format("Failed cas.GetFollowSeries for groupId: {0}, userId: {1}, domainId: {2}", m_nGroupID, m_sSiteGuid, domainId));
                        }
                    }
                }

                RecordingResponse domainRecordings = GetCurrentRecordings(wsUserName, wsPassword);
                if (domainRecordings != null && domainRecordings.Status != null && domainRecordings.Status.Code == (int)eResponseStatus.OK)
                {
                    List<long> epgIdsToOrderAndPage = new List<long>();
                    List<string> excludedCrids = new List<string>();
                    if (domainRecordings.TotalItems > 0)
                    {
                        if (channelIds != null && channelIds.Count > 0)
                        {
                            domainRecordings.Recordings = domainRecordings.Recordings.Where(x => channelIds.Contains(x.ChannelId)).Select(x => x).ToArray();
                        }
                        
                        // get crids to exclude
                        if (scheduledRecordingAssetType != ApiObjects.ScheduledRecordingAssetType.SINGLE)
                        {
                            List<RecordingType> recordingTypesToExcludeCrids = new List<RecordingType>() { RecordingType.Season, RecordingType.Series };
                            excludedCrids = domainRecordings.Recordings.Where(x => recordingTypesToExcludeCrids.Contains(x.Type)).Select(x => x.Crid).ToList();
                        }
                        
                        // get SINGLE scheduled recordings assets
                        if (scheduledRecordingAssetType != ApiObjects.ScheduledRecordingAssetType.SERIES)
                        {
                            domainRecordings.Recordings = domainRecordings.Recordings.Where(x => x.Type == RecordingType.Single && x.RecordingStatus == TstvRecordingStatus.Scheduled).Select(x => x).ToArray();
                            if (startDate.HasValue)
                            {
                                domainRecordings.Recordings = domainRecordings.Recordings.Where(x => x.EpgStartDate > startDate.Value).Select(x => x).ToArray();
                            }

                            if (endDate.HasValue)
                            {
                                domainRecordings.Recordings = domainRecordings.Recordings.Where(x => x.EpgEndDate < endDate.Value).Select(x => x).ToArray();
                            }

                            epgIdsToOrderAndPage = domainRecordings.Recordings.Select(x => x.EpgId).ToList();                            
                        }
                    }

                    response = SearchScheduledRecordings(m_nGroupID, epgIdsToOrderAndPage, excludedCrids, series, startDate, endDate);
                }
                else
                {
                    throw new Exception(string.Format("Failed GetCurrentRecordings for groupId: {0}, userId: {1}, domainId: {2}", m_nGroupID, m_sSiteGuid, domainId));
                }
            }

            catch (Exception ex)
            {
                log.Error("Failed ScheduledRecordingsRequest", ex);
                throw ex;
            }

            return response;
        }

        private RecordingResponse GetCurrentRecordings(string wsUserName, string wsPassword)
        {
            RecordingResponse result = new RecordingResponse();
            try
            {
                List<TstvRecordingStatus> statusesToSearch = new List<TstvRecordingStatus>();
                // should we get the scheduled single recordings
                if (scheduledRecordingAssetType != ApiObjects.ScheduledRecordingAssetType.SERIES)
                {
                    statusesToSearch.Add(TstvRecordingStatus.Scheduled);
                }

                // should we get the existing series recordings
                if (scheduledRecordingAssetType != ApiObjects.ScheduledRecordingAssetType.SINGLE)
                {
                    statusesToSearch.AddRange(new List<TstvRecordingStatus>() { TstvRecordingStatus.SeriesDelete, TstvRecordingStatus.SeriesCancel, TstvRecordingStatus.Failed,
                                                                                    TstvRecordingStatus.Recorded, TstvRecordingStatus.Recording });
                }

                using (ws_cas.module cas = new ws_cas.module())
                {
                    string url = Utils.GetWSURL("ws_cas");
                    if (!string.IsNullOrEmpty(url))
                    {
                        cas.Url = url;
                    }
                    else
                    {
                        throw new Exception("CAS url is null or empty");
                    }

                    result = cas.SearchDomainRecordings(wsUserName, wsPassword, m_sSiteGuid, domainId, statusesToSearch.ToArray(), string.Empty, 0, 0,
                                                        new OrderObj() { m_eOrderBy = OrderBy.ID, m_eOrderDir = OrderDir.ASC }, true);
                }
            }

            catch (Exception ex)
            {
                log.Error("Failed GetCurrentRecordings", ex);
            }

            return result;
        }

        private void GetWsCredentials(out string userName, out string password)
        {
            userName = string.Empty;
            password = string.Empty;

            //get username + password from wsCache
            ApiObjects.Credentials credentials = TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG, m_nGroupID, ApiObjects.eWSModules.CONDITIONALACCESS);

            if (credentials != null)
            {
                userName = credentials.m_sUsername;
                password = credentials.m_sPassword;
            }

            // validate user name and password length
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                throw new Exception(string.Format("No WS_CAS login parameters were extracted from DB. groupId={0}", m_nGroupID));
            }
        }

        private UnifiedSearchResponse SearchScheduledRecordings(int groupID, List<long> epgIdsToOrderAndPage, List<string> cridsToExclude, SeriesRecording[] series, DateTime? startDate, DateTime? endDate)
        {
            UnifiedSearchResponse response = null;

            // build the KSQL for the series
            string seriesId = "series_id";
            string seasonNumber = "season_number";

            // build the filter query for the search
            StringBuilder ksql = new StringBuilder("(and (or ");
            StringBuilder seasonsToExclude = null;
            string season = null;
            if (series != null)
            {
                foreach (SeriesRecording serie in series)
                {
                    season = (serie.SeasonNumber > 0 && !string.IsNullOrEmpty(seasonNumber)) ? string.Format("{0} = '{1}' ", seasonNumber, serie.SeasonNumber) : string.Empty;
                    seasonsToExclude = new StringBuilder();
                    if (serie.ExcludedSeasons != null && serie.ExcludedSeasons.Length > 0)
                    {
                        foreach (int seasonNumberToExclude in serie.ExcludedSeasons)
                        {
                            seasonsToExclude.AppendFormat("{0} != '{1}' ", seasonNumber, seasonNumberToExclude);
                        }
                    }

                    ksql.AppendFormat("(and {0} = '{1}' epg_channel_id = '{2}' {3} {4})", seriesId, serie.SeriesId, serie.EpgChannelId, season, seasonsToExclude.ToString());
                }
            }

            if (epgIdsToOrderAndPage != null && epgIdsToOrderAndPage.Count > 0)
            {
                ksql.AppendFormat("epg_id: {0}", string.Join(",", epgIdsToOrderAndPage));
            }

            if (startDate.HasValue)
            {
                ksql.AppendFormat(") start_date > '{0}'", TVinciShared.DateUtils.DateTimeToUnixTimestamp(startDate.Value));
            }
            else
            {
                ksql.AppendFormat(") start_date > '0'");
            }

            if (endDate.HasValue)
            {
                ksql.AppendFormat(" end_date < '{0}'", TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate.Value));
            }

            ksql.Append(")");

            // get program ids
            try
            {
                UnifiedSearchRequest searchRequest = new UnifiedSearchRequest(m_nPageSize, m_nPageIndex, m_nGroupID, m_sSignature, m_sSignString, orderBy,
                                                                        new List<int>() { 0 }, ksql.ToString(), string.Empty) { excludedCrids = cridsToExclude };

                BaseResponse baseResponse = searchRequest.GetResponse(searchRequest);
                response = baseResponse as UnifiedSearchResponse;
            }

            catch (Exception ex)
            {
                log.Error("SearchScheduledRecordings Failed", ex);
            }

            return response;
        }

    }
}
