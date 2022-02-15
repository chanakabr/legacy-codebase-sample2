using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Manager;
using Tvinci.Data.Loaders;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.DataEntities;
using Phx.Lib.Log;
using System.Reflection;
using Core.Catalog.Response;
using Core.Catalog.Request;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class ChannelsListsLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string PicSize { get; set; }
        public int CategoryID { get; set; }

        #region Constructors
        public ChannelsListsLoader(int categoryID, int groupID, string userIP, int pageSize, int pageIndex, string picSize)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            PicSize = picSize;
            CategoryID = categoryID;
        }

        public ChannelsListsLoader(int categoryID, string userName, string userIP, int pageSize, int pageIndex, string picSize)
            : this(categoryID, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize)
        {
        }
        #endregion

        public object Execute()
        {
            object retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = ExecuteChannelsListAdapter(m_oResponse as ChannelDetailsResponse);
            }
            else
            {
                retVal = new dsItemInfo();
            }
            return retVal;
        }

        private dsItemInfo ExecuteChannelsListAdapter(ChannelDetailsResponse channelDetailsResponse)
        {
            dsItemInfo retVal = new dsItemInfo();

            if (channelDetailsResponse.m_lchannelList.Count != 0)
            {
                dsItemInfo.ChannelRow channelRow;

                foreach (channelObj channel in channelDetailsResponse.m_lchannelList)
                {
                    channelRow = retVal.Channel.NewChannelRow();
                    channelRow.ChannelId = channel.m_nChannelID.ToString();
                    channelRow.Title = channel.m_sTitle;
                    channelRow.Description = channel.m_sDescription;
                    retVal.Channel.AddChannelRow(channelRow);
                }
            }

            return retVal;
        }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new ChannelsListRequest()
            {
                m_nCategoryID = CategoryID
            };
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.ChannelsListRequest":
                        ChannelsListRequest channelListRequest = obj as ChannelsListRequest;
                        sText.AppendFormat("ChannelsListRequest: CategoryID = {0}, GroupID = {1}, PageIndex = {2}, PageSize = {3}", channelListRequest.m_nCategoryID, channelListRequest.m_nGroupID, channelListRequest.m_nPageIndex, channelListRequest.m_nPageSize);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.ChannelDetailsResponse":
                        ChannelDetailsResponse channelListResponse = obj as ChannelDetailsResponse;
                        sText.AppendFormat("ChannelDetailsResponse: TotalItems = {0}, ", channelListResponse.m_nTotalItems);
                        sText.AppendLine(channelListResponse.m_lchannelList.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }

        #region ISupportPaging method
        public bool TryGetItemsCount(out long count)
        {
            count = 0;

            if (m_oResponse == null)
                return false;

            count = m_oResponse.m_nTotalItems;

            return true;
        }
        #endregion

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
