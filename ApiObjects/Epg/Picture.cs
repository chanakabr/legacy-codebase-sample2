using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace ApiObjects.Epg
{
    [Serializable]
    [DataContract]
    public class EpgPicture
    {
       

        [DataMember]
        public Int32 PicWidth { set; get; }
        [DataMember]
        public Int32 PicHeight { set; get; }
        [DataMember]
        public string Ratio { set; get; }
      
        [DataMember(IsRequired = false)]
        public int PicID { set; get; }

        [DataMember(IsRequired = false)]
        public string Url { set; get; }

        [XmlIgnore]
        public int RatioId { set; get; }

        [DataMember]
        public string Id { set; get; }

        [DataMember]
        public Int32 Version { set; get; }

        public EpgPicture()
        {
            PicHeight = 0;
            PicWidth = 0;
            Ratio = string.Empty;
            Url = string.Empty;
            PicID = 0;
            RatioId = 0;
        }

        public void Initialize(Int32 picWidth, Int32 picHeight, string ratio, string url)
        {
            this.PicHeight = picHeight;
            this.PicWidth = picWidth;
            this.Ratio = ratio;
            this.Url = url;

        }

        public void Initialize(Int32 picWidth, Int32 picHeight, string ratio, string url, int picID)
        {
            Initialize(picWidth, picHeight, ratio, url);
            this.PicID = picID;
        }

    }
}
