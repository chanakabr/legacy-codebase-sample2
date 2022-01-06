using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects.Statistics;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Phx.Lib.Log;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class BuzzMeterLoader : CatalogRequestManager, ILoaderAdapter
    {
        #region Members

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string Id { get; set; } // Indicates a series id or linear channel id

        #endregion

        #region CTOR

        public BuzzMeterLoader(int groupId, string key)
        {
            this.Id = key;
            this.GroupID = groupId;
        }

        #endregion

        #region CatalogRequestManager

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new BuzzMeterRequest()
            {
                m_nGroupID = GroupID,
                m_sKey = Id
            };
        }

        protected override void Log(string message, object obj)
        {
            if (!string.IsNullOrEmpty(message) && obj != null)
            {
                StringBuilder log = new StringBuilder();
                log.AppendLine(message);
                if (obj is BuzzMeterRequest)
                {
                    BuzzMeterRequest buzzMeterRequest = obj as BuzzMeterRequest;
                    log.AppendFormat("BundleMediaRequest: key = {0}, GroupID = {1}", buzzMeterRequest.m_sKey, buzzMeterRequest.m_nGroupID);
                }
                else if (obj is BuzzMeterResponse)
                {
                    BuzzMeterResponse buzzMeterResponse = obj as BuzzMeterResponse;
                    string msgLog = string.Empty;
                    if (buzzMeterResponse.m_buzzAverScore != null)
                    {
                        msgLog = string.Format("BuzzMeterResponse: UpdateDate = {0}, WeightedAverageScore = {1}, NormalizedWeightedAverageScore = {2}", buzzMeterResponse.m_buzzAverScore.UpdateDate, buzzMeterResponse.m_buzzAverScore.WeightedAverageScore, buzzMeterResponse.m_buzzAverScore.NormalizedWeightedAverageScore);
                    }
                    else
                    {
                        msgLog = "BuzzMeterResponse: No data returned";
                    }

                    log.Append(msgLog);
                }

                if (logger != null)
                {
                    logger.Info(log.ToString());
                }
            }
        }

        #endregion

        public object Execute()
        {
            BuzzWeightedAverScore response = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                response = (m_oResponse as BuzzMeterResponse).m_buzzAverScore;
            }
            return response;
        }

        #region ILoaderAdapter

        public bool IsPersist()
        {
            throw new NotImplementedException();
        }

        public object Execute(eExecuteBehaivor behaivor)
        {
            throw new NotImplementedException();
        }

        public object LastExecuteResult
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
