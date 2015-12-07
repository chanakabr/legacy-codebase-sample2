using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Manager;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;

namespace TVPApiModule.CatalogLoaders
{
    public class APIBundleMediaLoader : APIUnifiedSearchLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private string m_sCulture;

        public int bundleId
        {
            get;
            set;
        }
        public string mediaType
        {
            get;
            set;
        }
        public OrderObj orderObj
        {
            get;
            set;
        }
        public eBundleType bundleType
        {
            get;
            set;
        }

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
        //public APIBundleMediaLoader(int bundleId, int groupID, int groupIDParent, string platform, string userIP, string picSize) :
        //    this(bundleId, groupID, groupIDParent, platform, userIP, picSize)
        //{
        //}
        //int bundleId, string mediaType, OrderObj order, int groupID, string userIP, int pageSize, int pageIndex, string picSize
        public APIBundleMediaLoader(int bundleId, string mediaType, OrderObj order, int groupID, int groupIDParent, 
            PlatformType platform, string userIP, string picSize, int pageIndex, int pageSize, eBundleType bundleType, int domainId, string localeLanguage) :
            base(groupID, platform, domainId, userIP, pageSize, pageIndex, null, string.Empty, null, null, localeLanguage)
        {
            this.bundleId = bundleId;
            this.mediaType = mediaType;
            this.orderObj = order;
            this.bundleType = bundleType;

            //overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;
            Platform = platform.ToString();
        }

        #endregion

        #region Override Methods

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new BundleMediaRequest()
            {
                m_eBundleType = bundleType,
                m_nBundleID = bundleId,
                m_oOrderObj = orderObj,
                m_sMediaType = mediaType
            };
        }

        protected override void Log(string message, object obj)
        {
            if (!string.IsNullOrEmpty(message) && obj != null)
            {
                StringBuilder log = new StringBuilder();
                log.AppendLine(message);
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.BundleMediaRequest":
                    BundleMediaRequest bundleRequest = obj as BundleMediaRequest;
                    log.AppendFormat("BundleMediaRequest: BundleID = {0}, GroupID = {1}, BundleType = {2}", bundleRequest.m_nBundleID, bundleRequest.m_nGroupID, bundleRequest.m_eBundleType);
                    break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaIdsResponse":
                    MediaIdsResponse mediaIdsResponse = obj as MediaIdsResponse;
                    log.AppendFormat("MediaIdsResponse: TotalItemsInBundle = {0}, ", mediaIdsResponse.m_nTotalItems);
                    break;
                }

                if (logger != null)
                {
                    logger.Info(log.ToString());
                }
            }
        }

        public override string GetLoaderCachekey()
        {

            return string.Format("bundle_media_bundleId{0}mediaType{1}orderDir{2}orderBy{3}orderValue{4}index{5}size{6}group{7}",
                                    bundleId, mediaType, orderObj.m_eOrderDir, orderObj.m_eOrderBy,
                                    string.IsNullOrEmpty(orderObj.m_sOrderValue) ? string.Empty : orderObj.m_sOrderValue,
                                    PageIndex, PageSize, GroupID);
        }

        #endregion
        //public object ApiExecuteMultiMediaAdapter(List<BaseObject> medias)
        //{
        //    FlashVars techConfigFlashVars = ConfigManager.GetInstance().GetConfig(GroupIDParent, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).TechnichalConfiguration.Data.TVM.FlashVars;
        //    string fileFormat = techConfigFlashVars.FileFormat;
        //    string subFileFormat = (techConfigFlashVars.SubFileFormat.Split(';')).FirstOrDefault();
        //    return CatalogHelper.MediaObjToDsItemInfo(medias, PicSize, fileFormat, subFileFormat);
        //}
    }
}
