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
    public abstract partial class KalturaCrudObject<CoreT> : KalturaOTTObject where CoreT : ICrudHandeledObject
    {
        public abstract ICrudHandler<CoreT> Handler { get; } //  BaseCrudHandler<T>
        public abstract void ValidateForAdd();
        public abstract void ValidateForUpdate();
    }
}