using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoviFeeder.CMT_XSD
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class RoviNowtilusVodApi
    {

        private RoviNowtilusVodApiRequest requestField;

        private RoviNowtilusVodApiResponse responseField;

        private RoviNowtilusVodApiCampaign campaignField;

        /// <remarks/>
        public RoviNowtilusVodApiRequest Request
        {
            get
            {
                return this.requestField;
            }
            set
            {
                this.requestField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiResponse Response
        {
            get
            {
                return this.responseField;
            }
            set
            {
                this.responseField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiCampaign Campaign
        {
            get
            {
                return this.campaignField;
            }
            set
            {
                this.campaignField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiRequest
    {

        private string requestUriField;

        private string ip4AdressField;

        private string timeField;

        private uint timeUnixField;

        /// <remarks/>
        public string RequestUri
        {
            get
            {
                return this.requestUriField;
            }
            set
            {
                this.requestUriField = value;
            }
        }

        /// <remarks/>
        public string Ip4Adress
        {
            get
            {
                return this.ip4AdressField;
            }
            set
            {
                this.ip4AdressField = value;
            }
        }

        /// <remarks/>
        public string Time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        /// <remarks/>
        public uint TimeUnix
        {
            get
            {
                return this.timeUnixField;
            }
            set
            {
                this.timeUnixField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiResponse
    {

        private RoviNowtilusVodApiResponseSpecification specificationField;

        private System.DateTime timeField;

        private uint timeUnixField;

        /// <remarks/>
        public RoviNowtilusVodApiResponseSpecification Specification
        {
            get
            {
                return this.specificationField;
            }
            set
            {
                this.specificationField = value;
            }
        }

        /// <remarks/>
        public System.DateTime Time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        /// <remarks/>
        public uint TimeUnix
        {
            get
            {
                return this.timeUnixField;
            }
            set
            {
                this.timeUnixField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiResponseSpecification
    {

        private decimal versionField;

        /// <remarks/>
        public decimal Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaign
    {

        private RoviNowtilusVodApiCampaignCampaignMetaGroup campaignMetaGroupField;

        private RoviNowtilusVodApiCampaignPlacement[] placementListField;

        /// <remarks/>
        public RoviNowtilusVodApiCampaignCampaignMetaGroup CampaignMetaGroup
        {
            get
            {
                return this.campaignMetaGroupField;
            }
            set
            {
                this.campaignMetaGroupField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Placement", IsNullable = false)]
        public RoviNowtilusVodApiCampaignPlacement[] PlacementList
        {
            get
            {
                return this.placementListField;
            }
            set
            {
                this.placementListField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignCampaignMetaGroup
    {

        private byte campaignIdField;

        private byte campaignSortField;

        private RoviNowtilusVodApiCampaignCampaignMetaGroupTitleNameList titleNameListField;

        private string bannerUrlField;

        private object teaserUrlField;

        private object trailerUrlField;

        private string textField;

        private object maxLengthField;

        private RoviNowtilusVodApiCampaignCampaignMetaGroupVisibility visibilityField;

        /// <remarks/>
        public byte CampaignId
        {
            get
            {
                return this.campaignIdField;
            }
            set
            {
                this.campaignIdField = value;
            }
        }

        /// <remarks/>
        public byte CampaignSort
        {
            get
            {
                return this.campaignSortField;
            }
            set
            {
                this.campaignSortField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiCampaignCampaignMetaGroupTitleNameList TitleNameList
        {
            get
            {
                return this.titleNameListField;
            }
            set
            {
                this.titleNameListField = value;
            }
        }

        /// <remarks/>
        public string BannerUrl
        {
            get
            {
                return this.bannerUrlField;
            }
            set
            {
                this.bannerUrlField = value;
            }
        }

        /// <remarks/>
        public object TeaserUrl
        {
            get
            {
                return this.teaserUrlField;
            }
            set
            {
                this.teaserUrlField = value;
            }
        }

        /// <remarks/>
        public object TrailerUrl
        {
            get
            {
                return this.trailerUrlField;
            }
            set
            {
                this.trailerUrlField = value;
            }
        }

        /// <remarks/>
        public string Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        public object MaxLength
        {
            get
            {
                return this.maxLengthField;
            }
            set
            {
                this.maxLengthField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiCampaignCampaignMetaGroupVisibility Visibility
        {
            get
            {
                return this.visibilityField;
            }
            set
            {
                this.visibilityField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignCampaignMetaGroupTitleNameList
    {

        private RoviNowtilusVodApiCampaignCampaignMetaGroupTitleNameListTitleName titleNameField;

        /// <remarks/>
        public RoviNowtilusVodApiCampaignCampaignMetaGroupTitleNameListTitleName TitleName
        {
            get
            {
                return this.titleNameField;
            }
            set
            {
                this.titleNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignCampaignMetaGroupTitleNameListTitleName
    {

        private string langField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string lang
        {
            get
            {
                return this.langField;
            }
            set
            {
                this.langField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignCampaignMetaGroupVisibility
    {

        private RoviNowtilusVodApiCampaignCampaignMetaGroupVisibilityVisibilityPeriod visibilityPeriodField;

        private RoviNowtilusVodApiCampaignCampaignMetaGroupVisibilityVisibilityPlatforms visibilityPlatformsField;

        /// <remarks/>
        public RoviNowtilusVodApiCampaignCampaignMetaGroupVisibilityVisibilityPeriod VisibilityPeriod
        {
            get
            {
                return this.visibilityPeriodField;
            }
            set
            {
                this.visibilityPeriodField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiCampaignCampaignMetaGroupVisibilityVisibilityPlatforms VisibilityPlatforms
        {
            get
            {
                return this.visibilityPlatformsField;
            }
            set
            {
                this.visibilityPlatformsField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignCampaignMetaGroupVisibilityVisibilityPeriod
    {

        private string visibilityPeriodStartField;

        private string visibilityPeriodEndField;

        /// <remarks/>
        public string VisibilityPeriodStart
        {
            get
            {
                return this.visibilityPeriodStartField;
            }
            set
            {
                this.visibilityPeriodStartField = value;
            }
        }

        /// <remarks/>
        public string VisibilityPeriodEnd
        {
            get
            {
                return this.visibilityPeriodEndField;
            }
            set
            {
                this.visibilityPeriodEndField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignCampaignMetaGroupVisibilityVisibilityPlatforms
    {

        private bool visibilityCeField;

        private bool visibilityWebField;

        private bool visibilityMobileField;

        /// <remarks/>
        public bool VisibilityCe
        {
            get
            {
                return this.visibilityCeField;
            }
            set
            {
                this.visibilityCeField = value;
            }
        }

        /// <remarks/>
        public bool VisibilityWeb
        {
            get
            {
                return this.visibilityWebField;
            }
            set
            {
                this.visibilityWebField = value;
            }
        }

        /// <remarks/>
        public bool VisibilityMobile
        {
            get
            {
                return this.visibilityMobileField;
            }
            set
            {
                this.visibilityMobileField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignPlacement
    {

        private RoviNowtilusVodApiCampaignPlacementPlacementSort placementSortField;

        private ushort placementIdField;

        private byte contentPriorityField;

        private RoviNowtilusVodApiCampaignPlacementVisibility visibilityField;

        private RoviNowtilusVodApiCampaignPlacementPresentation presentationField;

        /// <remarks/>
        public RoviNowtilusVodApiCampaignPlacementPlacementSort PlacementSort
        {
            get
            {
                return this.placementSortField;
            }
            set
            {
                this.placementSortField = value;
            }
        }

        /// <remarks/>
        public ushort PlacementId
        {
            get
            {
                return this.placementIdField;
            }
            set
            {
                this.placementIdField = value;
            }
        }

        /// <remarks/>
        public byte ContentPriority
        {
            get
            {
                return this.contentPriorityField;
            }
            set
            {
                this.contentPriorityField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiCampaignPlacementVisibility Visibility
        {
            get
            {
                return this.visibilityField;
            }
            set
            {
                this.visibilityField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiCampaignPlacementPresentation Presentation
        {
            get
            {
                return this.presentationField;
            }
            set
            {
                this.presentationField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignPlacementPlacementSort
    {

        private string devcomField;

        private byte valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string devcom
        {
            get
            {
                return this.devcomField;
            }
            set
            {
                this.devcomField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public byte Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignPlacementVisibility
    {

        private RoviNowtilusVodApiCampaignPlacementVisibilityVisibilityPeriod visibilityPeriodField;

        private RoviNowtilusVodApiCampaignPlacementVisibilityVisibilityPlatforms visibilityPlatformsField;

        /// <remarks/>
        public RoviNowtilusVodApiCampaignPlacementVisibilityVisibilityPeriod VisibilityPeriod
        {
            get
            {
                return this.visibilityPeriodField;
            }
            set
            {
                this.visibilityPeriodField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiCampaignPlacementVisibilityVisibilityPlatforms VisibilityPlatforms
        {
            get
            {
                return this.visibilityPlatformsField;
            }
            set
            {
                this.visibilityPlatformsField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignPlacementVisibilityVisibilityPeriod
    {

        private string visibilityPeriodStartField;

        private string visibilityPeriodEndField;

        /// <remarks/>
        public string VisibilityPeriodStart
        {
            get
            {
                return this.visibilityPeriodStartField;
            }
            set
            {
                this.visibilityPeriodStartField = value;
            }
        }

        /// <remarks/>
        public string VisibilityPeriodEnd
        {
            get
            {
                return this.visibilityPeriodEndField;
            }
            set
            {
                this.visibilityPeriodEndField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignPlacementVisibilityVisibilityPlatforms
    {

        private bool visibilityCeField;

        private bool visibilityWebField;

        private bool visibilityMobileField;

        /// <remarks/>
        public bool VisibilityCe
        {
            get
            {
                return this.visibilityCeField;
            }
            set
            {
                this.visibilityCeField = value;
            }
        }

        /// <remarks/>
        public bool VisibilityWeb
        {
            get
            {
                return this.visibilityWebField;
            }
            set
            {
                this.visibilityWebField = value;
            }
        }

        /// <remarks/>
        public bool VisibilityMobile
        {
            get
            {
                return this.visibilityMobileField;
            }
            set
            {
                this.visibilityMobileField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignPlacementPresentation
    {

        private string presentationUrlField;

        private uint titleIdField;

        private RoviNowtilusVodApiCampaignPlacementPresentationTitleNameList titleNameListField;

        private ushort contentIdField;

        private RoviNowtilusVodApiCampaignPlacementPresentationLongTextList longTextListField;

        private string coverUrlField;

        private string trailerUrlField;

        private string previewUrlField;

        private string teaserUrlField;

        private object promoUrlField;

        /// <remarks/>
        public string PresentationUrl
        {
            get
            {
                return this.presentationUrlField;
            }
            set
            {
                this.presentationUrlField = value;
            }
        }

        /// <remarks/>
        public uint TitleId
        {
            get
            {
                return this.titleIdField;
            }
            set
            {
                this.titleIdField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiCampaignPlacementPresentationTitleNameList TitleNameList
        {
            get
            {
                return this.titleNameListField;
            }
            set
            {
                this.titleNameListField = value;
            }
        }

        /// <remarks/>
        public ushort ContentId
        {
            get
            {
                return this.contentIdField;
            }
            set
            {
                this.contentIdField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiCampaignPlacementPresentationLongTextList LongTextList
        {
            get
            {
                return this.longTextListField;
            }
            set
            {
                this.longTextListField = value;
            }
        }

        /// <remarks/>
        public string CoverUrl
        {
            get
            {
                return this.coverUrlField;
            }
            set
            {
                this.coverUrlField = value;
            }
        }

        /// <remarks/>
        public string TrailerUrl
        {
            get
            {
                return this.trailerUrlField;
            }
            set
            {
                this.trailerUrlField = value;
            }
        }

        /// <remarks/>
        public string PreviewUrl
        {
            get
            {
                return this.previewUrlField;
            }
            set
            {
                this.previewUrlField = value;
            }
        }

        /// <remarks/>
        public string TeaserUrl
        {
            get
            {
                return this.teaserUrlField;
            }
            set
            {
                this.teaserUrlField = value;
            }
        }

        /// <remarks/>
        public object PromoUrl
        {
            get
            {
                return this.promoUrlField;
            }
            set
            {
                this.promoUrlField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignPlacementPresentationTitleNameList
    {

        private RoviNowtilusVodApiCampaignPlacementPresentationTitleNameListTitleName titleNameField;

        /// <remarks/>
        public RoviNowtilusVodApiCampaignPlacementPresentationTitleNameListTitleName TitleName
        {
            get
            {
                return this.titleNameField;
            }
            set
            {
                this.titleNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignPlacementPresentationTitleNameListTitleName
    {

        private string langField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string lang
        {
            get
            {
                return this.langField;
            }
            set
            {
                this.langField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignPlacementPresentationLongTextList
    {

        private RoviNowtilusVodApiCampaignPlacementPresentationLongTextListLongText longTextField;

        /// <remarks/>
        public RoviNowtilusVodApiCampaignPlacementPresentationLongTextListLongText LongText
        {
            get
            {
                return this.longTextField;
            }
            set
            {
                this.longTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiCampaignPlacementPresentationLongTextListLongText
    {

        private string langField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string lang
        {
            get
            {
                return this.langField;
            }
            set
            {
                this.langField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
}
