using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Assets
{
    [Serializable]
    [SchemeClass(Required = new string[] { "ksql" })]
    public partial class KalturaFilterAssetByKsqlAction : KalturaFilterAction
    {
        /// <summary>
        /// ksql to filter assets by
        /// </summary>
        [DataMember(Name = "ksql")]
        [JsonProperty("ksql")]
        [XmlElement(ElementName = "ksql")]
        [SchemeProperty(Pattern = @"^((?!entitled_assets).)*$", MinLength = 1)]
        public string Ksql { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterAssetByKsql;
        }

        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(Ksql))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "ksql");
            }
        }
    }
}
