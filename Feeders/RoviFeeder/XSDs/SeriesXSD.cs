using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoviFeeder.SeriesXSD
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

        private RoviNowtilusVodApiPresentationPromotionContentList promotionContentListField;

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

        private RoviNowtilusVodApiPresentationPresentationMetaGroupVisibilityPeriodVisibilityPeriodEnd visibilityPeriodEndField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupVisibilityPeriodVisibilityPeriodEndUnix visibilityPeriodEndUnixField;

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
        public RoviNowtilusVodApiPresentationPresentationMetaGroupVisibilityPeriodVisibilityPeriodEnd VisibilityPeriodEnd
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
        public RoviNowtilusVodApiPresentationPresentationMetaGroupVisibilityPeriodVisibilityPeriodEndUnix VisibilityPeriodEndUnix
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupVisibilityPeriodVisibilityPeriodEnd
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupVisibilityPeriodVisibilityPeriodEndUnix
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitle
    {

        private string titleTypeField;

        private ushort titleIdField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleTypeDisplay titleTypeDisplayField;

        private System.DateTime lastEditDateField;

        private uint lastEditDateUnixField;

        private System.DateTime creationDateField;

        private uint creationDateUnixField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleProduction productionField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleSynopsisList synopsisListField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleTitleNameList titleNameListField;

        private object secondTitleNameListField;

        private object descriptionField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleGenreNameList genreNameListField;

        private object categoryListField;

        private object badgeListField;

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleDimensionList dimensionListField;

        /// <remarks/>
        public string TitleType
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
        public ushort TitleId
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
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleGenreNameList GenreNameList
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
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleDimensionList DimensionList
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleGenreNameList
    {

        private RoviNowtilusVodApiPresentationPresentationMetaGroupTitleGenreNameListGenreName genreNameField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPresentationMetaGroupTitleGenreNameListGenreName GenreName
        {
            get
            {
                return this.genreNameField;
            }
            set
            {
                this.genreNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleGenreNameListGenreName
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
    public partial class RoviNowtilusVodApiPresentationPresentationMetaGroupTitleDimensionList
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

        private RoviNowtilusVodApiPresentationPromotionContentListPromotionContentParentalControlList parentalControlListField;

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
        public RoviNowtilusVodApiPresentationPromotionContentListPromotionContentParentalControlList ParentalControlList
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
    public partial class RoviNowtilusVodApiPresentationPromotionContentListPromotionContentParentalControlList
    {

        private RoviNowtilusVodApiPresentationPromotionContentListPromotionContentParentalControlListParentalControl parentalControlField;

        /// <remarks/>
        public RoviNowtilusVodApiPresentationPromotionContentListPromotionContentParentalControlListParentalControl ParentalControl
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
    public partial class RoviNowtilusVodApiPresentationPromotionContentListPromotionContentParentalControlListParentalControl
    {

        private string parentalControlSystemField;

        private string parentalControlIdField;

        private object parentalControlAgeField;

        private RoviNowtilusVodApiPresentationPromotionContentListPromotionContentParentalControlListParentalControlParentalControlDisplay parentalControlDisplayField;

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
        public object ParentalControlAge
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
        public RoviNowtilusVodApiPresentationPromotionContentListPromotionContentParentalControlListParentalControlParentalControlDisplay ParentalControlDisplay
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
    public partial class RoviNowtilusVodApiPresentationPromotionContentListPromotionContentParentalControlListParentalControlParentalControlDisplay
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
}
