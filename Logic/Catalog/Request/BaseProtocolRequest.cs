using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using TVinciShared;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    [DataContract]
    public abstract class BaseProtocolRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected string sEndDate;
        protected int nDeviceID;
        protected int nLanguageID;

        #region Abstract methods

        protected abstract List<SearchResult> ExecuteNonIPNOProtocol(BaseRequest oBaseRequest);

        protected abstract List<SearchResult> ExecuteIPNOProtocol(BaseRequest oBaseRequest, int nOperatorID, List<List<string>> jsonizedChannelsDefinitions, ref ISearcher initializedSearcher);

        protected abstract int GetProtocolMaxResultsSize();
        #endregion

        public BaseProtocolRequest(Int32 nPageSize, Int32 nPageIndex, string sUserIP, Int32 nGroupID,
            Filter oFilter, string sSignature, string sSignString, string sSiteGuid, int nDomainId)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString, sSiteGuid, nDomainId)
        {
        }

        public BaseProtocolRequest() : base() { }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            CheckSignature(oBaseRequest);
            return ExecuteProtocol(oBaseRequest);
        }

        protected virtual BaseResponse ExecuteProtocol(BaseRequest oRequest)
        {
            List<SearchResult> res = null;
            ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();
            List<List<string>> jsonizedChannelsDefinitions = null;
            int nOperatorID = 0;
            MediaIdsResponse response = new MediaIdsResponse();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (CatalogLogic.IsUseIPNOFiltering(oRequest, ref searcher, ref jsonizedChannelsDefinitions, ref nOperatorID))
            {
                res = ExecuteIPNOProtocol(oRequest, nOperatorID, jsonizedChannelsDefinitions, ref searcher);
            }
            else
            {
                res = ExecuteNonIPNOProtocol(oRequest);
            }
            sw.Stop();

            log.Info(string.Format("Protocol execution finished. Request: {0} , Time elapsed: {1}", oRequest.ToString(), sw.ElapsedMilliseconds));

            response.m_nMediaIds = res;
            response.m_nTotalItems = res.Count;

            return response;
        }

        protected List<SearchResult> GetProtocolFinalResultsUsingSearcher(List<SearchResult> initialResults, ref ISearcher initializedSearcher, List<List<string>> jsonizedChannelsDefinitions, int nGroupID)
        {
            if (initialResults.Count > 0)
            {
                List<long> mediaIDs = initialResults.Select<SearchResult, long>(item => item.assetID).ToList();
                Dictionary<long, bool> resDict = initializedSearcher.ValidateMediaIDsInChannels(nGroupID, mediaIDs, jsonizedChannelsDefinitions[0], jsonizedChannelsDefinitions[1]);
                if (resDict != null && resDict.Count > 0)
                {

                    int maxResults = GetProtocolMaxResultsSize();
                    int counter = 0;
                    List<SearchResult> finalResults = new List<SearchResult>(maxResults);
                    for (int i = 0; i < initialResults.Count && counter < maxResults; i++)
                    {
                        if (resDict[initialResults[i].assetID])
                        {
                            finalResults.Add(initialResults[i]);
                            counter++;
                        }
                    }

                    return finalResults;
                }
            }

            return new List<SearchResult>(0);
        }

        protected List<SearchResult> ConvertProtocolDataTableToList(DataTable protocolDT, string sIDColName)
        {
            List<SearchResult> res = null;
            if (protocolDT != null && protocolDT.Columns != null && protocolDT.Rows != null && protocolDT.Rows.Count > 0)
            {
                int length = protocolDT.Rows.Count;
                res = new List<SearchResult>(length);
                for (int i = 0; i < length; i++)
                {
                    SearchResult oMediaRes = new SearchResult();
                    oMediaRes.assetID = Utils.GetIntSafeVal(protocolDT.Rows[i], sIDColName);
                    if (protocolDT.Rows[i]["UPDATE_DATE"] != DBNull.Value && protocolDT.Rows[i]["UPDATE_DATE"] != null &&
                        !string.IsNullOrEmpty(protocolDT.Rows[i]["UPDATE_DATE"].ToString()))
                    {
                        oMediaRes.UpdateDate = System.Convert.ToDateTime(protocolDT.Rows[i]["UPDATE_DATE"].ToString());
                    }
                    res.Add(oMediaRes);
                }
            }
            else
            {
                res = new List<SearchResult>(0);
            }

            return res;
        }

        protected void GetEndDateLanguageDeviceID(BaseRequest oBaseRequest)
        {
            if (oBaseRequest.m_oFilter != null)
            {
                sEndDate = ProtocolsFuncs.GetFinalEndDateField(oBaseRequest.m_oFilter.m_bUseFinalDate);
                if (!string.IsNullOrEmpty(oBaseRequest.m_oFilter.m_sDeviceId))
                    nDeviceID = Int32.Parse(oBaseRequest.m_oFilter.m_sDeviceId);
                nLanguageID = oBaseRequest.m_oFilter.m_nLanguage;
            }
            else
            {
                sEndDate = string.Empty;
                nDeviceID = 0;
                nLanguageID = 0;
            }
        }

    }
}
