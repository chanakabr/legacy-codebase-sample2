using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders;
using TVPApi;
using TVPApiModule.Manager;
using TVPPro.SiteManager.CatalogLoaders;
using Core.Catalog.Request;
using Core.Catalog.Response;
using ApiObjects;
using ApiObjects.Response;

namespace TVPApiModule.CatalogLoaders
{
    public class APICategoryLoader : CategoryLoader
    {
        private string m_sCulture;

        public string Culture
        {
            get { return m_sCulture; }
            set
            {
                m_sCulture = value;
                Language = TextLocalizationManager.Instance.GetTextLocalization(GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).GetLanguageDBID(value);
            }
        }

        public  object Execute()
        {
            CategoryResponse retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = m_oResponse as CategoryResponse;
            }
            return retVal;
        }

        public APICategoryLoader(int groupID, string platform, string userIP, int categoryID, string language) :
            base(groupID, userIP, categoryID)
        {
            Platform = platform;
            Culture = language;
        }
    }
}
