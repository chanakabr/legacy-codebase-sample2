using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Controllers;
using System.Text.RegularExpressions;

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
        protected List<Type> enums;
        protected List<Type> controllers;
        protected StreamWriter file;

        public Base(string path, Type baseClass = null)
        {
            if(baseClass == null)
            {
                baseClass = typeof(KalturaOTTObject);
            }

            assembly = Assembly.Load("WebAPI");
            var typeComparer = new TypeComparer();
            var allTypes = assembly.GetTypes();

            types = allTypes.Where(myType => myType.IsClass && baseClass.IsAssignableFrom(myType)).ToList();
            types.Sort(typeComparer);
            
            enums = types
                .SelectMany(GetUnderlyingEnumTypes)
                .Distinct()
                .ToList();
            enums.Sort(typeComparer);

            controllers = allTypes.Where(myType => myType.IsClass && typeof(IKalturaController).IsAssignableFrom(myType)).ToList();
            controllers.Sort(typeComparer);

            file = new StreamWriter(path);
        }

        public void write()
        {
            writeHeader();
            writeBody();
            writeFooter();

            file.Close();
        }

        protected abstract void writeHeader();
        protected abstract void writeBody();
        protected abstract void writeFooter();

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
            if (type == typeof(double))
                return "double";
            if (type == typeof(float))
                return "float";
            if (type == typeof(bool))
                return "bool";
            if (type.IsEnum)
                return type.Name;

            var regex = new Regex("^[^`]+");
            var match = regex.Match(type.Name);

            if (type.IsGenericType && addGenericDefinition)
            {
                return match.Value + "<" + String.Join(", ", type.GetGenericArguments().Select(t => GetTypeName(t))) + ">";
            }

            return match.Value;
        }

        private IEnumerable<Type> GetUnderlyingEnumTypes(Type type)
        {
            var enumTypes = new List<Type>();
            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.PropertyType.IsEnum)
                {
                    enumTypes.Add(propertyInfo.PropertyType);
                }

                var underlyingType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
                if (underlyingType?.IsEnum == true)
                {
                    enumTypes.Add(underlyingType);
                }
            }

            return enumTypes;
        }
    }
}
