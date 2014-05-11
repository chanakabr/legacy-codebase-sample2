using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoviFeeder.EpisodeXSD
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

        private RoviNowtilusVodApiPresentationPromotionContentList promotionContentListField;

        private RoviNowtilusVodApiPresentationContent[] contentListField;

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
        public RoviNowtilusVodApiPresentationPromotionContentList PromotionContentList
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
        [System.Xml.Serialization.XmlArrayItemAttribute("Content", IsNullable = false)]
        public RoviNowtilusVodApiPresentationContent[] ContentList
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

        private string presentationTypeField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupPresentationHierarchicalLevelCount presentationHierarchicalLevelCountField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupPresentationTypeDisplay presentationTypeDisplayField;

        private System.DateTime lastEditDateField;

        private uint lastEditDateUnixField;

        private object creationDateField;

        private byte creationDateUnixField;

        private string presentationStatusField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupProviderList providerListField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupVisibilityPeriod visibilityPeriodField;

        private byte displayAsNewField;

        private byte displayAsLastChanceField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitle titleField;

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
        public RoviNowtilusVodApiPresentationPresentationMetaGroupPresentationHierarchicalLevelCount PresentationHierarchicalLevelCount
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
        public object CreationDate
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
        public byte CreationDateUnix
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupPresentationHierarchicalLevelCount
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

        private object visibilityPeriodStartField;

        private byte visibilityPeriodStartUnixField;

        private System.DateTime visibilityPeriodEndField;

        private uint visibilityPeriodEndUnixField;

        /// <remarks/>
        public object VisibilityPeriodStart
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
        public byte VisibilityPeriodStartUnix
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

        private object creationDateField;

        private byte creationDateUnixField;

        private uint titleIdField;

        private object relatedTitleIdListField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleType titleTypeField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleTypeDisplay titleTypeDisplayField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSeriesGroup seriesGroupField;

        private object copyrightIdField;

        private object copyrightDisplayField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProduction productionField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSynopsisList synopsisListField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameList titleNameListField;

        private object secondTitleNameListField;

        private object descriptionField;

        private string[] actorNameListField;

        private object roleNameListField;

        private object directorNameListField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleGenreName[] genresNameListField;

        private object categoryListField;

        private object badgeListField;

        private object dimensionListField;

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
        public object CreationDate
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
        public byte CreationDateUnix
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
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleType TitleType
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
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSeriesGroup SeriesGroup
        {
            get
            {
                return this.seriesGroupField;
            }
            set
            {
                this.seriesGroupField = value;
            }
        }

        /// <remarks/>
        public object CopyrightId
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
        public object CopyrightDisplay
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
        public object DirectorNameList
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
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleGenreName[] GenresNameList
        {
            get
            {
                return this.genresNameListField;
            }
            set
            {
                this.genresNameListField = value;
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
        public object DimensionList
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSeriesGroup
    {

        private ushort seriesIdField;

        private ushort seasonIdField;

        private byte seasonNoField;

        /// <remarks/>
        public ushort SeriesId
        {
            get
            {
                return this.seriesIdField;
            }
            set
            {
                this.seriesIdField = value;
            }
        }

        /// <remarks/>
        public ushort SeasonId
        {
            get
            {
                return this.seasonIdField;
            }
            set
            {
                this.seasonIdField = value;
            }
        }

        /// <remarks/>
        public byte SeasonNo
        {
            get
            {
                return this.seasonNoField;
            }
            set
            {
                this.seasonNoField = value;
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

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameListSeriesTitleName seriesTitleNameField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameListSeasonTitleName seasonTitleNameField;

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

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameListSeriesTitleName SeriesTitleName
        {
            get
            {
                return this.seriesTitleNameField;
            }
            set
            {
                this.seriesTitleNameField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameListSeasonTitleName SeasonTitleName
        {
            get
            {
                return this.seasonTitleNameField;
            }
            set
            {
                this.seasonTitleNameField = value;
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameListSeriesTitleName
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameListSeasonTitleName
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

        private object[] itemsField;

        private ItemsChoiceType[] itemsElementNameField;

        private string commentField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CreationDate", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("CreationDateUnix", typeof(uint))]
        [System.Xml.Serialization.XmlElementAttribute("LastEditDate", typeof(System.DateTime))]
        [System.Xml.Serialization.XmlElementAttribute("LastEditDateUnix", typeof(uint))]
        [System.Xml.Serialization.XmlElementAttribute("LicenseBaseType", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("LicenseCode", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("LicenseGrantsList", typeof(RoviNowtilusVodApiPresentationLicenseLicenseGrantsList))]
        [System.Xml.Serialization.XmlElementAttribute("LicensePeriodGroup", typeof(RoviNowtilusVodApiPresentationLicenseLicensePeriodGroup))]
        [System.Xml.Serialization.XmlElementAttribute("LicensePriceList", typeof(RoviNowtilusVodApiPresentationLicenseLicensePriceList))]
        [System.Xml.Serialization.XmlElementAttribute("LicenseSubTypeList", typeof(RoviNowtilusVodApiPresentationLicenseLicenseSubTypeList))]
        [System.Xml.Serialization.XmlElementAttribute("LicenseType", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("LicenseTypeDisplay", typeof(RoviNowtilusVodApiPresentationLicenseLicenseTypeDisplay))]
        [System.Xml.Serialization.XmlElementAttribute("ProviderId", typeof(string))]
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

        private ushort[] contentIdField;

        private string commentField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ContentId")]
        public ushort[] ContentId
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
    public partial class RoviNowtilusVodApiPresentationLicenseLicenseSubTypeList
    {

        private string licenseSubTypeField;

        /// <remarks/>
        public string LicenseSubType
        {
            get
            {
                return this.licenseSubTypeField;
            }
            set
            {
                this.licenseSubTypeField = value;
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
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType
    {

        /// <remarks/>
        CreationDate,

        /// <remarks/>
        CreationDateUnix,

        /// <remarks/>
        LastEditDate,

        /// <remarks/>
        LastEditDateUnix,

        /// <remarks/>
        LicenseBaseType,

        /// <remarks/>
        LicenseCode,

        /// <remarks/>
        LicenseGrantsList,

        /// <remarks/>
        LicensePeriodGroup,

        /// <remarks/>
        LicensePriceList,

        /// <remarks/>
        LicenseSubTypeList,

        /// <remarks/>
        LicenseType,

        /// <remarks/>
        LicenseTypeDisplay,

        /// <remarks/>
        ProviderId,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContentList
    {

        private RoviNowtilusVodApiPresentationPromotionContentListPromotionContent promotionContentField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentListPromotionContent PromotionContent
        {
            get
            {
                return this.promotionContentField;
            }
            set
            {
                this.promotionContentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPromotionContentListPromotionContent
    {

        private System.DateTime lastEditDateField;

        private uint lastEditDateUnixField;

        private System.DateTime creationDateField;

        private uint creationDateUnixField;

        private ushort contentIdField;

        private string contentBaseTypeField;

        private string contentTypeField;

        private RoviNowtilusVodApiPresentationPromotionContentListPromotionContentContentTypeDisplay contentTypeDisplayField;

        private RoviNowtilusVodApiPresentationPromotionContentListPromotionContentContentNameList contentNameListField;

        private string contentLanguageField;

        private RoviNowtilusVodApiPresentationPromotionContentListPromotionContentFormatList formatListField;

        private object parentalControlListField;

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
        public RoviNowtilusVodApiPresentationPromotionContentListPromotionContentContentTypeDisplay ContentTypeDisplay
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
        public RoviNowtilusVodApiPresentationPromotionContentListPromotionContentContentNameList ContentNameList
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
        public string ContentLanguage
        {
            get
            {
                return this.contentLanguageField;
            }
            set
            {
                this.contentLanguageField = value;
            }
        }

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentListPromotionContentFormatList FormatList
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
        public object ParentalControlList
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
    public partial class RoviNowtilusVodApiPresentationPromotionContentListPromotionContentContentTypeDisplay
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
    public partial class RoviNowtilusVodApiPresentationPromotionContentListPromotionContentContentNameList
    {

        private RoviNowtilusVodApiPresentationPromotionContentListPromotionContentContentNameListContentName contentNameField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentListPromotionContentContentNameListContentName ContentName
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
    public partial class RoviNowtilusVodApiPresentationPromotionContentListPromotionContentContentNameListContentName
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
    public partial class RoviNowtilusVodApiPresentationPromotionContentListPromotionContentFormatList
    {

        private RoviNowtilusVodApiPresentationPromotionContentListPromotionContentFormatListFormat formatField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentListPromotionContentFormatListFormat Format
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
    public partial class RoviNowtilusVodApiPresentationPromotionContentListPromotionContentFormatListFormat
    {

        private ushort formatIdField;

        private RoviNowtilusVodApiPresentationPromotionContentListPromotionContentFormatListFormatMediaType mediaTypeField;

        private string startUrlField;

        private ushort widthField;

        private ushort heightField;

        private string aspectRatioField;

        private string commentField;

        /// <remarks/>
        public ushort FormatId
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
        public RoviNowtilusVodApiPresentationPromotionContentListPromotionContentFormatListFormatMediaType MediaType
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
    public partial class RoviNowtilusVodApiPresentationPromotionContentListPromotionContentFormatListFormatMediaType
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
    public partial class RoviNowtilusVodApiPresentationContent
    {

        private System.DateTime lastEditDateField;

        private uint lastEditDateUnixField;

        private System.DateTime creationDateField;

        private uint creationDateUnixField;

        private ushort contentIdField;

        private RoviNowtilusVodApiPresentationContentSeriesGroup seriesGroupField;

        private string contentBaseTypeField;

        private string contentTypeField;

        private RoviNowtilusVodApiPresentationContentContentTypeDisplay contentTypeDisplayField;

        private RoviNowtilusVodApiPresentationContentContentNameList contentNameListField;

        private byte runTimeMinutesField;

        private ushort runTimeSecondsField;

        private RoviNowtilusVodApiPresentationContentSynopsisList synopsisListField;

        private RoviNowtilusVodApiPresentationContentFormatList formatListField;

        private RoviNowtilusVodApiPresentationContentParentalControlList parentalControlListField;

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
        public RoviNowtilusVodApiPresentationContentSeriesGroup SeriesGroup
        {
            get
            {
                return this.seriesGroupField;
            }
            set
            {
                this.seriesGroupField = value;
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
        public RoviNowtilusVodApiPresentationContentContentTypeDisplay ContentTypeDisplay
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
        public RoviNowtilusVodApiPresentationContentContentNameList ContentNameList
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
        public RoviNowtilusVodApiPresentationContentSynopsisList SynopsisList
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
        public RoviNowtilusVodApiPresentationContentFormatList FormatList
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
        public RoviNowtilusVodApiPresentationContentParentalControlList ParentalControlList
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
    public partial class RoviNowtilusVodApiPresentationContentSeriesGroup
    {

        private object seriesIdField;

        private ushort seasonIdField;

        private byte seasonNoField;

        private byte episodeNoField;

        private uint episodeIdField;

        /// <remarks/>
        public object SeriesId
        {
            get
            {
                return this.seriesIdField;
            }
            set
            {
                this.seriesIdField = value;
            }
        }

        /// <remarks/>
        public ushort SeasonId
        {
            get
            {
                return this.seasonIdField;
            }
            set
            {
                this.seasonIdField = value;
            }
        }

        /// <remarks/>
        public byte SeasonNo
        {
            get
            {
                return this.seasonNoField;
            }
            set
            {
                this.seasonNoField = value;
            }
        }

        /// <remarks/>
        public byte EpisodeNo
        {
            get
            {
                return this.episodeNoField;
            }
            set
            {
                this.episodeNoField = value;
            }
        }

        /// <remarks/>
        public uint EpisodeId
        {
            get
            {
                return this.episodeIdField;
            }
            set
            {
                this.episodeIdField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentContentTypeDisplay
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
    public partial class RoviNowtilusVodApiPresentationContentContentNameList
    {

        private RoviNowtilusVodApiPresentationContentContentNameListContentName contentNameField;

        private RoviNowtilusVodApiPresentationContentContentNameListContentNameLong contentNameLongField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentContentNameListContentName ContentName
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

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentContentNameListContentNameLong ContentNameLong
        {
            get
            {
                return this.contentNameLongField;
            }
            set
            {
                this.contentNameLongField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationContentContentNameListContentName
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
    public partial class RoviNowtilusVodApiPresentationContentContentNameListContentNameLong
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
    public partial class RoviNowtilusVodApiPresentationContentSynopsisList
    {

        private RoviNowtilusVodApiPresentationContentSynopsisListSynopsis synopsisField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentSynopsisListSynopsis Synopsis
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
    public partial class RoviNowtilusVodApiPresentationContentSynopsisListSynopsis
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
    public partial class RoviNowtilusVodApiPresentationContentFormatList
    {

        private RoviNowtilusVodApiPresentationContentFormatListFormat formatField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentFormatListFormat Format
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
    public partial class RoviNowtilusVodApiPresentationContentFormatListFormat
    {

        private ushort formatIdField;

        private RoviNowtilusVodApiPresentationContentFormatListFormatAudioTrackList audioTrackListField;

        private object subtitleTrackListField;

        private RoviNowtilusVodApiPresentationContentFormatListFormatMediaType mediaTypeField;

        private string startUrlField;

        private byte widthField;

        private byte heightField;

        private string aspectRatioField;

        private RoviNowtilusVodApiPresentationContentFormatListFormatDimensionList dimensionListField;

        private string commentField;

        /// <remarks/>
        public ushort FormatId
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
        public RoviNowtilusVodApiPresentationContentFormatListFormatAudioTrackList AudioTrackList
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
        public RoviNowtilusVodApiPresentationContentFormatListFormatMediaType MediaType
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
        public byte Width
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
        public byte Height
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
        public RoviNowtilusVodApiPresentationContentFormatListFormatDimensionList DimensionList
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
    public partial class RoviNowtilusVodApiPresentationContentFormatListFormatAudioTrackList
    {

        private RoviNowtilusVodApiPresentationContentFormatListFormatAudioTrackListAudioTrack audioTrackField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentFormatListFormatAudioTrackListAudioTrack AudioTrack
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
    public partial class RoviNowtilusVodApiPresentationContentFormatListFormatAudioTrackListAudioTrack
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
    public partial class RoviNowtilusVodApiPresentationContentFormatListFormatMediaType
    {

        private string shortNameField;

        private bool encryptedField;

        private string encryptionTypeField;

        private string encryptionKeyIdField;

        private string minimumMediaOutputProtectionProfileField;

        private RoviNowtilusVodApiPresentationContentFormatListFormatMediaTypeMinimumMediaOutputProtectionLevelGroup minimumMediaOutputProtectionLevelGroupField;

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
        public string EncryptionKeyId
        {
            get
            {
                return this.encryptionKeyIdField;
            }
            set
            {
                this.encryptionKeyIdField = value;
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
        public RoviNowtilusVodApiPresentationContentFormatListFormatMediaTypeMinimumMediaOutputProtectionLevelGroup MinimumMediaOutputProtectionLevelGroup
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
    public partial class RoviNowtilusVodApiPresentationContentFormatListFormatMediaTypeMinimumMediaOutputProtectionLevelGroup
    {

        private byte compressedDigitalAudioField;

        private byte unompressedDigitalAudioField;

        private ushort compressedDigitalVideoField;

        private byte unompressedDigitalVideoField;

        private byte analogVideoField;

        /// <remarks/>
        public byte CompressedDigitalAudio
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
        public byte UnompressedDigitalAudio
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
        public byte UnompressedDigitalVideo
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
    public partial class RoviNowtilusVodApiPresentationContentFormatListFormatDimensionList
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
    public partial class RoviNowtilusVodApiPresentationContentParentalControlList
    {

        private RoviNowtilusVodApiPresentationContentParentalControlListParentalControl parentalControlField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationContentParentalControlListParentalControl ParentalControl
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
    public partial class RoviNowtilusVodApiPresentationContentParentalControlListParentalControl
    {

        private string parentalControlSystemField;

        private string parentalControlIdField;

        private byte parentalControlAgeField;

        private RoviNowtilusVodApiPresentationContentParentalControlListParentalControlParentalControlDisplay parentalControlDisplayField;

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
        public RoviNowtilusVodApiPresentationContentParentalControlListParentalControlParentalControlDisplay ParentalControlDisplay
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
    public partial class RoviNowtilusVodApiPresentationContentParentalControlListParentalControlParentalControlDisplay
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
