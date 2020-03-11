using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Helper;
using TVPApi;
using TVPPro.SiteManager.DataEntities;
using TVPPro.Configuration.Technical;
using TVPApiModule.Manager;
using Core.Catalog;
using ApiObjects.SearchObjects;
using Core.Catalog.Request;
using Core.Catalog.Response;
using OrderBy = ApiObjects.SearchObjects.OrderBy;

namespace TVPApiModule.CatalogLoaders
{
    public class APISearchMediaLoader : SearchMediaLoader
    {
        private string m_sCulture;

        public string Culture
        {
            get { return m_sCulture; }
            set
            {
                m_sCulture = value;
                Language = TextLocalizationManager.Instance.GetTextLocalization(GroupIDParent, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).GetLanguageDBID(value);
            }
        }

        public int GroupIDParent { get; set; }

        #region Constructors
        public APISearchMediaLoader(int groupID, PlatformType platform, string userIP, int pageSize, int pageIndex, string picSize, bool exact, List<KeyValue> orList,
            List<KeyValue> andList, List<int> mediaTypes)
            : base(groupID, userIP, pageSize, pageIndex, picSize, exact, orList, andList, mediaTypes)
        {
            GroupIDParent = SiteMapManager.GetInstance.GetPageData(GroupID, platform).GetTVMAccountByGroupID(groupID).BaseGroupID;
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            Platform = platform.ToString();
        }

        public APISearchMediaLoader(int groupID, int groupIDParent, string platform, string userIP, int pageSize, int pageIndex, string picSize, bool exact, bool and, 
            OrderBy orderBy, OrderDir orderDir, string orderValue, string name,
            string description, List<int> mediaIDs, List<int> mediaTypes, List<KeyValue> metas, List<KeyValue> tags) 
            : base(groupID, userIP, pageSize, pageIndex, picSize, exact, and, orderBy, orderDir, orderValue, name, description, mediaIDs, mediaTypes, metas, tags)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;
            Platform = platform;
        }

        public APISearchMediaLoader(int groupID, int groupIDParent, string platform, string userIP, int pageSize, int pageIndex, string picSize, string searchText)
            : base(groupID, userIP, pageSize, pageIndex, picSize, searchText)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;
            Platform = platform;
        }

        #endregion

        public object ApiExecuteMultiMediaAdapter(List<BaseObject> medias)
        {
            var techConfigFlashVars = GroupsManager.GetGroup(GroupIDParent).GetFlashVars((PlatformType)Enum.Parse(typeof(PlatformType), Platform));
            string fileFormat = techConfigFlashVars.FileFormat;
            string subFileFormat = (techConfigFlashVars.SubFileFormat.Split(';')).FirstOrDefault();
            return CatalogHelper.MediaObjToDsItemInfo(medias, PicSize, fileFormat, subFileFormat);
        }

        protected override void BuildSpecificRequest()
        {
            List<KeyValue> TagAndMetaList = new List<KeyValue>(); 
            m_oRequest = new MediaSearchFullRequest()
            {
                m_bExact = Exact,
                m_oOrderObj = new OrderObj()
                {
                    m_eOrderBy = OrderBy,
                    m_eOrderDir = OrderDir,
                    m_sOrderValue = OrderMetaMame,
                },
                m_nMediaTypes = MediaTypes,
            };

            //In case the request DOES NOT include an orList or an andList
            if ((OrList == null || OrList.Count() == 0) && (AndList == null || AndList.Count == 0))
            {
                //In case search is performed by free text on all metas/tags
                if ((Metas == null || Metas.Count() == 0) && (Tags == null || Tags.Count == 0))
                {
                    Metas = APICatalogHelper.GetMetasTagsFromConfiguration("meta", Name, GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform));
                    foreach (var meta in Metas)
                    {
                        TagAndMetaList.Add(meta);
                    }
                    Tags = APICatalogHelper.GetMetasTagsFromConfiguration("tag", Name, GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform));
                    foreach (var tag in Tags)
                    {
                        TagAndMetaList.Add(tag);
                    }

                    TagAndMetaList.Add(new KeyValue() { m_sKey = "Name", m_sValue = Name });
                    TagAndMetaList.Add(new KeyValue() { m_sKey = "Description", m_sValue = Name });
                }
                //In case search is performed by exact metas/tags, not by free text
                else
                {
                    foreach (var meta in Metas)
                    {
                        TagAndMetaList.Add(meta);
                    }

                    foreach (var tag in Tags)
                    {
                        TagAndMetaList.Add(tag);
                    }
                }

                if (And)
                {
                    (m_oRequest as MediaSearchFullRequest).m_AndList = TagAndMetaList;
                }
                else
                {
                    (m_oRequest as MediaSearchFullRequest).m_OrList = TagAndMetaList;
                }
            }
            //In case the request includes an orList or an andList
            else
            {
                (m_oRequest as MediaSearchFullRequest).m_AndList = AndList;
                (m_oRequest as MediaSearchFullRequest).m_OrList = OrList;
            }
        }
    }
}
