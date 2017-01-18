using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Tvinci.Core.DAL;
using Core.Catalog.Response;
using KLogMonitor;
using ApiObjects.Catalog;

namespace Core.Catalog.Request
{
    [DataContract]
    public class BundlesContainingMediaRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public BundleKeyValue[] m_oBundles;
        [DataMember]
        public int m_nMediaID;

        public BundlesContainingMediaRequest()
            : base()
        {

        }

        protected override void CheckRequestValidness()
        {
            if (m_oBundles == null || m_oBundles.Length == 0 || m_nMediaID == 0)
                throw new Exception("Request is invalid");
        }

        private void GetBundlesList(ref List<int> subs, ref List<int> cols)
        {
            subs = new List<int>();
            cols = new List<int>();
            for (int i = 0; i < m_oBundles.Length; i++)
            {
                if (m_oBundles[i].m_eBundleType == eBundleType.SUBSCRIPTION)
                {
                    subs.Add(m_oBundles[i].m_nBundleCode);
                }
                else
                {
                    // collection
                    cols.Add(m_oBundles[i].m_nBundleCode);
                }

            }
        }

        private void FillResponse(List<int> channelsOfMedia, Dictionary<int, List<int>> channelsToSubsMapping,
           Dictionary<int, List<int>> channelsToColsMapping, ref BundlesContainingMediaResponse initializedResponse)
        {
            if (channelsOfMedia != null && channelsOfMedia.Count > 0)
            {
                for (int i = 0; i < channelsOfMedia.Count; i++)
                {
                    // set IsContained: true in the relevant subscriptions in the response
                    if (channelsToSubsMapping.ContainsKey(channelsOfMedia[i]))
                    {
                        List<int> subsContainingChannel = channelsToSubsMapping[channelsOfMedia[i]];

                        if (subsContainingChannel != null && subsContainingChannel.Count > 0)
                        {
                            for (int j = 0; j < subsContainingChannel.Count; j++)
                            {
                                if (initializedResponse.m_oSubsToIndexMapping.ContainsKey(subsContainingChannel[j]))
                                {
                                    int indexOfSub = initializedResponse.m_oSubsToIndexMapping[subsContainingChannel[j]];
                                    initializedResponse.m_oBundles[indexOfSub].m_bIsContained = true;
                                    initializedResponse.m_nTotalItems++;
                                }
                            }
                        }
                    }

                    // set IsContained: true in the relevant collections in the response
                    if (channelsToColsMapping.ContainsKey(channelsOfMedia[i]))
                    {
                        List<int> colsContainingChannel = channelsToColsMapping[channelsOfMedia[i]];

                        if (colsContainingChannel != null && colsContainingChannel.Count > 0)
                        {
                            for (int j = 0; j < colsContainingChannel.Count; j++)
                            {
                                if (initializedResponse.m_oColsToIndexMapping.ContainsKey(colsContainingChannel[j]))
                                {
                                    int indexOfCol = initializedResponse.m_oColsToIndexMapping[colsContainingChannel[j]];
                                    initializedResponse.m_oBundles[indexOfCol].m_bIsContained = true;
                                    initializedResponse.m_nTotalItems++;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            BundlesContainingMediaResponse response = null;
            try
            {
                CheckRequestValidness();
                CheckSignature(this);

                List<int> subs = null;
                List<int> cols = null;
                GetBundlesList(ref subs, ref cols);

                Dictionary<int, List<int>> channelsToSubsMapping = null;
                Dictionary<int, List<int>> channelsToColsMapping = null;

                CatalogDAL.Get_ChannelsByBundles(m_nGroupID, subs, cols, ref channelsToSubsMapping,
                    ref channelsToColsMapping);

                ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

                response = new BundlesContainingMediaResponse(m_oBundles);

                List<int> channelsOfMedia = searcher.GetMediaChannels(m_nGroupID, m_nMediaID);

                FillResponse(channelsOfMedia, channelsToSubsMapping, channelsToColsMapping, ref response);

            }
            catch (Exception ex)
            {
                log.Error("Exception at BundlesContainingMediaRequest", ex);
                throw ex;
            }

            return response;
        }
    }
}
