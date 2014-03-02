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

        public int m_nLangID { get; set; }

        public MediaAutoCompleteRequest() : base() { }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            MediaAutoCompleteResponse response = null;
            try
            {
                MediaAutoCompleteRequest request = oBaseRequest as MediaAutoCompleteRequest;
                response = new MediaAutoCompleteResponse();

                if(request == null || string.IsNullOrEmpty(request.m_sPrefix))
                    throw new Exception("request object is null or Required variables is null");

                int nTotalItems = 0;

                ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

                if (searcher != null)
                {
                    MediaSearchObj searchObj = new MediaSearchObj();
                    searchObj.m_nGroupId = request.m_nGroupID;

                    Group oGroup = GroupsCache.Instance.GetGroup(request.m_nGroupID);
                    if (oGroup != null)
                    {
                        searchObj.m_oLangauge = oGroup.GetLanguage(request.m_nLangID);
                    }

                    searchObj.m_sName = request.m_sPrefix;
                    if (m_lMetas != null && m_lMetas.Count > 0)
                    {
                        foreach (string meta in m_lMetas)
                        {
                            searchObj.m_dOr.Add(new SearchValue() { m_sKey = meta, m_sKeyPrefix = "metas", m_sValue = request.m_sPrefix });
                        }
                    }

                    if (m_lTags != null && m_lTags.Count > 0)
                    {
                        foreach (string tag in m_lTags)
                        {
                            searchObj.m_dOr.Add(new SearchValue() { m_sKey = tag, m_sKeyPrefix = "tags", m_sValue = request.m_sPrefix });
                        }
                    }

                    response.lResults = searcher.GetAutoCompleteList(request.m_nGroupID, searchObj, request.m_nLangID, ref nTotalItems);
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
