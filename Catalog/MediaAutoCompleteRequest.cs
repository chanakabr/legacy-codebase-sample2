using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    [Serializable]
    [DataContract]
    public class MediaAutoCompleteRequest : BaseRequest, IRequestImp
    {
        [DataMember]
        public List<string> m_lMetas { get; set; }
        [DataMember]
        public List<string> m_lTags { get; set; }
        [DataMember]
        public string m_sPrefix { get; set; }
        [DataMember]
        public List<int> m_MediaTypes { get; set; }

        public MediaAutoCompleteRequest() : base() { }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            MediaAutoCompleteResponse response = null;
            try
            {
                MediaAutoCompleteRequest request = oBaseRequest as MediaAutoCompleteRequest;
                if (request == null || string.IsNullOrEmpty(request.m_sPrefix))
                    throw new Exception("request object is null or Required variables is null");
                CheckSignature(request);

                response = new MediaAutoCompleteResponse();

                int nTotalItems = 0;

                ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

                if (searcher != null)
                {
                    MediaSearchObj searchObj = new MediaSearchObj();
                    searchObj.m_nGroupId = request.m_nGroupID;
                    
                    searchObj.m_nPageSize = request.m_nPageSize;
                    searchObj.m_nPageIndex = request.m_nPageIndex;

                    Group oGroup = GroupsCache.Instance.GetGroup(request.m_nGroupID);
                    if (oGroup != null && request.m_oFilter != null)
                    {
                        searchObj.m_oLangauge = oGroup.GetLanguage(request.m_oFilter.m_nLanguage);
                    }

                    searchObj.m_sName = request.m_sPrefix;
                    if (request.m_lMetas != null && request.m_lMetas.Count > 0)
                    {
                        foreach (string meta in request.m_lMetas)
                        {
                            searchObj.m_dOr.Add(new SearchValue() { m_sKey = meta, m_sKeyPrefix = "metas", m_sValue = request.m_sPrefix });
                        }
                    }

                    if (request.m_lTags != null && request.m_lTags.Count > 0)
                    {
                        foreach (string tag in request.m_lTags)
                        {
                            searchObj.m_dOr.Add(new SearchValue() { m_sKey = tag, m_sKeyPrefix = "tags", m_sValue = request.m_sPrefix });
                        }
                    }
                    List<List<string>> jsonizedChannelsDefinitions = null;
                    if (Catalog.IsUseIPNOFiltering(request, ref searcher, ref jsonizedChannelsDefinitions))
                    {
                        searchObj.m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt = jsonizedChannelsDefinitions[0];
                        searchObj.m_lOrMediaNotInAnyOfTheseChannelsDefinitions = jsonizedChannelsDefinitions[1];
                    }
                    else
                    {
                        searchObj.m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt = null;
                        searchObj.m_lOrMediaNotInAnyOfTheseChannelsDefinitions = null;
                    }

                    response.lResults = searcher.GetAutoCompleteList(request.m_nGroupID, searchObj, request.m_oFilter.m_nLanguage, ref nTotalItems);
                    response.m_nTotalItems = nTotalItems;
                }
                else
                {
                    Logger.Logger.Log("Error", "AutoCompleteRequest - could not load searcher", "Catalog");
                }

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("AutoCompleteRequest", ex.Message), "Catalog");
                throw ex;
            }


            return response;

        }
    }
}
