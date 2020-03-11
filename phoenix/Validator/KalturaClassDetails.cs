using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;

namespace Validator
{
    public class KalturaControllerDetails
    {
        public Type ServiceType { get; set; }
        public string ServiceId { get; set; }
        public List<KalturaActionDetails> Actions { get; set; }
        public bool IsCrudController { get; set; }
        public bool IsAbstract { get; set; }

        public KalturaControllerDetails()
        {
            Actions = new List<KalturaActionDetails>();
        }
    }

    public class KalturaActionDetails
    {
        public bool IsGenericMethod { get; set; }
        public string LoweredName { get; set; }
        public string RealName { get; set; }
        public string Description { get; set; }
        public bool IsDeprecated { get; set; }
        public bool IsSessionRequired { get; set; }
        public List<KalturaPrameterDetails> Prameters { get; set; }
        public Dictionary<string, string> ReturnedTypes { get; set; }
        public List<StatusCode> ApiThrows { get; set; }
        public List<eResponseStatus> ClientThrows { get; set; }

        public KalturaActionDetails()
        {
            Prameters = new List<KalturaPrameterDetails>();
            ReturnedTypes = new Dictionary<string, string>();
            ApiThrows = new List<StatusCode>();
            ClientThrows = new List<eResponseStatus>();
        }
    }

    public class KalturaPrameterDetails
    {
        public Type ParameterType { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> ParameterTypes { get; set; }
        public bool IsOptional { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        public int Position { get; set; }

        public KalturaPrameterDetails()
        {
            ParameterTypes = new Dictionary<string, string>();
        }
    }

    public class KalturaClassDetails
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string BaseName { get; set; }
        public bool IsAbstract { get; set; }
        public List<KalturaPropertyDetails> Properties { get; set; }

        public KalturaClassDetails()
        {
            Properties = new List<KalturaPropertyDetails>();
        }
    }

    public class KalturaPropertyDetails
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ObsoleteAttribute Obsolete { get; set; }
        public JsonIgnoreAttribute JsonIgnore{ get; set; }
        public DeprecatedAttribute Deprecated { get; set; }
        public DataMemberAttribute DataMember { get; set; }
        public SchemePropertyAttribute SchemeProperty { get; set; }
        public Type PropertyType { get; set; }


        public bool IsObsolete { get { return Obsolete != null; } }
    }
}
