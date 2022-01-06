using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataLoaders;
using System.Data;
using TVPPro.Configuration.Technical;
using Phx.Lib.Log;
using System.Reflection;

namespace TVPPro.SiteManager.Manager
{
    public class MediaTypes
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        static volatile MediaTypes m_instance;
        static object m_oLock = new object();
        private Dictionary<string, MediaTypeInfo> m_dictMediaTypesByID = new Dictionary<string, MediaTypeInfo>();
        private MLMediaType<string, MediaTypeInfo> m_dictMediaTypesByName = new MLMediaType<string, MediaTypeInfo>(StringComparer.InvariantCultureIgnoreCase);

        public struct MediaTypeInfo
        {
            public string TypeName;
            public string TypeID;
            public int TVMAccountID;
            public string TemplateName;
            public string PictureSize;
            public string PageFilename;
        }

        #region
        public Dictionary<string, MediaTypeInfo> MediaTypesByID
        {
            get { return m_dictMediaTypesByID; }
        }
        public MLMediaType<string, MediaTypeInfo> MediaTypesByName
        {
            get { return m_dictMediaTypesByName; }
        }

        public class MLMediaType<TKey, TValue> : IDictionary<string, MediaTypes.MediaTypeInfo>
        {
            //public MediaTypes.MediaTypeInfo this[TKey key]
            //{
            //    get
            //    {
            //        MediaTypeInfo mType = Keys.Where(x=> x == key).First()[
            //        mType.TypeName = TextLocalization.Instance[mType.TypeName];
            //        return mType;
            //    }
            //}

            //public new KeyCollection Keys
            //{
            //    get {                    
            //        Dictionary<string, MediaTypeInfo> dic = new Dictionary<string,MediaTypeInfo>();
            //        KeyCollection kk = new KeyCollection(dic);
            //        //return (from rows in base.Keys select dic.Add(rows, base[rows]));
            //        foreach (var k in base.Keys)
            //        {
            //            dic.Add(TextLocalization.Instance[k], this[k]);
            //        }

            //        return kk;
            //    }
            //}
            private Dictionary<string, MediaTypes.MediaTypeInfo> m_Dict;
            public MLMediaType()
            //: base()
            {
                m_Dict = new Dictionary<string, MediaTypeInfo>();
            }
            public MLMediaType(IEqualityComparer<string> comparer)
            //: base(comparer)
            {
                m_Dict = new Dictionary<string, MediaTypeInfo>(comparer);
            }

            public void Add(string key, MediaTypeInfo value)
            {
                m_Dict.Add(key, value);
            }

            public bool ContainsKey(string key)
            {
                return m_Dict.ContainsKey(key);
            }

            public ICollection<string> Keys
            {
                get
                {
                    return (ICollection<string>)m_Dict.Keys.Select(x => TextLocalization.Instance[x]).ToList();
                }
            }

            public bool Remove(string key)
            {
                return m_Dict.Remove(key);
            }

            public bool TryGetValue(string key, out MediaTypeInfo value)
            {
                return m_Dict.TryGetValue(key, out value);
            }

            public ICollection<MediaTypeInfo> Values
            {
                get
                {
                    return m_Dict.Values;
                }
            }

            public MediaTypeInfo this[string key]
            {
                get
                {
                    return m_Dict.Where(x => TextLocalization.Instance[x.Key].ToLower() == key.ToLower()).First().Value;
                }
                set
                {
                }
            }

            public void Add(KeyValuePair<string, MediaTypeInfo> item)
            {
                m_Dict.Add(item.Key, item.Value);
            }

            public void Clear()
            {
                m_Dict.Clear();
            }

            public bool Contains(KeyValuePair<string, MediaTypeInfo> item)
            {
                return m_Dict.Contains(item);
            }

            public void CopyTo(KeyValuePair<string, MediaTypeInfo>[] array, int arrayIndex)
            {

            }

            public int Count
            {
                get
                {
                    return m_Dict.Count;

                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public bool Remove(KeyValuePair<string, MediaTypeInfo> item)
            {
                return m_Dict.Remove(item.Key);
            }

            public IEnumerator<KeyValuePair<string, MediaTypeInfo>> GetEnumerator()
            {
                return m_Dict.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return m_Dict.GetEnumerator();
            }
        }
        #endregion

        #region Constructor
        public MediaTypes()
        {
            MediasContentTypeLoader MediasContentType = new MediasContentTypeLoader();
            TVPPro.SiteManager.DataEntities.dsMediaTypes m_Types = MediasContentType.Execute();

            if (m_Types.MediaTypes.Columns.Contains("TvmTypeID") && m_Types.MediaTypes.Columns.Contains("TypeName"))
            {
                foreach (DataRow row in m_Types.MediaTypes.Rows)
                {
                    if (!m_dictMediaTypesByID.Keys.Contains(row["TvmTypeID"].ToString()))
                    {
                        MediaTypeInfo mediaTypeInfo = new MediaTypeInfo();
                        mediaTypeInfo.PageFilename = row["PageFilename"].ToString();
                        mediaTypeInfo.PictureSize = row["PictureSize"].ToString();
                        mediaTypeInfo.TemplateName = row["TemplateName"].ToString();
                        mediaTypeInfo.TVMAccountID = (int)row["TVMAccountID"];
                        mediaTypeInfo.TypeID = row["TvmTypeID"].ToString();
                        mediaTypeInfo.TypeName = row["TypeName"].ToString();

                        m_dictMediaTypesByID.Add(mediaTypeInfo.TypeID, mediaTypeInfo);

                        string sMediaType = mediaTypeInfo.TypeName;
                        m_dictMediaTypesByName.Add(sMediaType, mediaTypeInfo);
                    }
                }
            }

        }
        #endregion

        public static MediaTypes Instance
        {
            get
            {
                if (m_instance == null)
                {
                    lock (m_oLock)
                    {
                        if (m_instance == null)
                        {
                            m_instance = new MediaTypes();
                        }
                    }
                }

                return m_instance;
            }
        }

        public MediaTypeInfo GetMediaTypeInfo(string sMediaType)
        {
            MediaTypeInfo mtiRet = new MediaTypeInfo();

            if (m_dictMediaTypesByID.Keys.Contains(sMediaType))
                mtiRet = m_dictMediaTypesByID[sMediaType];
            else if (m_dictMediaTypesByName.Keys.Select(x => x.ToLower()).Contains(sMediaType.ToLower()))
                mtiRet = m_dictMediaTypesByName[sMediaType];

            return mtiRet;
        }
    }
}
