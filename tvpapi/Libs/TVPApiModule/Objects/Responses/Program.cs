using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace TVPApiModule.Objects.Responses
{
    [DataContract]
    public class Program
    {
        [DataMember]
        public string AssetId;

        [DataMember]
        public DateTime m_dUpdateDate;

        [DataMember]
        public ApiObjects.eAssetTypes AssetType { get; set; }

        [DataMember]
        public ProgramData m_oProgram;

        public Program()
        {
            this.m_oProgram = new ProgramData();
            m_dUpdateDate = DateTime.MinValue;
            AssetType = ApiObjects.eAssetTypes.MEDIA;
        }

        public Program(ApiObjects.EPGChannelProgrammeObject source)
        {
            AssetType = ApiObjects.eAssetTypes.EPG;
            m_dUpdateDate = DateTime.MinValue;
            this.m_oProgram = new ProgramData(source);
        }
    }

    public class ProgramData
    {
        #region Members
        public const string DATE_FORMAT = "dd/MM/yyyy HH:mm:ss";

        public long EPG_ID;
        public string EPG_CHANNEL_ID;
        public string EPG_IDENTIFIER;
        public string NAME;
        public LanguageContainerDTO[] ProgrammeName;
        public string DESCRIPTION;
        public LanguageContainerDTO[] ProgrammeDescription;
        public string START_DATE;
        public string END_DATE;
        public string PIC_URL;
        public int PIC_ID;
        public string STATUS;
        public string IS_ACTIVE;
        public string GROUP_ID;
        public string UPDATER_ID;
        public string UPDATE_DATE;
        public string PUBLISH_DATE;
        public string CREATE_DATE;
        public int LIKE_COUNTER;

        public List<EPGDictionaryDTO> EPG_TAGS;
        public List<EPGDictionaryDTO> EPG_Meta;

        public List<EpgPictureDTO> EPG_PICTURES;

        public string media_id;

        // the linear media of the epg_channel
        public long LINEAR_MEDIA_ID;
        public int ENABLE_CDVR;
        public int ENABLE_CATCH_UP;
        public long CHANNEL_CATCH_UP_BUFFER;
        public int ENABLE_START_OVER;
        public int ENABLE_TRICK_PLAY;

        public string CRID;

        #endregion

        #region Ctors

        public ProgramData()
        {

        }

        public ProgramData(ApiObjects.EPGChannelProgrammeObject source)
        {
            if (source != null)
            {
                this.CHANNEL_CATCH_UP_BUFFER = source.CHANNEL_CATCH_UP_BUFFER;
                this.CREATE_DATE = source.CREATE_DATE;
                this.CRID = source.CRID;
                this.DESCRIPTION = source.DESCRIPTION;
                this.ENABLE_CATCH_UP = source.ENABLE_CATCH_UP;
                this.ENABLE_CDVR = source.ENABLE_CDVR;
                this.ENABLE_START_OVER = source.ENABLE_START_OVER;
                this.ENABLE_TRICK_PLAY = source.ENABLE_TRICK_PLAY;
                this.END_DATE = source.END_DATE;
                this.EPG_CHANNEL_ID = source.EPG_CHANNEL_ID;
                this.EPG_ID = source.EPG_ID;
                this.EPG_IDENTIFIER = source.EPG_IDENTIFIER;

                if (source.EPG_Meta != null)
                {
                    this.EPG_Meta = source.EPG_Meta.Select(o => new EPGDictionaryDTO(o)).ToList();
                }

                if (source.EPG_PICTURES != null)
                {
                    this.EPG_PICTURES = source.EPG_PICTURES.Select(o => new EpgPictureDTO(o)).ToList();
                }

                if (source.EPG_TAGS != null)
                {
                    this.EPG_TAGS = source.EPG_TAGS.Select(o => new EPGDictionaryDTO(o)).ToList();
                }

                this.GROUP_ID = source.GROUP_ID;
                this.IS_ACTIVE = source.IS_ACTIVE;
                this.LIKE_COUNTER = source.LIKE_COUNTER;
                this.LINEAR_MEDIA_ID = source.LINEAR_MEDIA_ID;
                this.media_id = source.media_id;
                this.NAME = source.NAME;
                this.PIC_ID = source.PIC_ID;
                this.PIC_URL = source.PIC_URL;

                if (source.ProgrammeDescription != null)
                {
                    this.ProgrammeDescription = source.ProgrammeDescription.Select(o => new LanguageContainerDTO(o)).ToArray();
                }

                if (source.ProgrammeName != null)
                {
                    this.ProgrammeName = source.ProgrammeName.Select(o => new LanguageContainerDTO(o)).ToArray();
                }

                this.PUBLISH_DATE = source.PUBLISH_DATE;
                this.START_DATE = source.START_DATE;
                this.STATUS = source.STATUS;
                this.UPDATER_ID = source.UPDATER_ID;
                this.UPDATE_DATE = source.UPDATE_DATE;
            }
        }

        #endregion
    }


    [Serializable]
    [DataContract]
    public class EpgPictureDTO
    {
        #region Data Members

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

        #endregion

        #region Ctors

        public EpgPictureDTO()
        {

        }

        public EpgPictureDTO(ApiObjects.Epg.EpgPicture source)
        {
            this.BaseUrl = source.BaseUrl;
            this.ChannelId = source.ChannelId;
            this.EpgProgramId = source.EpgProgramId;
            this.Id = source.Id;
            this.ImageTypeId = source.ImageTypeId;
            this.IsProgramImage = source.IsProgramImage;
            this.PicHeight = source.PicHeight;
            this.PicID = source.PicID;
            this.PicName = source.PicName;
            this.PicWidth = source.PicWidth;
            this.ProgramName = source.ProgramName;
            this.Ratio = source.Ratio;
            this.RatioId = source.RatioId;
            this.Url = source.Url;
            this.Version = source.Version;
        }

        #endregion
    }


    [Serializable]
    [DataContract]
    public class EPGDictionaryDTO
    {
        [DataMember]
        public string Key;
        [DataMember]
        public string Value;
        [DataMember]
        public LanguageContainerDTO[] Values;

        public EPGDictionaryDTO()
        {

        }

        public EPGDictionaryDTO(ApiObjects.EPGDictionary source)
        {
            this.Key = source.Key;
            this.Value = source.Value;

            if (source.Values != null)
            {
                this.Values = source.Values.Select(i => new LanguageContainerDTO(i)).ToArray();
            }
        }
    }
}