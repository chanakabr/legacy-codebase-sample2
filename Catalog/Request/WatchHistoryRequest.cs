using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using ApiObjects.MediaMarks;
using Tvinci.Core.DAL;
using System.Data;
using Catalog.Response;
using KLogMonitor;
using System.Reflection;

namespace Catalog.Request
{
    [DataContract]
    public class WatchHistoryRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public OrderDir OrderDir { get; set; }

        [DataMember]
        public eWatchStatus FilterStatus { get; set; }

        [DataMember]
        public List<int> AssetTypes { get; set; }

        [DataMember]
        public List<string> AssetIds { get; set; }

        [DataMember]
        public int NumOfDays { get; set; }        

        public BaseResponse GetResponse(BaseRequest baseRequest)
        {
            WatchHistoryResponse response = new WatchHistoryResponse();

            try
            {
                WatchHistoryRequest request = baseRequest as WatchHistoryRequest;

                // request validation
                if (request == null)
                    throw new ArgumentNullException("request object is null or Required variables is null");

                // group validation
                if (request.m_nGroupID == 0)
                {
                    var exception = new ArgumentException("No group Id was sent in request");
                    exception.Data["StatusCode"] = (int)eResponseStatus.BadSearchRequest;
                    throw exception;
                }

                // check signature
                CheckSignature(baseRequest);

                // take finished percent threshold 
                int finishedPercentThreshold = 0;
                object dbThresholdVal = ODBCWrapper.Utils.GetTableSingleVal("groups", "FINISHED_PERCENT_THRESHOLD", m_nGroupID, 3600);
                if (dbThresholdVal == null ||
                    dbThresholdVal == DBNull.Value ||
                    !int.TryParse(dbThresholdVal.ToString(), out finishedPercentThreshold))
                {
                    finishedPercentThreshold = Catalog.FINISHED_PERCENT_THRESHOLD;
                }

                // get results
                int totalItems = 0;

                List<int> excludedAssetTypes = new List<int>() { (int)eAssetTypes.EPG };

                // If asset types contains NPVR explicitly, don't exclude it.
                if (AssetTypes == null || !AssetTypes.Contains((int)eAssetTypes.NPVR))
                {
                    excludedAssetTypes.Add((int)eAssetTypes.NPVR);
                }

                List<WatchHistory> res = Catalog.GetUserWatchHistory(
                    m_nGroupID, m_sSiteGuid, AssetTypes, AssetIds, excludedAssetTypes, FilterStatus, NumOfDays,
                    OrderDir, m_nPageIndex, m_nPageSize, finishedPercentThreshold, out totalItems);

                // convert to client response
                UserWatchHistory userWatchHistory;
                foreach (var item in res)
                {
                    userWatchHistory = new Response.UserWatchHistory()
                    {
                        Duration = item.Duration,
                        IsFinishedWatching = item.IsFinishedWatching,
                        LastWatch = item.LastWatch,
                        Location = item.Location,
                        AssetId = item.AssetId,
                        UserID = item.UserID,
                        AssetTypeId = item.AssetTypeId,
                        m_dUpdateDate = item.UpdateDate,
                        EpgId = item.EpgId
                    };

                    switch (item.AssetTypeId)
                    {
                        case (int)eAssetTypes.EPG:
                        case (int)eAssetTypes.NPVR:
                            userWatchHistory.AssetType = (eAssetTypes)item.AssetTypeId;
                            break;
                        default:
                            userWatchHistory.AssetType = eAssetTypes.MEDIA;
                            break;
                    }
                    response.result.Add(userWatchHistory);
                }

                response.m_nTotalItems = totalItems;
                response.status.Code = (int)eResponseStatus.OK;
                response.status.Message = eResponseStatus.OK.ToString();
            }
            catch (Exception ex)
            {
                log.Error("Error - WatchHistoryRequest - " +
                    string.Format("Exception: group = {0}, siteGuid = {1}, message = {2}, ST = {3}",
                    baseRequest.m_nGroupID,     // {0}
                    baseRequest.m_sSiteGuid,    // {1}
                    ex.Message,                 // {2}
                    ex.StackTrace               // {3}
                    ), ex);


                response.status.Code = (int)eResponseStatus.Error;
                response.status.Message = "Error retrieving data";
            }

            return (BaseResponse)response;
        }
    }
}