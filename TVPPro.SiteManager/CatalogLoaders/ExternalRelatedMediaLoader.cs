using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Objects;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class ExternalRelatedMediaLoader : RelatedMediaLoader
    {
        public string FreeParam { get; set; }

        #region Constructors
        public ExternalRelatedMediaLoader(int mediaID, List<int> mediaTypes, int groupID, string userIP, int pageSize, int pageIndex, string picSize, string freeParam = null)
            : base(mediaID, mediaTypes, groupID, userIP, pageSize, pageIndex, picSize)
        {
            MediaID = mediaID;
            MediaTypes = mediaTypes;
            FreeParam = freeParam;
        }

        public ExternalRelatedMediaLoader(int mediaID, List<int> mediaTypes, string userName, string userIP, int pageSize, int pageIndex, string picSize, string freeParam = null)
            : base(mediaID, mediaTypes, userName, userIP, pageSize, pageIndex, picSize)
        {
            FreeParam = freeParam;
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new MediaRelatedExternalRequest()
            {
                m_nMediaTypes = MediaTypes,
                m_nMediaID = MediaID,
                m_sFreeParam = FreeParam
            };
        }
    }
}
