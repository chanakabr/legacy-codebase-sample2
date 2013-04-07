using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Helper;
using TVPApi;
using TVPPro.SiteManager.DataEntities;
using TVPPro.Configuration.Technical;

namespace TVPApiModule.CatalogLoaders
{
    public class APISearchMediaLoader : SearchMediaLoader
    {
        public int GroupIDParent { get; set; }

        #region Constructors
        public APISearchMediaLoader(int groupID, int groupIDParent, string userIP, int pageSize, int pageIndex, string picSize, bool exact, bool and, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy, OrderDir orderDir, string orderValue, string name,
            string description, List<int> mediaIDs, List<int> mediaTypes, List<KeyValue> metas, List<KeyValue> tags) 
            : base(groupID, userIP, pageSize, pageIndex, picSize, exact, and, orderBy, orderDir, orderValue, name, description, mediaIDs, mediaTypes, metas, tags)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;          
        }

        public APISearchMediaLoader(int groupID, int groupIDParent, string userIP, int pageSize, int pageIndex, string picSize, int language, bool exact, bool and, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy, OrderDir orderDir, string orderValue, string name,
            string description, List<int> mediaIDs, List<int> mediaTypes, List<KeyValue> metas, List<KeyValue> tags)
            : this(groupID, groupIDParent, userIP, pageSize, pageIndex, picSize, exact, and, orderBy, orderDir, orderValue, name, description, mediaIDs, mediaTypes, metas, tags)
        {
            Language = language;
        }

        public APISearchMediaLoader(int groupID, int groupIDParent, string userIP, int pageSize, int pageIndex, string picSize, string searchText)
            : base(groupID, userIP, pageSize, pageIndex, picSize, searchText)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;
        }

        public APISearchMediaLoader(int groupID, int groupIDParent, string userIP, int pageSize, int pageIndex, string picSize, int language, string searchText)
            : this(groupID, groupIDParent, userIP, pageSize, pageIndex, picSize, searchText)
        {
            Language = language;
        }
        #endregion

        public object ApiExecuteMultiMediaAdapter(List<BaseObject> medias)
        {
            FlashVars techConfigFlashVars = ConfigManager.GetInstance().GetConfig(GroupIDParent, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).TechnichalConfiguration.Data.TVM.FlashVars;
            string fileFormat = techConfigFlashVars.FileFormat;
            string subFileFormat = (techConfigFlashVars.SubFileFormat.Split(';')).FirstOrDefault();
            return CatalogHelper.MediaObjToDsItemInfo(medias, PicSize, fileFormat, subFileFormat);
        }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new MediaSearchRequest() 
            {
                m_bExact = Exact,
                m_bAnd = And,
                m_oOrderObj = new OrderObj()
                {
                    m_eOrderBy = OrderBy,
                    m_eOrderDir = OrderDir,
                    m_eOrderValue = OrderMetaMame,
                },
                m_sName = Name,
                m_sDescription = Description,
                m_nMediaTypes = MediaTypes
            };
            if (Metas == null || Metas.Count() == 0)
            {
                Metas = APICatalogHelper.GetMetasTagsFromConfiguration("meta", Name, GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform));
                foreach (var meta in Metas)
                {
                    meta.m_sValue = Name;
                }
            }
            if (Tags == null || Tags.Count == 0)
            {
                Tags = APICatalogHelper.GetMetasTagsFromConfiguration("tag", Name, GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform));
                foreach (var tag in Tags)
                {
                    tag.m_sValue = Name;
                }
            }
            (m_oRequest as MediaSearchRequest).m_lMetas = Metas;
            (m_oRequest as MediaSearchRequest).m_lTags = Tags;
        }
    }
}
