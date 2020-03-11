using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.Configuration.Media;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.SiteManager.DataEntities;
using TVPPro.Configuration.Technical;
using System.Linq;
using System.Collections.Generic;
using TVPPro.SiteManager.Context;

namespace TVPPro.SiteManager.Helper
{
    public class MediaMappingHelper
    {
        static MediaMappingHelper instance = null;
        static object instanceLock = new object();

        private Dictionary<string, string> TagsToMediaType = new Dictionary<string, string>();

        private MediaMappingHelper()
        {
            foreach (TVPPro.Configuration.Media.Redirect redirectTag in MediaConfiguration.Instance.Data.TVM.SearchValues.RedirectCollection)
            {
                foreach (TVPPro.Configuration.Media.Map map in redirectTag.Mapping.MapCollection)
                {
                    string tag = map.Tag;
                    string mediaType = map.MediaTypeID;

                    TagsToMediaType.Add(tag, mediaType);
                }
            }
        }

        public static MediaMappingHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new MediaMappingHelper();
                        }
                    }
                }

                return instance;
            }
        }

        public static void Initialize()
        {
            //if (MediaMappingHelper.Instance == null);
        }

        public string this[string tag]
        {
            get
            {
                if (TagsToMediaType.ContainsKey(tag))
                    return TagsToMediaType[tag];
                return String.Empty;

            }
        }
    }
}
