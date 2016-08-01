using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Linq;
using WebAPI.Models.General;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Models;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web;
using System.Xml.Serialization;

namespace Validator.Managers.Scheme
{
    internal class Field
    {
        private string _name;
        private List<string> _dependsOn;

        public Field(Type type)
        {
            _name = type.Name;
            _dependsOn = new List<string>();

            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType.IsGenericType)
                {
                    foreach (var genericArgument in property.PropertyType.GetGenericArguments())
                    {
                        AddDependency(genericArgument);
                    }
                }
                else if (property.PropertyType.IsArray)
                {
                    var arrType = property.PropertyType.GetElementType();
                    AddDependency(arrType);
                }
                else if (property.PropertyType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    AddDependency(property.PropertyType);
                }
            }

            if (type.BaseType != null)
                AddDependency(type.BaseType);
        }

        private void AddDependency(Type type)
        {
            if (type.Name == _name)
                return;

            if (type.IsGenericType)
            {
                foreach (var genericArgument in type.GetGenericArguments())
                {
                    AddDependency(genericArgument);
                }
            }
            else if (type.IsArray)
            {
                var arrType = type.GetElementType();
                AddDependency(arrType);
            }
            else if (type.IsSubclassOf(typeof(KalturaOTTObject)))
            {
                _dependsOn.Add(type.Name);
            }
        }

        public string Name 
        {
            get
            {
                return _name;
            }
        }

        public List<string> DependsOn 
        { 
            get{
                return _dependsOn;
            }
        }
    }
}