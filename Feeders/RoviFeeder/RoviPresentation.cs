using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoviFeeder.VOD_object
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

        private RoviNowtilusVodApiPresentation presentationField;

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
        public RoviNowtilusVodApiPresentation Presentation
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
    public partial class RoviNowtilusVodApiPresentation
    {

        private RoviNowtilusVodApiPresentationPresentationMetaGroup presentationMetaGroupField;

        private RoviNowtilusVodApiPresentationLicense[] licenseListField;

        private RoviNowtilusVodApiPresentationPromotionContent[] promotionContentListField;

        private RoviNowtilusVodApiPresentationContentList contentListField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroup PresentationMetaGroup
        {
            get
            {
                return this.presentationMetaGroupField;
            }
            set
            {
                this.presentationMetaGroupField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("License", IsNullable = false)]
        public RoviNowtilusVodApiPresentationLicense[] LicenseList
        {
            get
            {
                return this.licenseListField;
            }
            set
            {
                this.licenseListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("PromotionContent", IsNullable = false)]
        public RoviNowtilusVodApiPresentationPromotionContent[] PromotionContentList
        {
            get
            {
                return this.promotionContentListField;
            }
            set
            {
                this.promotionContentListField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentList ContentList
        {
            get
            {
                return this.contentListField;
            }
            set
            {
                this.contentListField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroup
    {

        private byte presentationIdField;

        private string presentationTypeField;

        private byte presentationHierarchicalLevelCountField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupPresentationTypeDisplay presentationTypeDisplayField;

        private System.DateTime lastEditDateField;

        private uint lastEditDateUnixField;

        private System.DateTime creationDateField;

        private uint creationDateUnixField;

        private string presentationStatusField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupProviderList providerListField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupVisibilityPeriod visibilityPeriodField;

        private byte displayAsNewField;

        private byte displayAsLastChanceField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitle titleField;

        /// <remarks/>
        public byte PresentationId
        {
            get
            {
                return this.presentationIdField;
            }
            set
            {
                this.presentationIdField = value;
            }
        }

        /// <remarks/>
        public string PresentationType
        {
            get
            {
                return this.presentationTypeField;
            }
            set
            {
                this.presentationTypeField = value;
            }
        }

        /// <remarks/>
        public byte PresentationHierarchicalLevelCount
        {
            get
            {
                return this.presentationHierarchicalLevelCountField;
            }
            set
            {
                this.presentationHierarchicalLevelCountField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupPresentationTypeDisplay PresentationTypeDisplay
        {
            get
            {
                return this.presentationTypeDisplayField;
            }
            set
            {
                this.presentationTypeDisplayField = value;
            }
        }

        /// <remarks/>
        public System.DateTime LastEditDate
        {
            get
            {
                return this.lastEditDateField;
            }
            set
            {
                this.lastEditDateField = value;
            }
        }

        /// <remarks/>
        public uint LastEditDateUnix
        {
            get
            {
                return this.lastEditDateUnixField;
            }
            set
            {
                this.lastEditDateUnixField = value;
            }
        }

        /// <remarks/>
        public System.DateTime CreationDate
        {
            get
            {
                return this.creationDateField;
            }
            set
            {
                this.creationDateField = value;
            }
        }

        /// <remarks/>
        public uint CreationDateUnix
        {
            get
            {
                return this.creationDateUnixField;
            }
            set
            {
                this.creationDateUnixField = value;
            }
        }

        /// <remarks/>
        public string PresentationStatus
        {
            get
            {
                return this.presentationStatusField;
            }
            set
            {
                this.presentationStatusField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupProviderList ProviderList
        {
            get
            {
                return this.providerListField;
            }
            set
            {
                this.providerListField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupVisibilityPeriod VisibilityPeriod
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
        public byte DisplayAsNew
        {
            get
            {
                return this.displayAsNewField;
            }
            set
            {
                this.displayAsNewField = value;
            }
        }

        /// <remarks/>
        public byte DisplayAsLastChance
        {
            get
            {
                return this.displayAsLastChanceField;
            }
            set
            {
                this.displayAsLastChanceField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitle Title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupPresentationTypeDisplay
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupProviderList
    {

        private RoviNowtilusVodApiPresentationPresentationMetaGroupProviderListProvider providerField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupProviderListProvider Provider
        {
            get
            {
                return this.providerField;
            }
            set
            {
                this.providerField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupProviderListProvider
    {

        private string providerIdField;

        private string providerNameField;

        /// <remarks/>
        public string ProviderId
        {
            get
            {
                return this.providerIdField;
            }
            set
            {
                this.providerIdField = value;
            }
        }

        /// <remarks/>
        public string ProviderName
        {
            get
            {
                return this.providerNameField;
            }
            set
            {
                this.providerNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupVisibilityPeriod
    {

        private System.DateTime visibilityPeriodStartField;

        private uint visibilityPeriodStartUnixField;

        private System.DateTime visibilityPeriodEndField;

        private uint visibilityPeriodEndUnixField;

        /// <remarks/>
        public System.DateTime VisibilityPeriodStart
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
        public uint VisibilityPeriodStartUnix
        {
            get
            {
                return this.visibilityPeriodStartUnixField;
            }
            set
            {
                this.visibilityPeriodStartUnixField = value;
            }
        }

        /// <remarks/>
        public System.DateTime VisibilityPeriodEnd
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

        /// <remarks/>
        public uint VisibilityPeriodEndUnix
        {
            get
            {
                return this.visibilityPeriodEndUnixField;
            }
            set
            {
                this.visibilityPeriodEndUnixField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitle
    {

        private System.DateTime lastEditDateField;

        private uint lastEditDateUnixField;

        private System.DateTime creationDateField;

        private uint creationDateUnixField;

        private byte titleIdField;

        private object relatedTitleIdListField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleType[] titleTypeField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleTypeDisplay titleTypeDisplayField;

        private uint copyrightIdField;

        private string copyrightDisplayField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProduction productionField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSynopsisList synopsisListField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameList titleNameListField;

        private object secondTitleNameListField;

        private object descriptionField;

        private string[] actorNameListField;

        private object roleNameListField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleDirectorNameList directorNameListField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleGenreName[] genreNameListField;

        private object categoryListField;

        private object badgeListField;

        private object audioTrackListField;

        private object subtitleTrackListField;

        /// <remarks/>
        public System.DateTime LastEditDate
        {
            get
            {
                return this.lastEditDateField;
            }
            set
            {
                this.lastEditDateField = value;
            }
        }

        /// <remarks/>
        public uint LastEditDateUnix
        {
            get
            {
                return this.lastEditDateUnixField;
            }
            set
            {
                this.lastEditDateUnixField = value;
            }
        }

        /// <remarks/>
        public System.DateTime CreationDate
        {
            get
            {
                return this.creationDateField;
            }
            set
            {
                this.creationDateField = value;
            }
        }

        /// <remarks/>
        public uint CreationDateUnix
        {
            get
            {
                return this.creationDateUnixField;
            }
            set
            {
                this.creationDateUnixField = value;
            }
        }

        /// <remarks/>
        public byte TitleId
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
        public object RelatedTitleIdList
        {
            get
            {
                return this.relatedTitleIdListField;
            }
            set
            {
                this.relatedTitleIdListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("TitleType")]
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleType[] TitleType
        {
            get
            {
                return this.titleTypeField;
            }
            set
            {
                this.titleTypeField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleTypeDisplay TitleTypeDisplay
        {
            get
            {
                return this.titleTypeDisplayField;
            }
            set
            {
                this.titleTypeDisplayField = value;
            }
        }

        /// <remarks/>
        public uint CopyrightId
        {
            get
            {
                return this.copyrightIdField;
            }
            set
            {
                this.copyrightIdField = value;
            }
        }

        /// <remarks/>
        public string CopyrightDisplay
        {
            get
            {
                return this.copyrightDisplayField;
            }
            set
            {
                this.copyrightDisplayField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProduction Production
        {
            get
            {
                return this.productionField;
            }
            set
            {
                this.productionField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSynopsisList SynopsisList
        {
            get
            {
                return this.synopsisListField;
            }
            set
            {
                this.synopsisListField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameList TitleNameList
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
        public object SecondTitleNameList
        {
            get
            {
                return this.secondTitleNameListField;
            }
            set
            {
                this.secondTitleNameListField = value;
            }
        }

        /// <remarks/>
        public object Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("ActorName", IsNullable = false)]
        public string[] ActorNameList
        {
            get
            {
                return this.actorNameListField;
            }
            set
            {
                this.actorNameListField = value;
            }
        }

        /// <remarks/>
        public object RoleNameList
        {
            get
            {
                return this.roleNameListField;
            }
            set
            {
                this.roleNameListField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleDirectorNameList DirectorNameList
        {
            get
            {
                return this.directorNameListField;
            }
            set
            {
                this.directorNameListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("GenreName", IsNullable = false)]
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleGenreName[] GenreNameList
        {
            get
            {
                return this.genreNameListField;
            }
            set
            {
                this.genreNameListField = value;
            }
        }

        /// <remarks/>
        public object CategoryList
        {
            get
            {
                return this.categoryListField;
            }
            set
            {
                this.categoryListField = value;
            }
        }

        /// <remarks/>
        public object BadgeList
        {
            get
            {
                return this.badgeListField;
            }
            set
            {
                this.badgeListField = value;
            }
        }

        /// <remarks/>
        public object AudioTrackList
        {
            get
            {
                return this.audioTrackListField;
            }
            set
            {
                this.audioTrackListField = value;
            }
        }

        /// <remarks/>
        public object SubtitleTrackList
        {
            get
            {
                return this.subtitleTrackListField;
            }
            set
            {
                this.subtitleTrackListField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleType
    {

        private string devcomField;

        private bool deprecatedField;

        private bool deprecatedFieldSpecified;

        private string valueField;

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
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool deprecated
        {
            get
            {
                return this.deprecatedField;
            }
            set
            {
                this.deprecatedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deprecatedSpecified
        {
            get
            {
                return this.deprecatedFieldSpecified;
            }
            set
            {
                this.deprecatedFieldSpecified = value;
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleTypeDisplay
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProduction
    {

        private ushort releaseYearField;

        private object releaseYearStartField;

        private object releaseYearEndField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProductionReleaseYearDisplay releaseYearDisplayField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProductionProductionCountryList productionCountryListField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProductionProductionCountryDisplay productionCountryDisplayField;

        /// <remarks/>
        public ushort ReleaseYear
        {
            get
            {
                return this.releaseYearField;
            }
            set
            {
                this.releaseYearField = value;
            }
        }

        /// <remarks/>
        public object ReleaseYearStart
        {
            get
            {
                return this.releaseYearStartField;
            }
            set
            {
                this.releaseYearStartField = value;
            }
        }

        /// <remarks/>
        public object ReleaseYearEnd
        {
            get
            {
                return this.releaseYearEndField;
            }
            set
            {
                this.releaseYearEndField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProductionReleaseYearDisplay ReleaseYearDisplay
        {
            get
            {
                return this.releaseYearDisplayField;
            }
            set
            {
                this.releaseYearDisplayField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProductionProductionCountryList ProductionCountryList
        {
            get
            {
                return this.productionCountryListField;
            }
            set
            {
                this.productionCountryListField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProductionProductionCountryDisplay ProductionCountryDisplay
        {
            get
            {
                return this.productionCountryDisplayField;
            }
            set
            {
                this.productionCountryDisplayField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProductionReleaseYearDisplay
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProductionProductionCountryList
    {

        private string productionCountryField;

        /// <remarks/>
        public string ProductionCountry
        {
            get
            {
                return this.productionCountryField;
            }
            set
            {
                this.productionCountryField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProductionProductionCountryDisplay
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSynopsisList
    {

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSynopsisListSynopsis synopsisField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSynopsisListSynopsisShort synopsisShortField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSynopsisListSynopsis Synopsis
        {
            get
            {
                return this.synopsisField;
            }
            set
            {
                this.synopsisField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSynopsisListSynopsisShort SynopsisShort
        {
            get
            {
                return this.synopsisShortField;
            }
            set
            {
                this.synopsisShortField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSynopsisListSynopsis
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSynopsisListSynopsisShort
    {

        private string langField;

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
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameList
    {

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameListTitleName titleNameField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameListTitleName TitleName
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameListTitleName
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleDirectorNameList
    {

        private string directorNameField;

        /// <remarks/>
        public string DirectorName
        {
            get
            {
                return this.directorNameField;
            }
            set
            {
                this.directorNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleGenreName
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
    public partial class RoviNowtilusVodApiPresentationLicense
    {

        private System.DateTime lastEditDateField;

        private uint lastEditDateUnixField;

        private System.DateTime creationDateField;

        private uint creationDateUnixField;

        private string providerIdField;

        private string licenseBaseTypeField;

        private string licenseTypeField;

        private string[] licenseSubTypeListField;

        private string licenseCodeField;

        private RoviNowtilusVodApiPresentationLicenseLicenseTypeDisplay licenseTypeDisplayField;

        private RoviNowtilusVodApiPresentationLicenseLicensePriceList licensePriceListField;

        private byte licenseRentDurationHoursField;

        private bool licenseRentDurationHoursFieldSpecified;

        private RoviNowtilusVodApiPresentationLicenseLicensePeriodGroup licensePeriodGroupField;

        private byte licenseGraceTimeDaysField;

        private bool licenseGraceTimeDaysFieldSpecified;

        private RoviNowtilusVodApiPresentationLicenseLicenseGrantsList licenseGrantsListField;

        private string commentField;

        /// <remarks/>
        public System.DateTime LastEditDate
        {
            get
            {
                return this.lastEditDateField;
            }
            set
            {
                this.lastEditDateField = value;
            }
        }

        /// <remarks/>
        public uint LastEditDateUnix
        {
            get
            {
                return this.lastEditDateUnixField;
            }
            set
            {
                this.lastEditDateUnixField = value;
            }
        }

        /// <remarks/>
        public System.DateTime CreationDate
        {
            get
            {
                return this.creationDateField;
            }
            set
            {
                this.creationDateField = value;
            }
        }

        /// <remarks/>
        public uint CreationDateUnix
        {
            get
            {
                return this.creationDateUnixField;
            }
            set
            {
                this.creationDateUnixField = value;
            }
        }

        /// <remarks/>
        public string ProviderId
        {
            get
            {
                return this.providerIdField;
            }
            set
            {
                this.providerIdField = value;
            }
        }

        /// <remarks/>
        public string LicenseBaseType
        {
            get
            {
                return this.licenseBaseTypeField;
            }
            set
            {
                this.licenseBaseTypeField = value;
            }
        }

        /// <remarks/>
        public string LicenseType
        {
            get
            {
                return this.licenseTypeField;
            }
            set
            {
                this.licenseTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("LicenseSubType", IsNullable = false)]
        public string[] LicenseSubTypeList
        {
            get
            {
                return this.licenseSubTypeListField;
            }
            set
            {
                this.licenseSubTypeListField = value;
            }
        }

        /// <remarks/>
        public string LicenseCode
        {
            get
            {
                return this.licenseCodeField;
            }
            set
            {
                this.licenseCodeField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationLicenseLicenseTypeDisplay LicenseTypeDisplay
        {
            get
            {
                return this.licenseTypeDisplayField;
            }
            set
            {
                this.licenseTypeDisplayField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationLicenseLicensePriceList LicensePriceList
        {
            get
            {
                return this.licensePriceListField;
            }
            set
            {
                this.licensePriceListField = value;
            }
        }

        /// <remarks/>
        public byte LicenseRentDurationHours
        {
            get
            {
                return this.licenseRentDurationHoursField;
            }
            set
            {
                this.licenseRentDurationHoursField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LicenseRentDurationHoursSpecified
        {
            get
            {
                return this.licenseRentDurationHoursFieldSpecified;
            }
            set
            {
                this.licenseRentDurationHoursFieldSpecified = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationLicenseLicensePeriodGroup LicensePeriodGroup
        {
            get
            {
                return this.licensePeriodGroupField;
            }
            set
            {
                this.licensePeriodGroupField = value;
            }
        }

        /// <remarks/>
        public byte LicenseGraceTimeDays
        {
            get
            {
                return this.licenseGraceTimeDaysField;
            }
            set
            {
                this.licenseGraceTimeDaysField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LicenseGraceTimeDaysSpecified
        {
            get
            {
                return this.licenseGraceTimeDaysFieldSpecified;
            }
            set
            {
                this.licenseGraceTimeDaysFieldSpecified = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationLicenseLicenseGrantsList LicenseGrantsList
        {
            get
            {
                return this.licenseGrantsListField;
            }
            set
            {
                this.licenseGrantsListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string comment
        {
            get
            {
                return this.commentField;
            }
            set
            {
                this.commentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationLicenseLicenseTypeDisplay
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
    public partial class RoviNowtilusVodApiPresentationLicenseLicensePriceList
    {

        private RoviNowtilusVodApiPresentationLicenseLicensePriceListLicensePrice licensePriceField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationLicenseLicensePriceListLicensePrice LicensePrice
        {
            get
            {
                return this.licensePriceField;
            }
            set
            {
                this.licensePriceField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationLicenseLicensePriceListLicensePrice
    {

        private string currencyField;

        private decimal valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string currency
        {
            get
            {
                return this.currencyField;
            }
            set
            {
                this.currencyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public decimal Value
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
    public partial class RoviNowtilusVodApiPresentationLicenseLicensePeriodGroup
    {

        private System.DateTime licensePurchasePeriodStartField;

        private uint licensePurchasePeriodStartUnixField;

        private System.DateTime licensePurchasePeriodEndField;

        private uint licensePurchasePeriodEndUnixField;

        private System.DateTime licenseUsagePeriodStartField;

        private uint licenseUsagePeriodStartUnixField;

        private RoviNowtilusVodApiPresentationLicenseLicensePeriodGroupLicenseUsagePeriodEnd licenseUsagePeriodEndField;

        private RoviNowtilusVodApiPresentationLicenseLicensePeriodGroupLicenseUsagePeriodEndUnix licenseUsagePeriodEndUnixField;

        /// <remarks/>
        public System.DateTime LicensePurchasePeriodStart
        {
            get
            {
                return this.licensePurchasePeriodStartField;
            }
            set
            {
                this.licensePurchasePeriodStartField = value;
            }
        }

        /// <remarks/>
        public uint LicensePurchasePeriodStartUnix
        {
            get
            {
                return this.licensePurchasePeriodStartUnixField;
            }
            set
            {
                this.licensePurchasePeriodStartUnixField = value;
            }
        }

        /// <remarks/>
        public System.DateTime LicensePurchasePeriodEnd
        {
            get
            {
                return this.licensePurchasePeriodEndField;
            }
            set
            {
                this.licensePurchasePeriodEndField = value;
            }
        }

        /// <remarks/>
        public uint LicensePurchasePeriodEndUnix
        {
            get
            {
                return this.licensePurchasePeriodEndUnixField;
            }
            set
            {
                this.licensePurchasePeriodEndUnixField = value;
            }
        }

        /// <remarks/>
        public System.DateTime LicenseUsagePeriodStart
        {
            get
            {
                return this.licenseUsagePeriodStartField;
            }
            set
            {
                this.licenseUsagePeriodStartField = value;
            }
        }

        /// <remarks/>
        public uint LicenseUsagePeriodStartUnix
        {
            get
            {
                return this.licenseUsagePeriodStartUnixField;
            }
            set
            {
                this.licenseUsagePeriodStartUnixField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationLicenseLicensePeriodGroupLicenseUsagePeriodEnd LicenseUsagePeriodEnd
        {
            get
            {
                return this.licenseUsagePeriodEndField;
            }
            set
            {
                this.licenseUsagePeriodEndField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationLicenseLicensePeriodGroupLicenseUsagePeriodEndUnix LicenseUsagePeriodEndUnix
        {
            get
            {
                return this.licenseUsagePeriodEndUnixField;
            }
            set
            {
                this.licenseUsagePeriodEndUnixField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationLicenseLicensePeriodGroupLicenseUsagePeriodEnd
    {

        private string devcomField;

        private System.DateTime valueField;

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
        public System.DateTime Value
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
    public partial class RoviNowtilusVodApiPresentationLicenseLicensePeriodGroupLicenseUsagePeriodEndUnix
    {

        private string devcomField;

        private ulong valueField;

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
        public ulong Value
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
    public partial class RoviNowtilusVodApiPresentationLicenseLicenseGrantsList
    {

        private RoviNowtilusVodApiPresentationLicenseLicenseGrantsListContentIdList contentIdListField;

        private RoviNowtilusVodApiPresentationLicenseLicenseGrantsListDimensionList dimensionListField;

        private bool hasAllAudioTracksField;

        private object maxAudioTracksField;

        private RoviNowtilusVodApiPresentationLicenseLicenseGrantsListAudioTrackList audioTrackListField;

        private bool hasAllSubtitleTracksField;

        private object maxSubtitleTracksField;

        private object subtitleTrackListField;

        private object deviceIdBlacklistField;

        private object deviceTypeBlacklistField;

        private string[] territoryWhitelistField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationLicenseLicenseGrantsListContentIdList ContentIdList
        {
            get
            {
                return this.contentIdListField;
            }
            set
            {
                this.contentIdListField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationLicenseLicenseGrantsListDimensionList DimensionList
        {
            get
            {
                return this.dimensionListField;
            }
            set
            {
                this.dimensionListField = value;
            }
        }

        /// <remarks/>
        public bool HasAllAudioTracks
        {
            get
            {
                return this.hasAllAudioTracksField;
            }
            set
            {
                this.hasAllAudioTracksField = value;
            }
        }

        /// <remarks/>
        public object MaxAudioTracks
        {
            get
            {
                return this.maxAudioTracksField;
            }
            set
            {
                this.maxAudioTracksField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationLicenseLicenseGrantsListAudioTrackList AudioTrackList
        {
            get
            {
                return this.audioTrackListField;
            }
            set
            {
                this.audioTrackListField = value;
            }
        }

        /// <remarks/>
        public bool HasAllSubtitleTracks
        {
            get
            {
                return this.hasAllSubtitleTracksField;
            }
            set
            {
                this.hasAllSubtitleTracksField = value;
            }
        }

        /// <remarks/>
        public object MaxSubtitleTracks
        {
            get
            {
                return this.maxSubtitleTracksField;
            }
            set
            {
                this.maxSubtitleTracksField = value;
            }
        }

        /// <remarks/>
        public object SubtitleTrackList
        {
            get
            {
                return this.subtitleTrackListField;
            }
            set
            {
                this.subtitleTrackListField = value;
            }
        }

        /// <remarks/>
        public object DeviceIdBlacklist
        {
            get
            {
                return this.deviceIdBlacklistField;
            }
            set
            {
                this.deviceIdBlacklistField = value;
            }
        }

        /// <remarks/>
        public object DeviceTypeBlacklist
        {
            get
            {
                return this.deviceTypeBlacklistField;
            }
            set
            {
                this.deviceTypeBlacklistField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Territory", IsNullable = false)]
        public string[] TerritoryWhitelist
        {
            get
            {
                return this.territoryWhitelistField;
            }
            set
            {
                this.territoryWhitelistField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationLicenseLicenseGrantsListContentIdList
    {

        private byte contentIdField;

        private string commentField;

        /// <remarks/>
        public byte ContentId
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string comment
        {
            get
            {
                return this.commentField;
            }
            set
            {
                this.commentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationLicenseLicenseGrantsListDimensionList
    {

        private string dimensionField;

        /// <remarks/>
        public string Dimension
        {
            get
            {
                return this.dimensionField;
            }
            set
            {
                this.dimensionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationLicenseLicenseGrantsListAudioTrackList
    {

        private string audioTrackField;

        /// <remarks/>
        public string AudioTrack
        {
            get
            {
                return this.audioTrackField;
            }
            set
            {
                this.audioTrackField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContent
    {

        private object[] itemsField;

        private ItemsChoiceType[] itemsElementNameField;

        private string commentField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ContentBaseType", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("ContentId", typeof(byte))]
        [System.Xml.Serialization.XmlElementAttribute("ContentLanguage", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("ContentNameList", typeof(RoviNowtilusVodApiPresentationPromotionContentContentNameList))]
        [System.Xml.Serialization.XmlElementAttribute("ContentType", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("ContentTypeDisplay", typeof(RoviNowtilusVodApiPresentationPromotionContentContentTypeDisplay))]
        [System.Xml.Serialization.XmlElementAttribute("CreationDate", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("CreationDateUnix", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("FormatList", typeof(RoviNowtilusVodApiPresentationPromotionContentFormatList))]
        [System.Xml.Serialization.XmlElementAttribute("LastEditDate", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("LastEditDateUnix", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("ParentalControlList", typeof(RoviNowtilusVodApiPresentationPromotionContentParentalControlList))]
        [System.Xml.Serialization.XmlElementAttribute("RunTimeMinutes", typeof(object))]
        [System.Xml.Serialization.XmlElementAttribute("RunTimeSeconds", typeof(object))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType[] ItemsElementName
        {
            get
            {
                return this.itemsElementNameField;
            }
            set
            {
                this.itemsElementNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string comment
        {
            get
            {
                return this.commentField;
            }
            set
            {
                this.commentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContentContentNameList
    {

        private RoviNowtilusVodApiPresentationPromotionContentContentNameListContentName contentNameField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentContentNameListContentName ContentName
        {
            get
            {
                return this.contentNameField;
            }
            set
            {
                this.contentNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContentContentNameListContentName
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
    public partial class RoviNowtilusVodApiPresentationPromotionContentContentTypeDisplay
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
    public partial class RoviNowtilusVodApiPresentationPromotionContentFormatList
    {

        private RoviNowtilusVodApiPresentationPromotionContentFormatListFormat formatField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentFormatListFormat Format
        {
            get
            {
                return this.formatField;
            }
            set
            {
                this.formatField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContentFormatListFormat
    {

        private byte formatIdField;

        private RoviNowtilusVodApiPresentationPromotionContentFormatListFormatAudioTrackList audioTrackListField;

        private object subtitleTrackListField;

        private RoviNowtilusVodApiPresentationPromotionContentFormatListFormatMediaType mediaTypeField;

        private string startUrlField;

        private string widthField;

        private string heightField;

        private string aspectRatioField;

        private RoviNowtilusVodApiPresentationPromotionContentFormatListFormatDimensionList dimensionListField;

        private string commentField;

        /// <remarks/>
        public byte FormatId
        {
            get
            {
                return this.formatIdField;
            }
            set
            {
                this.formatIdField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentFormatListFormatAudioTrackList AudioTrackList
        {
            get
            {
                return this.audioTrackListField;
            }
            set
            {
                this.audioTrackListField = value;
            }
        }

        /// <remarks/>
        public object SubtitleTrackList
        {
            get
            {
                return this.subtitleTrackListField;
            }
            set
            {
                this.subtitleTrackListField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentFormatListFormatMediaType MediaType
        {
            get
            {
                return this.mediaTypeField;
            }
            set
            {
                this.mediaTypeField = value;
            }
        }

        /// <remarks/>
        public string StartUrl
        {
            get
            {
                return this.startUrlField;
            }
            set
            {
                this.startUrlField = value;
            }
        }

        /// <remarks/>
        public string Width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }

        /// <remarks/>
        public string Height
        {
            get
            {
                return this.heightField;
            }
            set
            {
                this.heightField = value;
            }
        }

        /// <remarks/>
        public string AspectRatio
        {
            get
            {
                return this.aspectRatioField;
            }
            set
            {
                this.aspectRatioField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentFormatListFormatDimensionList DimensionList
        {
            get
            {
                return this.dimensionListField;
            }
            set
            {
                this.dimensionListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string comment
        {
            get
            {
                return this.commentField;
            }
            set
            {
                this.commentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContentFormatListFormatAudioTrackList
    {

        private RoviNowtilusVodApiPresentationPromotionContentFormatListFormatAudioTrackListAudioTrack audioTrackField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentFormatListFormatAudioTrackListAudioTrack AudioTrack
        {
            get
            {
                return this.audioTrackField;
            }
            set
            {
                this.audioTrackField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContentFormatListFormatAudioTrackListAudioTrack
    {

        private string languageField;

        private byte channelCountField;

        /// <remarks/>
        public string Language
        {
            get
            {
                return this.languageField;
            }
            set
            {
                this.languageField = value;
            }
        }

        /// <remarks/>
        public byte ChannelCount
        {
            get
            {
                return this.channelCountField;
            }
            set
            {
                this.channelCountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContentFormatListFormatMediaType
    {

        private string shortNameField;

        private bool encryptedField;

        /// <remarks/>
        public string ShortName
        {
            get
            {
                return this.shortNameField;
            }
            set
            {
                this.shortNameField = value;
            }
        }

        /// <remarks/>
        public bool Encrypted
        {
            get
            {
                return this.encryptedField;
            }
            set
            {
                this.encryptedField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContentFormatListFormatDimensionList
    {

        private string dimensionField;

        /// <remarks/>
        public string Dimension
        {
            get
            {
                return this.dimensionField;
            }
            set
            {
                this.dimensionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContentParentalControlList
    {

        private RoviNowtilusVodApiPresentationPromotionContentParentalControlListParentalControl parentalControlField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentParentalControlListParentalControl ParentalControl
        {
            get
            {
                return this.parentalControlField;
            }
            set
            {
                this.parentalControlField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContentParentalControlListParentalControl
    {

        private string parentalControlSystemField;

        private string parentalControlIdField;

        private byte parentalControlAgeField;

        private RoviNowtilusVodApiPresentationPromotionContentParentalControlListParentalControlParentalControlDisplay parentalControlDisplayField;

        private string territoryField;

        /// <remarks/>
        public string ParentalControlSystem
        {
            get
            {
                return this.parentalControlSystemField;
            }
            set
            {
                this.parentalControlSystemField = value;
            }
        }

        /// <remarks/>
        public string ParentalControlId
        {
            get
            {
                return this.parentalControlIdField;
            }
            set
            {
                this.parentalControlIdField = value;
            }
        }

        /// <remarks/>
        public byte ParentalControlAge
        {
            get
            {
                return this.parentalControlAgeField;
            }
            set
            {
                this.parentalControlAgeField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentParentalControlListParentalControlParentalControlDisplay ParentalControlDisplay
        {
            get
            {
                return this.parentalControlDisplayField;
            }
            set
            {
                this.parentalControlDisplayField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string territory
        {
            get
            {
                return this.territoryField;
            }
            set
            {
                this.territoryField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContentParentalControlListParentalControlParentalControlDisplay
    {

        private string langField;

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
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType
    {

        /// <remarks/>
        ContentBaseType,

        /// <remarks/>
        ContentId,

        /// <remarks/>
        ContentLanguage,

        /// <remarks/>
        ContentNameList,

        /// <remarks/>
        ContentType,

        /// <remarks/>
        ContentTypeDisplay,

        /// <remarks/>
        CreationDate,

        /// <remarks/>
        CreationDateUnix,

        /// <remarks/>
        FormatList,

        /// <remarks/>
        LastEditDate,

        /// <remarks/>
        LastEditDateUnix,

        /// <remarks/>
        ParentalControlList,

        /// <remarks/>
        RunTimeMinutes,

        /// <remarks/>
        RunTimeSeconds,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentList
    {

        private RoviNowtilusVodApiPresentationContentListContent contentField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContent Content
        {
            get
            {
                return this.contentField;
            }
            set
            {
                this.contentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContent
    {

        private System.DateTime lastEditDateField;

        private uint lastEditDateUnixField;

        private System.DateTime creationDateField;

        private uint creationDateUnixField;

        private byte contentIdField;

        private string contentBaseTypeField;

        private string contentTypeField;

        private RoviNowtilusVodApiPresentationContentListContentContentTypeDisplay contentTypeDisplayField;

        private RoviNowtilusVodApiPresentationContentListContentContentNameList contentNameListField;

        private byte runTimeMinutesField;

        private ushort runTimeSecondsField;

        private RoviNowtilusVodApiPresentationContentListContentSynopsisList synopsisListField;

        private RoviNowtilusVodApiPresentationContentListContentFormatList formatListField;

        private RoviNowtilusVodApiPresentationContentListContentParentalControlList parentalControlListField;

        private string commentField;

        /// <remarks/>
        public System.DateTime LastEditDate
        {
            get
            {
                return this.lastEditDateField;
            }
            set
            {
                this.lastEditDateField = value;
            }
        }

        /// <remarks/>
        public uint LastEditDateUnix
        {
            get
            {
                return this.lastEditDateUnixField;
            }
            set
            {
                this.lastEditDateUnixField = value;
            }
        }

        /// <remarks/>
        public System.DateTime CreationDate
        {
            get
            {
                return this.creationDateField;
            }
            set
            {
                this.creationDateField = value;
            }
        }

        /// <remarks/>
        public uint CreationDateUnix
        {
            get
            {
                return this.creationDateUnixField;
            }
            set
            {
                this.creationDateUnixField = value;
            }
        }

        /// <remarks/>
        public byte ContentId
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
        public string ContentBaseType
        {
            get
            {
                return this.contentBaseTypeField;
            }
            set
            {
                this.contentBaseTypeField = value;
            }
        }

        /// <remarks/>
        public string ContentType
        {
            get
            {
                return this.contentTypeField;
            }
            set
            {
                this.contentTypeField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentContentTypeDisplay ContentTypeDisplay
        {
            get
            {
                return this.contentTypeDisplayField;
            }
            set
            {
                this.contentTypeDisplayField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentContentNameList ContentNameList
        {
            get
            {
                return this.contentNameListField;
            }
            set
            {
                this.contentNameListField = value;
            }
        }

        /// <remarks/>
        public byte RunTimeMinutes
        {
            get
            {
                return this.runTimeMinutesField;
            }
            set
            {
                this.runTimeMinutesField = value;
            }
        }

        /// <remarks/>
        public ushort RunTimeSeconds
        {
            get
            {
                return this.runTimeSecondsField;
            }
            set
            {
                this.runTimeSecondsField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentSynopsisList SynopsisList
        {
            get
            {
                return this.synopsisListField;
            }
            set
            {
                this.synopsisListField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentFormatList FormatList
        {
            get
            {
                return this.formatListField;
            }
            set
            {
                this.formatListField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentParentalControlList ParentalControlList
        {
            get
            {
                return this.parentalControlListField;
            }
            set
            {
                this.parentalControlListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string comment
        {
            get
            {
                return this.commentField;
            }
            set
            {
                this.commentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContentContentTypeDisplay
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
    public partial class RoviNowtilusVodApiPresentationContentListContentContentNameList
    {

        private RoviNowtilusVodApiPresentationContentListContentContentNameListContentName contentNameField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentContentNameListContentName ContentName
        {
            get
            {
                return this.contentNameField;
            }
            set
            {
                this.contentNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContentContentNameListContentName
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
    public partial class RoviNowtilusVodApiPresentationContentListContentSynopsisList
    {

        private RoviNowtilusVodApiPresentationContentListContentSynopsisListSynopsis synopsisField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentSynopsisListSynopsis Synopsis
        {
            get
            {
                return this.synopsisField;
            }
            set
            {
                this.synopsisField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContentSynopsisListSynopsis
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
    public partial class RoviNowtilusVodApiPresentationContentListContentFormatList
    {

        private RoviNowtilusVodApiPresentationContentListContentFormatListFormat formatField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentFormatListFormat Format
        {
            get
            {
                return this.formatField;
            }
            set
            {
                this.formatField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContentFormatListFormat
    {

        private byte formatIdField;

        private RoviNowtilusVodApiPresentationContentListContentFormatListFormatAudioTrackList audioTrackListField;

        private object subtitleTrackListField;

        private RoviNowtilusVodApiPresentationContentListContentFormatListFormatMediaType mediaTypeField;

        private string startUrlField;

        private ushort widthField;

        private ushort heightField;

        private string aspectRatioField;

        private RoviNowtilusVodApiPresentationContentListContentFormatListFormatDimensionList dimensionListField;

        private string commentField;

        /// <remarks/>
        public byte FormatId
        {
            get
            {
                return this.formatIdField;
            }
            set
            {
                this.formatIdField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentFormatListFormatAudioTrackList AudioTrackList
        {
            get
            {
                return this.audioTrackListField;
            }
            set
            {
                this.audioTrackListField = value;
            }
        }

        /// <remarks/>
        public object SubtitleTrackList
        {
            get
            {
                return this.subtitleTrackListField;
            }
            set
            {
                this.subtitleTrackListField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentFormatListFormatMediaType MediaType
        {
            get
            {
                return this.mediaTypeField;
            }
            set
            {
                this.mediaTypeField = value;
            }
        }

        /// <remarks/>
        public string StartUrl
        {
            get
            {
                return this.startUrlField;
            }
            set
            {
                this.startUrlField = value;
            }
        }

        /// <remarks/>
        public ushort Width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }

        /// <remarks/>
        public ushort Height
        {
            get
            {
                return this.heightField;
            }
            set
            {
                this.heightField = value;
            }
        }

        /// <remarks/>
        public string AspectRatio
        {
            get
            {
                return this.aspectRatioField;
            }
            set
            {
                this.aspectRatioField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentFormatListFormatDimensionList DimensionList
        {
            get
            {
                return this.dimensionListField;
            }
            set
            {
                this.dimensionListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string comment
        {
            get
            {
                return this.commentField;
            }
            set
            {
                this.commentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContentFormatListFormatAudioTrackList
    {

        private RoviNowtilusVodApiPresentationContentListContentFormatListFormatAudioTrackListAudioTrack audioTrackField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentFormatListFormatAudioTrackListAudioTrack AudioTrack
        {
            get
            {
                return this.audioTrackField;
            }
            set
            {
                this.audioTrackField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContentFormatListFormatAudioTrackListAudioTrack
    {

        private string languageField;

        private byte channelCountField;

        /// <remarks/>
        public string Language
        {
            get
            {
                return this.languageField;
            }
            set
            {
                this.languageField = value;
            }
        }

        /// <remarks/>
        public byte ChannelCount
        {
            get
            {
                return this.channelCountField;
            }
            set
            {
                this.channelCountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContentFormatListFormatMediaType
    {

        private string shortNameField;

        private bool encryptedField;

        private string encryptionTypeField;

        private string minimumMediaOutputProtectionProfileField;

        private RoviNowtilusVodApiPresentationContentListContentFormatListFormatMediaTypeMinimumMediaOutputProtectionLevelGroup minimumMediaOutputProtectionLevelGroupField;

        private ushort minimumDeviceSecurityLevelField;

        /// <remarks/>
        public string ShortName
        {
            get
            {
                return this.shortNameField;
            }
            set
            {
                this.shortNameField = value;
            }
        }

        /// <remarks/>
        public bool Encrypted
        {
            get
            {
                return this.encryptedField;
            }
            set
            {
                this.encryptedField = value;
            }
        }

        /// <remarks/>
        public string EncryptionType
        {
            get
            {
                return this.encryptionTypeField;
            }
            set
            {
                this.encryptionTypeField = value;
            }
        }

        /// <remarks/>
        public string MinimumMediaOutputProtectionProfile
        {
            get
            {
                return this.minimumMediaOutputProtectionProfileField;
            }
            set
            {
                this.minimumMediaOutputProtectionProfileField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentFormatListFormatMediaTypeMinimumMediaOutputProtectionLevelGroup MinimumMediaOutputProtectionLevelGroup
        {
            get
            {
                return this.minimumMediaOutputProtectionLevelGroupField;
            }
            set
            {
                this.minimumMediaOutputProtectionLevelGroupField = value;
            }
        }

        /// <remarks/>
        public ushort MinimumDeviceSecurityLevel
        {
            get
            {
                return this.minimumDeviceSecurityLevelField;
            }
            set
            {
                this.minimumDeviceSecurityLevelField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContentFormatListFormatMediaTypeMinimumMediaOutputProtectionLevelGroup
    {

        private ushort compressedDigitalAudioField;

        private ushort unompressedDigitalAudioField;

        private ushort compressedDigitalVideoField;

        private ushort unompressedDigitalVideoField;

        private byte analogVideoField;

        /// <remarks/>
        public ushort CompressedDigitalAudio
        {
            get
            {
                return this.compressedDigitalAudioField;
            }
            set
            {
                this.compressedDigitalAudioField = value;
            }
        }

        /// <remarks/>
        public ushort UnompressedDigitalAudio
        {
            get
            {
                return this.unompressedDigitalAudioField;
            }
            set
            {
                this.unompressedDigitalAudioField = value;
            }
        }

        /// <remarks/>
        public ushort CompressedDigitalVideo
        {
            get
            {
                return this.compressedDigitalVideoField;
            }
            set
            {
                this.compressedDigitalVideoField = value;
            }
        }

        /// <remarks/>
        public ushort UnompressedDigitalVideo
        {
            get
            {
                return this.unompressedDigitalVideoField;
            }
            set
            {
                this.unompressedDigitalVideoField = value;
            }
        }

        /// <remarks/>
        public byte AnalogVideo
        {
            get
            {
                return this.analogVideoField;
            }
            set
            {
                this.analogVideoField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContentFormatListFormatDimensionList
    {

        private string dimensionField;

        /// <remarks/>
        public string Dimension
        {
            get
            {
                return this.dimensionField;
            }
            set
            {
                this.dimensionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContentParentalControlList
    {

        private RoviNowtilusVodApiPresentationContentListContentParentalControlListParentalControl parentalControlField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentParentalControlListParentalControl ParentalControl
        {
            get
            {
                return this.parentalControlField;
            }
            set
            {
                this.parentalControlField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContentParentalControlListParentalControl
    {

        private string parentalControlSystemField;

        private string parentalControlIdField;

        private byte parentalControlAgeField;

        private RoviNowtilusVodApiPresentationContentListContentParentalControlListParentalControlParentalControlDisplay parentalControlDisplayField;

        private string territoryField;

        /// <remarks/>
        public string ParentalControlSystem
        {
            get
            {
                return this.parentalControlSystemField;
            }
            set
            {
                this.parentalControlSystemField = value;
            }
        }

        /// <remarks/>
        public string ParentalControlId
        {
            get
            {
                return this.parentalControlIdField;
            }
            set
            {
                this.parentalControlIdField = value;
            }
        }

        /// <remarks/>
        public byte ParentalControlAge
        {
            get
            {
                return this.parentalControlAgeField;
            }
            set
            {
                this.parentalControlAgeField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentListContentParentalControlListParentalControlParentalControlDisplay ParentalControlDisplay
        {
            get
            {
                return this.parentalControlDisplayField;
            }
            set
            {
                this.parentalControlDisplayField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string territory
        {
            get
            {
                return this.territoryField;
            }
            set
            {
                this.territoryField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentListContentParentalControlListParentalControlParentalControlDisplay
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
