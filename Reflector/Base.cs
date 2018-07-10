using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Controllers;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using WebAPI.Models.Renderers;

namespace Reflector
{
    class TypeComparer : IComparer<Type>
    {
        public int Compare(Type a, Type b)
        {
            return a.Name.CompareTo(b.Name);
        }
    }

    class MethodInfoComparer : IComparer<MethodInfo>
    {
        public int Compare(MethodInfo a, MethodInfo b)
        {
            return a.Name.CompareTo(b.Name);
        }
    }

    class PropertyInfoComparer : IComparer<PropertyInfo>
    {
        public int Compare(PropertyInfo a, PropertyInfo b)
        {
            return a.Name.CompareTo(b.Name);
        }
    }

    class OldStandardArgumentAttributeComparer : IComparer<OldStandardVersionedAttribute>
    {
        public int Compare(OldStandardVersionedAttribute a, OldStandardVersionedAttribute b)
        {
            return a.Compare(b);
        }
    }

    abstract class Base
    {
        private Assembly assembly;
        protected List<Type> types;
        protected List<Type> controllers;
        protected StreamWriter file;

        public Base(string path, Type baseClass = null)
        {
            if(baseClass == null)
            {
                baseClass = typeof(KalturaOTTObject);
            }

            assembly = Assembly.Load("WebAPI");
            types = assembly.GetTypes().Where(myType => myType.IsClass && baseClass.IsAssignableFrom(myType)).ToList();
            types.Sort(new TypeComparer());

            controllers = assembly.GetTypes().Where(myType => myType.IsClass && typeof(IKalturaController).IsAssignableFrom(myType)).ToList();
            controllers.Sort(new TypeComparer());

            file = new StreamWriter(path);
        }

        public void wrtie()
        {
            wrtieHeader();
            wrtieBody();
            wrtieFooter();

            file.Close();
        }

        protected abstract void wrtieHeader();
        protected abstract void wrtieBody();
        protected abstract void wrtieFooter();

        protected string GetTypeName(Type type, bool addGenericDefinition = false)
        {
            if (type == typeof(String))
                return "string";
            if (type == typeof(DateTime))
                return "int";
            if (type == typeof(long) || type == typeof(Int64))
                return "long";
            if (type == typeof(Int32))
                return "int";
            if (type == typeof(double) || type == typeof(float))
                return "float";
            if (type == typeof(bool))
                return "bool";
            if (type.IsEnum)
                return type.Name;

            Regex regex = new Regex("^[^`]+");
            Match match = regex.Match(type.Name);

            if (type.IsGenericType && addGenericDefinition)
            {
                return match.Value + "<" + String.Join(", ", type.GetGenericArguments().Select(t => GetTypeName(t))) + ">";
            }

            return match.Value;
        }
    }
}
