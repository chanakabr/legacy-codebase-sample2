using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.DataEntities;
using System.Data;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class SubscriptionMediaLoader : MultiMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int SubscriptionID { get; set; }
        public OrderBy OrderBy { get; set; }
        public OrderDir OrderDir { get; set; }
        public string OrderMetaMame { get; set; }
        public string Name { get; set; }
        public List<int> MediaTypes { get; set; }

        #region Constructors
        public SubscriptionMediaLoader(int subscriptionID, string userName, string userIP, int pageSize, int pageIndex, string picSize, OrderBy orderBy, OrderDir orderDir, string orderValue)
            : this(subscriptionID, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize, orderBy, orderDir, orderValue)
        {
        }

        public SubscriptionMediaLoader(int subscriptionID, int groupID, string userIP, int pageSize, int pageIndex, string picSize, OrderBy orderBy, OrderDir orderDir, string orderValue)
            : this(subscriptionID, groupID, userIP, pageSize, pageIndex, picSize)
        {
            OrderBy = orderBy;
            OrderDir = orderDir;
            OrderMetaMame = orderValue;
        }

        public SubscriptionMediaLoader(int subscriptionID, int groupID, string userIP, int pageSize, int pageIndex, string picSize)
            : base(groupID, userIP, pageSize, pageIndex, picSize)
        {
            SubscriptionID = subscriptionID;
        }

        public SubscriptionMediaLoader(int subscriptionID, string userName, string userIP, int pageSize, int pageIndex, string picSize)
            : this(subscriptionID, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize)
        {
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new BundleMediaRequest
            {
                m_nBundleID = SubscriptionID,
                m_eBundleType = eBundleType.SUBSCRIPTION,
                m_oOrderObj = new OrderObj()
                {
                    m_eOrderBy = OrderBy,
                    m_eOrderDir = OrderDir,
                    m_sOrderValue = OrderMetaMame,
                }
            };
            if (MediaTypes != null && MediaTypes.Count > 0)
            {
                string sbTypes = String.Join(";", MediaTypes.Select(type => type.ToString()).ToArray());
                ((BundleMediaRequest)m_oRequest).m_sMediaType = sbTypes;
            }
        }


        public override string GetLoaderCachekey()
        {
            //MediaType = mt,
            //OrderBy = ob,
            //OrderDie = od,
            //OrderMetaName = omn

            StringBuilder key = new StringBuilder();
            key.AppendFormat("subscription_id{0}_index{1}_size{2}_group{3}", SubscriptionID, PageIndex, PageSize, GroupID);
            if (MediaTypes != null && MediaTypes.Count > 0)
                key.AppendFormat("_mt={0}", string.Join(",", MediaTypes.Select(type => type.ToString()).ToArray()));
            key.AppendFormat("ob={0}od={1}", OrderBy, OrderDir);
            if (!string.IsNullOrEmpty(OrderMetaMame))
                key.AppendFormat("omn={0}", OrderMetaMame);
            return key.ToString();
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.SubscriptionMediaRequest":
                        BundleMediaRequest subMediaRequest = obj as BundleMediaRequest;
                        sText.AppendFormat("SubscriptionMediaRequest: SubscriptionID = {0}, GroupID = {1}, PageIndex = {2}, PageSize = {3}", subMediaRequest.m_nBundleID, subMediaRequest.m_nGroupID, subMediaRequest.m_nPageIndex, subMediaRequest.m_nPageSize);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaIdsResponse":
                        MediaIdsResponse mediaIDsResponse = obj as MediaIdsResponse;
                        sText.AppendFormat("MediaIdsResponse: TotalItems = {0}, ", mediaIDsResponse.m_nTotalItems);
                        sText.AppendLine(mediaIDsResponse.m_nMediaIds.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }
    }
}
