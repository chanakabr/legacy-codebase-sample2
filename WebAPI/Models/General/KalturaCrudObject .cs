using ApiLogic.Base;
using ApiObjects.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Reflection;

namespace WebAPI.Models.General
{
    // TODO SHIR - CRUD changes
    /// <summary>
    /// Base class that have CRUD actions 
    /// </summary>
    public abstract partial class KalturaCrudObject<CoreT, IdentifierT> : KalturaOTTObject 
        where CoreT : ICrudHandeledObject
        where IdentifierT : IConvertible
    {
        internal abstract ICrudHandler<CoreT, IdentifierT> Handler { get; } //  BaseCrudHandler<T>
        internal abstract void ValidateForAdd();
        internal abstract void ValidateForUpdate();
    }
}