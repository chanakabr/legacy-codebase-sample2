using ApiObjects;
using Core.Catalog.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class SlimAssetInfo
    {
        #region Data Members

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "type")]
        public int Type
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "description")]
        public string Description
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "images", NullValueHandling = NullValueHandling.Ignore)]
        public List<Image> Images
        {
            get;
            set;
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Freely built slim asset info object
        /// </summary>
        public SlimAssetInfo()
        {
        }

        /// <summary>
        /// Build slim asset info with specific values
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="images"></param>
        public SlimAssetInfo(string id, int type, string name, string description, List<Image> images)
        {
            this.Id = id;
            this.Type = type;
            this.Name = name;
            this.Description = description;
            this.Images = images;
        }

        /// <summary>
        /// Create a slim asset info object that represents a media. Images are optional
        /// </summary>
        /// <param name="media"></param>
        /// <param name="shouldAddImages"></param>
        public SlimAssetInfo(MediaObj media, bool shouldAddImages)
        {
            if (media != null)
            {
                Id = media.AssetId;
                Name = media.m_sName;
                Description = media.m_sDescription;

                if (media.m_oMediaType != null)
                {
                    Type = media.m_oMediaType.m_nTypeID;
                }

                if (shouldAddImages && media.m_lPicture != null)
                {
                    Images = new List<Image>();
                    foreach (var mediaPicture in media.m_lPicture)
                    {
                        Image picture = new Image(mediaPicture);
                        Images.Add(picture);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a slim asset info object that represents an EPG. Images are optional
        /// </summary>
        /// <param name="epg"></param>
        /// <param name="shouldAddImages"></param>
        public SlimAssetInfo(EPGChannelProgrammeObject epg, bool shouldAddImages)
        {
            if (epg != null)
            {
                Id = epg.EPG_ID.ToString();
                Type = 0;
                Name = epg.NAME;
                Description = epg.DESCRIPTION;

                if (shouldAddImages && epg.EPG_PICTURES != null)
                {
                    Images = new List<Image>();
                    foreach (var epgPicture in epg.EPG_PICTURES)
                    {
                        Image picture = new Image(epgPicture);
                        Images.Add(picture);
                    }
                }
            }
        }

        #endregion

    }
}