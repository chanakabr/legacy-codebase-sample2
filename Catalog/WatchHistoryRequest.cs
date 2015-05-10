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

namespace Catalog
{
    [DataContract]
    public class WatchHistoryRequest : BaseRequest, IRequestImp
    {
        [DataMember]
        public OrderDir OrderDir { get; set; }

        [DataMember]
        public eWatchStatus FilterStatus { get; set; }

        [DataMember]
        public List<int> AssetTypes { get; set; }

        [DataMember]
        public int NumOfDays { get; set; }

        [DataMember]
        private const int FINISHED_PERCENT_THRESHOLD = 95;

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

                // get results
                int totalItems = 0;
                response.result = CatalogDAL.GetUserWatchHistory(m_sSiteGuid, AssetTypes, new List<int>() { (int)eAssetFilterTypes.NPVR }, FilterStatus, NumOfDays, OrderDir, m_nPageIndex, m_nPageSize, FINISHED_PERCENT_THRESHOLD, out totalItems);
                response.m_nTotalItems = totalItems;
                response.status.Code = (int)eResponseStatus.OK;
                response.status.Message = eResponseStatus.OK.ToString();

                // get last updated date of media (exclude NPVR)
                if (response != null && response.result != null && response.result.Count() > 0)
                {
                    List<int> mediaIds = response.result.Where(x => x.AssetTypeId != (int)eAssetFilterTypes.NPVR).Select(item => int.Parse(item.AssetId)).ToList();
                    DataTable dt = CatalogDAL.Get_MediaUpdateDate(mediaIds);
                    if (dt != null)
                    {
                        if (dt.Columns != null)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                // get relevant watch history item and validate the updated asset is not NPVR (in case they have the same ID as the media asset)
                                var historyWatchItem = response.result.Where(x => x.AssetTypeId != (int)eAssetFilterTypes.NPVR &&
                                                                                  int.Parse(x.AssetId) == Utils.GetIntSafeVal(dt.Rows[i], "ID")).FirstOrDefault();
                                // update date 
                                if (historyWatchItem != null && dt.Rows[i]["UPDATE_DATE"] != null)
                                    historyWatchItem.AssetUpdatedDate = System.Convert.ToDateTime(dt.Rows[i]["UPDATE_DATE"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error - WatchHistoryRequest",
                    string.Format("Exception: group = {0}, siteGuid = {1}, message = {2}, ST = {3}",
                    baseRequest.m_nGroupID,     // {0}
                    baseRequest.m_sSiteGuid,    // {1}
                    ex.Message,                 // {2}
                    ex.StackTrace               // {3}
                    ), this.GetType().Name);


                response.status.Code = (int)eResponseStatus.Error;
                response.status.Message = "Error retrieving data";
            }

            return (BaseResponse)response;
        }
    }
}