using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Epg
{
    [Serializable]
    public class EpgPicture
    {
        public Int32 PicWidth {set; get;}
        public Int32 PicHeight { set; get; }
        public string Ratio { set; get; }
        public string Url { set; get; }

        public EpgPicture()
        {
            PicHeight = 0;
            PicWidth = 0;
            Ratio = string.Empty;
            Url = string.Empty;
        }

        public void Initialize (Int32 picWidth , Int32 picHeight , string ratio, string url)
        {
            this.PicHeight = picHeight;
            this.PicWidth = picWidth;
            this.Ratio = ratio;
            this.Url = url;
        }
    }
}
