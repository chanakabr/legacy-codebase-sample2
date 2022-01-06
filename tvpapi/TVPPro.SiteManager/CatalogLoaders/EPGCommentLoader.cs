using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Phx.Lib.Log;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Core.Catalog.Request;
using Core.Catalog.Response;
using ApiObjects;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class EPGCommentLoader : CatalogRequestManager, ILoaderAdapter
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int EpgProgramID { get; set; }
        public string ContentText { get; set; }
        public string Country { get; set; }
        public string Header { get; set; }
        public string SubHeader { get; set; }
        public string UDID { get; set; }
        public string Writer { get; set; }
        public bool AutoActive { get; set; }

        #region Constructors
        public EPGCommentLoader(int groupID, string userIP, int language, string siteGuid, string udid, int epgProgramID, string contentText, string country, string header, string subHeader, string writer, bool autoActive)
            : base(groupID, userIP, 0, 0)
        {
            EpgProgramID = epgProgramID;
            ContentText = contentText;
            Country = country;
            Header = header;
            SiteGuid = siteGuid;
            SubHeader = subHeader;
            UDID = udid;
            Writer = writer;
            Language = language;
            AutoActive = autoActive;

        }

        public EPGCommentLoader(string userName, string userIP, int language, string siteGuid, string udid, int epgProgramID, string contentText, string country, string header, string subHeader, string writer, bool autoActive)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, language, siteGuid, udid, epgProgramID, contentText, country, header, subHeader, writer, autoActive)
        {
        }

        public EPGCommentLoader(int groupID, string userIP, int language, string siteGuid, string udid, int epgProgramID, string contentText, string country, string header, string subHeader, string writer, bool autoActive, Provider provider)
            : this(groupID, userIP, language, siteGuid, udid, epgProgramID, contentText, country, header, subHeader, writer, autoActive)
        {
            m_oProvider = provider;
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new EpgCommentRequest()
            {
                m_nAssetID = EpgProgramID,
                m_sContentText = ContentText,
                m_sCountry = Country,
                m_sHeader = Header,
                m_sSubHeader = SubHeader,
                m_sUDID = UDID,
                m_sWriter = Writer,
                m_bAutoActive = AutoActive,
            };
        }

        public object Execute()
        {
            CommentResponse retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = m_oResponse as CommentResponse;
            }
            return retVal;
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgCommentRequest":
                        EpgCommentRequest commentRequest = obj as EpgCommentRequest;
                        sText.AppendFormat("EpgCommentRequest: EpgProgramID = {0}, GroupID = {1}, SiteGuid = {2}, AutoActive = {3},", EpgProgramID, GroupID, SiteGuid, AutoActive);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.CommentResponse":
                        CommentResponse commentResponse = obj as CommentResponse;
                        sText.AppendFormat("CommentResponse: StatusComment = {0}, ", commentResponse.eStatusComment);
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
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
        #endregion
    }
}
