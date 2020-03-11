using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using KLogMonitor;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.FlashChannelsMedia;
using TVPPro.Configuration.Media;
using TVPPro.SiteManager.Context;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class TVMFlashChannelLoader : TVMAdapter<XmlDocument>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private string m_tvmUser;
        private string m_tvmPass;
        private FlashLoadersParams m_FlashLoadersParams;

        #region Loader properties
        public long ChannelID
        {
            get
            {
                return Parameters.GetParameter<long>(eParameterType.Retrieve, "ChannelID", 0);
            }
            set
            {
                Parameters.SetParameter<long>(eParameterType.Retrieve, "ChannelID", value);
            }
        }

        public string FlashParamsSTR
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "FlashParamsSTR", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "FlashParamsSTR", value);
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
        #endregion

        public TVMFlashChannelLoader(string TVMUser, string TVMPass, long channelID, FlashLoadersParams FlashChannelParams)
        {
            m_tvmUser = TVMUser;
            m_tvmPass = TVMPass;
            ChannelID = channelID;
            m_FlashLoadersParams = FlashChannelParams;
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            FlashChannelsMedia result = new FlashChannelsMedia();

            var newChannel = new Tvinci.Data.TVMDataLoader.Protocols.FlashChannelsMedia.channel();
            newChannel.id = int.Parse(ChannelID.ToString());
            newChannel.number_of_items = PageSize;
            newChannel.start_index = PageIndex;
            result.root.request.channelCollection.Add(newChannel);

            result.root.flashvars.player_un = m_tvmUser;
            result.root.flashvars.player_pass = m_tvmPass;

            result.root.flashvars.pic_size1 = m_FlashLoadersParams.Pic1Size;
            result.root.flashvars.pic_size2 = m_FlashLoadersParams.Pic2Size;
            result.root.flashvars.pic_size3 = m_FlashLoadersParams.Pic3Size;

            if (m_FlashLoadersParams.IsPic1Poster)
            {
                result.root.flashvars.pic_size1_format = "POSTER";
                result.root.flashvars.pic_size1_quality = "HIGH";
            }

            if (m_FlashLoadersParams.IsPic2Poster)
            {
                result.root.flashvars.pic_size2_format = "POSTER";
                result.root.flashvars.pic_size2_quality = "HIGH";
            }

            result.root.flashvars.client_IP = SiteManager.Helper.SiteHelper.GetClientIP();
            result.root.flashvars.lang = m_FlashLoadersParams.Language;
            result.root.flashvars.file_format = m_FlashLoadersParams.MainFileFormat;
            result.root.flashvars.sub_file_format = m_FlashLoadersParams.SubFileFormat;
            result.root.flashvars.file_quality = file_quality.high;
            result.root.request.@params.with_info = WithInfo.ToString();
            result.root.request.@params.info_struct.statistics = true;
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();
            result.root.request.@params.info_struct.name.MakeSchemaCompliant();
            result.root.request.@params.info_struct.description.MakeSchemaCompliant();

            if (WithInfo)
            {
                string[] arrMetas = MediaConfiguration.Instance.Data.TVM.FlashMediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                foreach (string metaName in arrMetas)
                {
                    result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = metaName });
                }

                string[] arrTags = MediaConfiguration.Instance.Data.TVM.FlashMediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
                foreach (string tagName in arrTags)
                {
                    result.root.request.@params.info_struct.tags.tag_typeCollection.Add(new tag_type() { name = tagName });
                }
            }

            return result;
        }

        protected override XmlDocument PreCacheHandling(object retrievedData)
        {
            FlashChannelsMedia data = retrievedData as FlashChannelsMedia;
            if (data == null)
            {
                throw new Exception("");
            }

            XmlDocument result = new XmlDocument();

            if (data.response.channelCollection.Count != 0)
            {
                responsechannel channel = data.response.channelCollection[0];

                if (channel.mediaCollection.Count != 0)
                {
                    XmlSerializer xs = new XmlSerializer(data.GetType());

                    using (StringWriter sw = new StringWriter())
                    {
                        XmlDocument xdoc = new XmlDocument();

                        xs.Serialize(sw, data);
                        xdoc.LoadXml(sw.ToString());

                        XmlNode xn = xdoc.SelectSingleNode("FlashChannelsMedia/response");

                        if (xn != null)
                            result.LoadXml(xn.OuterXml);
                    }
                }
            }

            //performanceLogger.Info("Tag Reflaction - ChannelID: " + ChannelID + ", Tags: " + result.Tags.Rows.Count + ", Total Time: " + span.TotalMilliseconds.ToString() + "ms");

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{507BAB9D-1482-4562-AD13-6BE33FA3432E}"); }
        }
    }
}