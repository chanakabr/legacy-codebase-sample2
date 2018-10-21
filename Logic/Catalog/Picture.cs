using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Core.Catalog
{
    [DataContract]
    public class Picture
    {
        [DataMember]
        public string m_sSize = string.Empty;

        [DataMember]
        public string m_sURL = string.Empty;

        [DataMember]
        public string ratio = string.Empty;

        [DataMember]
        public int version;

        [DataMember]
        public string id = string.Empty;

        [DataMember]
        public bool isDefault = false;

        [DataMember]
        public long imageTypeId;

        [DataMember]
        public string imageTypeName;

        public Picture() { }

        public Picture(string sSize, string sURL, string picRatio)
        {
            m_sSize = sSize;
            m_sURL = sURL;
            ratio = picRatio;
        }

        public Picture(int groupId, CatalogManagement.Image image, string imageTypeName, string picRatio, PicSize picSize = null)
        {
            this.id = image.ContentId;
            this.ratio = picRatio;
            this.version = image.Version;
            this.imageTypeId = image.ImageTypeId;
            this.imageTypeName = imageTypeName;
            if (picSize != null)
            {
                this.m_sSize = string.Format("{0}X{1}", picSize.Width, picSize.Height);
                this.m_sURL = TVinciShared.ImageUtils.BuildImageUrl(groupId, this.id, this.version, picSize.Width, picSize.Height, 100);
            }
            else
            {                                                
                this.m_sURL = image.Url;
            }
        }
    }

}