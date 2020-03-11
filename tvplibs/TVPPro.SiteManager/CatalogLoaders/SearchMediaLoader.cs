using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Manager;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.Helper;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class SearchMediaLoader : MultiMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public bool Exact { get; set; }
        public bool And { get; set; } //(cut_with)
        public OrderBy OrderBy { get; set; }
        public OrderDir OrderDir { get; set; }
        public string OrderMetaMame { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> MediaIDs { get; set; }
        public List<int> MediaTypes { get; set; }
        public List<KeyValue> Metas { get; set; }
        public List<KeyValue> Tags { get; set; }
        public List<KeyValue> AndList { get; set; }
        public List<KeyValue> OrList { get; set; }

        #region Constructors
        public SearchMediaLoader(int groupID, string userIP, int pageSize, int pageIndex, string picSize, bool exact, List<KeyValue> orList,
            List<KeyValue> andList, List<int> mediaTypes)
            : base(groupID, userIP, pageSize, pageIndex, picSize)
        {
            Exact = exact;
            MediaTypes = mediaTypes;
            OrList = orList;
            AndList = andList;
        }

        public SearchMediaLoader(int groupID, string userIP, int pageSize, int pageIndex, string picSize, bool exact, bool and, OrderBy orderBy, OrderDir orderDir, string orderValue, string name,
            string description, List<int> mediaIDs, List<int> mediaTypes, List<KeyValue> metas, List<KeyValue> tags)
            : base(groupID, userIP, pageSize, pageIndex, picSize)
        {
            Exact = exact;
            And = and;
            OrderBy = orderBy;
            OrderDir = orderDir;
            OrderMetaMame = orderValue;
            Name = name;
            Description = description;
            MediaIDs = mediaIDs;
            MediaTypes = mediaTypes;
            Metas = metas;
            Tags = tags;
        }
        public SearchMediaLoader(string userName, string userIP, int pageSize, int pageIndex, string picSize, bool exact, bool and, OrderBy orderBy, OrderDir orderDir, string orderValue, string name,
            string description, List<int> mediaIDs, List<int> mediaTypes, List<KeyValue> metas, List<KeyValue> tags)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize, exact, and, orderBy, orderDir, orderValue, name, description, mediaIDs, mediaTypes, metas, tags)
        {
        }
        public SearchMediaLoader(int groupID, string userIP, int pageSize, int pageIndex, string picSize, string searchText)
            : base(groupID, userIP, pageSize, pageIndex, picSize)
        {
            Name = searchText;
            Description = searchText;
        }

        public SearchMediaLoader(string userName, string userIP, int pageSize, int pageIndex, string picSize, string searchText)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize, searchText)
        {
        }
        #endregion

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
                    Metas = CatalogHelper.GetMetasTagsFromConfiguration("meta", Name);
                    foreach (var meta in Metas)
                    {
                        TagAndMetaList.Add(meta);
                    }
                    Tags = CatalogHelper.GetMetasTagsFromConfiguration("tag", Name);
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

        public override string GetLoaderCachekey()
        {
            StringBuilder key = new StringBuilder();

            // g = GroupId
            // ps = PageSize
            // pi = PageIndex
            // e = Exact
            // a = And
            // ob = OrderBy
            // od = OrderDir
            // om = OrderMetaMame
            // n = Name
            // d = Description
            // ids = MediaIDs
            // mt = MediaTypes
            // m = Metas
            // t = Tags


            key.AppendFormat("search_g={0}_ps={1}_pi={2}_e={3}_a={4}_ob={5}_od={6}", GroupID, PageSize, PageIndex, Exact, And, OrderBy, OrderDir);
            if (!string.IsNullOrEmpty(OrderMetaMame))
                key.AppendFormat("_om={0}", OrderMetaMame);
            if (!string.IsNullOrEmpty(Name))
                key.AppendFormat("_n={0}", Name);
            if (!string.IsNullOrEmpty(Description))
                key.AppendFormat("_d={0}", Description);
            if (MediaIDs != null && MediaIDs.Count > 0)
                key.AppendFormat("_ids={0}", string.Join(",", MediaIDs.Select(id => id.ToString()).ToArray()));
            if (MediaTypes != null && MediaTypes.Count > 0)
                key.AppendFormat("_mt={0}", string.Join(",", MediaTypes.Select(type => type.ToString()).ToArray()));
            if (Metas != null && Metas.Count > 0)
                key.AppendFormat("_m={0}", string.Join(",", Metas.Select(meta => string.Format("{0}:{1}", meta.m_sKey, meta.m_sValue)).ToArray()));
            if (Tags != null && Tags.Count > 0)
                key.AppendFormat("_t={0}", string.Join(",", Tags.Select(tag => string.Format("{0}:{1}", tag.m_sKey, tag.m_sValue)).ToArray()));

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
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaSearchRequest":
                        MediaSearchRequest searchRequest = obj as MediaSearchRequest;
                        sText.Append(searchRequest.ToStringEx());

                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaIdsResponse":
                        MediaIdsResponse mediaIdsResponse = obj as MediaIdsResponse;
                        sText.AppendFormat("MediaIdsResponse for Search: TotalItems = {0}, ", mediaIdsResponse.m_nTotalItems);
                        sText.AppendLine(mediaIdsResponse.m_nMediaIds.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }
    }
}
