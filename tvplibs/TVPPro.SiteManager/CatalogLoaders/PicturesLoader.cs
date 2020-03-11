using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.Manager;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.SiteManager.Helper;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class PicturesLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        protected PictureCache m_oPictureCache;
        
        public List<int> PicturesIDs { get; set; }
        public string PicSize { get; set; }


        #region Constructors
        public PicturesLoader(List<int> picturesIDs, int groupID, string userIP, string picSize)
            : base(groupID, userIP, 0, 0)
        {
            PicturesIDs = picturesIDs;
            PicSize = picSize;
        }

        public PicturesLoader(List<int> picturesIDs, string userName, string userIP, string picSize)
            : this(picturesIDs, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, picSize)
        {
        }

        public PicturesLoader(List<int> picturesIDs, int groupID, string userIP, string picSize, Provider provider)
            : this(picturesIDs, groupID, userIP, picSize)
        {
            m_oProvider = provider;
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new PicRequest()
            {
                m_nPicIds = PicturesIDs
            };
        }

        public object Execute()
        {
            object retVal = null;
            List<BaseObject> lPicObj = null;
            m_oPictureCache = new PictureCache(PicturesIDs, GroupID, m_sUserIP, m_oFilter);
            m_oPictureCache.BuildRequest();
            lPicObj = (List<BaseObject>)m_oPictureCache.Execute();
            if (lPicObj != null)
            {
                retVal = ExecutePicturesAdapter(lPicObj);
            }
            else
            {
                retVal = new SerializableDictionary<string, string>();
            }
            return retVal;
        }

        private SerializableDictionary<string, string> ExecutePicturesAdapter(List<BaseObject> pictures)
        {
            SerializableDictionary<string, string> retVal = new SerializableDictionary<string, string>();

            if (pictures != null && pictures.Count > 0)
            {
                foreach (PicObj pic in pictures)
                {
                    StringBuilder picUrl = new StringBuilder(pic.m_Picture.Where(url => url.m_sSize.ToLower() == PicSize.ToLower()).FirstOrDefault().m_sURL);
                    if (!retVal.ContainsKey(pic.AssetId))
                    {
                        retVal.Add(pic.AssetId, picUrl.ToString());
                    }
                }
            }
            return retVal;
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

        protected override void Log(string message, object obj)
        {
            throw new NotImplementedException();
        }
    }
}
