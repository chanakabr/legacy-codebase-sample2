using ApiObjects.CrowdsourceItems.Base;
using Core.Catalog.Request;
using Core.Catalog.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using TVPApi;
using TVPApiModule.Manager;

namespace TVPApiModule.CatalogLoaders
{
    public class APICrowdsourceLoader : CatalogRequestManager, ILoaderAdapter
    {
        public long EpochLastDate { get; set; }
        private string m_sCulture;

        public string Culture
        {
            get { return m_sCulture; }
            set
            {
                m_sCulture = value;
                Language = TextLocalizationManager.Instance.GetTextLocalization(GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform, true)).GetLanguageDBID(Culture);
            }
        }

        public APICrowdsourceLoader(int groupID, string language, int pageSize, long epochLastDate, string userIP, PlatformType platform)
            : base(groupID, userIP, pageSize, 0)
        {
            this.Platform = platform.ToString();
            this.Culture = language;
            this.EpochLastDate = epochLastDate;
        }

        public object Execute()
        {
            BuildRequest();
            List<BaseCrowdsourceItem> retVal = null;
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                if (m_oResponse != null && ((CrowdsourceResponse)m_oResponse).CrowdsourceItems != null)
                {
                    retVal = ((CrowdsourceResponse)m_oResponse).CrowdsourceItems;
                }
            }  
            return retVal;
        }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new CrowdsourceRequest()
            {
                LastDate = EpochLastDate
            };
        }



        #region ILoaderAdapter not implemented methods
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

        protected override void Log(string message, object obj)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
