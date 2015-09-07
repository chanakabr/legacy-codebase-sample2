using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.TVMDataLoader.Protocols.UserSocialMedias;
using TVPPro.Configuration.Technical;
using TVPPro.Configuration.Media;
using TVPPro.SiteManager.Helper;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.Context;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class UserSocialMediasLoader : TVMAdapter<dsItemInfo>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private string m_tvmUser;
        private string m_tvmPass;

        #region Properties
        public string PicSize
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "PicSize", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "PicSize", value);

            }
        }

        public bool IsPosterPic
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "IsPosterPic", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "IsPosterPic", value);
            }
        }

        public bool WithInfo
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "WithInfo", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "WithInfo", value);
            }
        }

        public bool WithFileTypes
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "WithFileTypes", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "WithFileTypes", value);
            }
        }

        public string SiteGuid
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "SiteGuid", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "SiteGuid", value);
            }
        }

        public TVPPro.SiteManager.TvinciPlatform.api.SocialAction SocialAction
        {
            get
            {
                return Parameters.GetParameter<TVPPro.SiteManager.TvinciPlatform.api.SocialAction>(eParameterType.Retrieve, "SocialAction", TVPPro.SiteManager.TvinciPlatform.api.SocialAction.UNKNOWN);
            }
            set
            {
                Parameters.SetParameter<TVPPro.SiteManager.TvinciPlatform.api.SocialAction>(eParameterType.Retrieve, "SocialAction", value);
            }
        }

        public TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform SocialPlatform
        {
            get
            {
                return Parameters.GetParameter<TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform>(eParameterType.Retrieve, "SocialPlatform", TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform.UNKNOWN);
            }
            set
            {
                Parameters.SetParameter<TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform>(eParameterType.Retrieve, "SocialPlatform", value);
            }
        }
        #endregion Properties

        public override eCacheMode GetCacheMode()
        {
            return eCacheMode.Never;
        }

        #region C'tor
        public UserSocialMediasLoader(string picSize)
        {
            PicSize = picSize;
            // Do nothing.
        }

        public UserSocialMediasLoader(string TVMUser, string TVMPass, string picSize)
        {
            m_tvmUser = TVMUser;
            m_tvmPass = TVMPass;

            //if (string.IsNullOrEmpty(picSize))
            //{
            //    throw new Exception("Picture size is null or empty");
            //}

            PicSize = picSize;
        }
        #endregion
        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            UserSocialMedias result = new UserSocialMedias();

            result.root.request.@params.site_guid = SiteGuid;
            result.root.request.@params.with_file_types = WithFileTypes.ToString();
            result.root.request.@params.social_action = SocialAction.ToString();
            result.root.request.@params.social_platform = SocialPlatform.ToString();
            result.root.request.@params.with_info = WithInfo.ToString();
            result.root.request.@params.with_info = WithInfo.ToString();
            result.root.request.@params.info_struct.statistics = true;
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();
            result.root.flashvars.no_cache = "1";

            result.root.flashvars.player_un = m_tvmUser;
            result.root.flashvars.player_pass = m_tvmPass;

            result.root.request.channel.number_of_items = PageSize;
            result.root.request.channel.start_index = PageIndex;

            // Type
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();
            result.root.flashvars.pic_size1 = PicSize;

            if (IsPosterPic)
            {
                result.root.flashvars.pic_size1_format = "POSTER";
                result.root.flashvars.pic_size1_quality = "HIGH";
            }

            result.root.flashvars.file_format = TechnicalConfiguration.Instance.Data.TVM.FlashVars.FileFormat;
            result.root.flashvars.file_quality = file_quality.high;

            if (WithInfo)
            {
                string[] arrMetas = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                foreach (string metaName in arrMetas)
                {
                    result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = metaName });
                }

                string[] arrTags = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
                foreach (string tagName in arrTags)
                {
                    result.root.request.@params.info_struct.tags.tag_typeCollection.Add(new tag_type() { name = tagName });
                }
            }

            return result;
        }

        protected override dsItemInfo PreCacheHandling(object retrievedData)
        {
            bool handleSingleTypeOnly = string.IsNullOrEmpty(PicSize);
            UserSocialMedias data = retrievedData as UserSocialMedias;
            if (data == null)
            {
                throw new Exception("");
            }
            dsItemInfo result = new dsItemInfo();

            if (data.response.channelCollection.Count != 0)
            {
                responsechannel channel = data.response.channelCollection[0];

                if (channel.mediaCollection.Count != 0)
                {
                    foreach (media media in channel.mediaCollection)
                    {
                        if (string.IsNullOrEmpty(media.id))
                        {
                            // not a valid situation
                            continue;
                        }

                        dsItemInfo.ItemRow itemRow = result.Item.NewItemRow();
                        itemRow.ID = media.id;

                        itemRow.MediaType = media.type.value;
                        itemRow.MediaTypeID = media.type.id;
                        itemRow.Title = media.title;
                        itemRow.DescriptionShort = media.description.value;
                        itemRow.Rate = Convert.ToDouble(media.rating.avg);
                        itemRow.ImageLink = media.pic_size1;
                        //itemRow.FileID = media.file_id;
                        itemRow.ViewCounter = Convert.ToInt32(media.views.count);
                        //itemRow.Duration = media.duration;
                        itemRow.URL = media.url;
                        //Add create date.
                        try
                        {
                            string[] date = media.date.Split('/');
                            itemRow.AddedDate = new DateTime(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
                        }
                        catch (Exception ex)
                        {
                            logger.Error("", ex);
                        }

                        // add sub file format info
                        //if (media.inner_medias.Count > 0)
                        //{
                        //    itemRow.SubFileID = media.inner_medias[0].file_id;
                        //    itemRow.SubFileFormat = media.inner_medias[0].file_format;
                        //    itemRow.SubDuration = media.inner_medias[0].duration;
                        //    itemRow.SubURL = media.inner_medias[0].url;
                        //}

                        if (WithInfo)
                        {
                            DataHelper.CollectMetasInfo(ref result, media);

                            DataHelper.CollectTagsInfo(ref result, media);

                            /*dsItemInfo.TagsRow rowTag = result.Tags.AddTagsRow(media.id);

                            foreach (tags_collectionstag_type tagType in media.tags_collections)
                            {
                                String sTagType = tagType.name;
                                foreach (tag tagElement in tagType.tagCollection)
                                {
                                    if (!result.Tags.Columns.Contains(sTagType))
                                    {
                                        System.Data.DataColumn colTagName = result.Tags.Columns.Add(sTagType, typeof(string));

                                        rowTag[colTagName] = tagElement.name;
                                    }
                                    else
                                    {
                                        rowTag[sTagType] += (!String.IsNullOrEmpty(rowTag[sTagType].ToString())) ? string.Concat(rowTag[sTagType], "|", tagElement.name) : tagElement.name;
                                    }
                                }
                            }
                            */
                            //
                        }
                        result.Item.AddItemRow(itemRow);
                    }
                }

                dsItemInfo.ChannelRow channelRow = result.Channel.NewChannelRow();
                channelRow.ChannelId = channel.id;
                //channelRow.EnableRssFeed = channel.rss;

                result.Channel.AddChannelRow(channelRow);
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{12C6FFD1-56B8-42D6-B00E-9B602D628417}"); }
        }

    }
}
