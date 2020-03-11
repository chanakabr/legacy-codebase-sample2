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
        public int PicWidth { set; get; }

        [DataMember]
        public int PicHeight { set; get; }

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
        public int Version { set; get; }

        [DataMember]
        public bool IsProgramImage { set; get; }

        [DataMember]
        public long ImageTypeId { set; get; }

        [XmlIgnore]
        public int ChannelId { get; set; }

        [XmlIgnore]
        public int EpgProgramId { get; set; }

        [XmlIgnore]
        public string PicName { get; set; }

        [XmlIgnore]
        public string ProgramName { get; set; }

        [XmlIgnore]
        public string BaseUrl { get; set; }

        public EpgPicture()
        {
            PicHeight = 0;
            PicWidth = 0;
            Ratio = string.Empty;
            Url = string.Empty;
            PicID = 0;
            RatioId = 0;
        }

        public void Initialize(int picWidth, int picHeight, string ratio, string url)
        {
            this.PicHeight = picHeight;
            this.PicWidth = picWidth;
            this.Ratio = ratio;
            this.Url = url;

        }

        public void Initialize(int picWidth, int picHeight, string ratio, string url, int picID)
        {
            Initialize(picWidth, picHeight, ratio, url);
            this.PicID = picID;
        }

        public override string ToString()
        {
            return $"{{id:{Id}, url:{Url}, channelId:[{ChannelId}], programId:{EpgProgramId}}}";
        }
    }
}
