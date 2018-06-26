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
    enum PropertyType
    {
        NATIVE,
        STRING,
        OBJECT,
        GENERIC_OBJECT,
        ARRAY,
        NUMERIC_ARRAY,
        MAP,
        ENUM,
        BOOLEAN,
        CUSTOM
    }

    class Serializer : Base
    {
        public Serializer() : base("..\\..\\..\\WebAPI\\Reflection\\KalturaJsonSerializer.cs", typeof(IKalturaJsonable))
        {
            types.Remove(typeof(KalturaJsonable));
            types.Remove(typeof(KalturaMultilingualString));
        }
        
        protected override void wrtieHeader()
        {
            file.WriteLine("// NOTICE: This is a generated file, to modify it, edit Program.cs in Reflector project");
            file.WriteLine("using System;");
            file.WriteLine("using System.Linq;");
            file.WriteLine("using System.Web;");
            file.WriteLine("using WebAPI.Managers.Scheme;");
            file.WriteLine("using System.Collections.Generic;");
        }

        protected override void wrtieBody()
        {
            wrtieUsing();
            wrtiePartialClasses();
        }

        protected override void wrtieFooter()
        {
        }

        private bool IsObsolete(PropertyInfo property)
        {
            return (property.GetCustomAttribute<ObsoleteAttribute>() != null);
        }

        private string DeprecationVersion(PropertyInfo property)
        {
            DeprecatedAttribute deprecated = property.GetCustomAttribute<DeprecatedAttribute>();
            if (deprecated != null)
            {
                return deprecated.SinceVersion;
            }
            return null;
        }

        private void wrtieSerializeTypeProperties(Type type)
        {
            if (type.BaseType != null && type != typeof(KalturaOTTObject))
            {
                wrtieSerializeTypeProperties(type.BaseType);
            }

            List<PropertyInfo> properties = type.GetProperties().ToList();
            properties.Sort(new PropertyInfoComparer());
            foreach (PropertyInfo property in properties)
            {
                if (property.DeclaringType != type)
                    continue;

                DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>(false);
                string propertyName = property.Name;
                bool obsoleteHandled = !IsObsolete(property);
                string isObsolete = !obsoleteHandled ? "!omitObsolete && " : "";

                string deprecationVersion = DeprecationVersion(property);
                bool deprecationHandled = deprecationVersion == null;
                string isDeprecated = deprecationHandled ? "" : "!DeprecatedAttribute.IsDeprecated(\"" + deprecationVersion + "\", currentVersion) && ";
                

                PropertyType propertyType = PropertyType.NATIVE;
                string tab = "";
                string genericType = "";

                if (typeof(IKalturaJsonable).IsAssignableFrom(property.PropertyType))
                {
                    propertyType = PropertyType.OBJECT;
                    if(property.PropertyType.GetMethod("ToCustomJson") != null)
                    {
                        propertyType = PropertyType.CUSTOM;
                    }
                }
                else if (property.PropertyType == typeof(object))
                {
                    propertyType = PropertyType.GENERIC_OBJECT;
                }
                else if (property.PropertyType == typeof(string))
                {
                    propertyType = PropertyType.STRING;
                }
                else if (property.PropertyType == typeof(bool))
                {
                    propertyType = PropertyType.BOOLEAN;
                }
                else if (property.PropertyType.IsGenericType)
                {
                    if (property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        Type InnerType = property.PropertyType.GetGenericArguments()[0];
                        if (InnerType.IsEnum)
                        {
                            propertyType = PropertyType.ENUM;
                        }
                        else if (InnerType == typeof(bool))
                        {
                            propertyType = PropertyType.BOOLEAN;
                        }
                        else
                        {
                            propertyType = PropertyType.NATIVE;
                        }
                        tab = "    ";
                        obsoleteHandled = true;
                        deprecationHandled = true;
                        file.WriteLine("            if(" + isObsolete + isDeprecated + propertyName + ".HasValue)");
                        file.WriteLine("            {");
                    }
                    else if (property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        propertyType = typeof(IKalturaOTTObject).IsAssignableFrom(property.PropertyType.GetGenericArguments()[0]) ? PropertyType.ARRAY : PropertyType.NUMERIC_ARRAY;
                        genericType = property.PropertyType.GetGenericArguments()[0].Name;
                        switch (genericType)
                        {
                            case "Int64":
                                genericType = "long";
                                break;

                            case "Int32":
                                genericType = "int";
                                break;
                        }
                        tab = "    ";
                        obsoleteHandled = true;
                        deprecationHandled = true;
                        file.WriteLine("            if(" + isObsolete + isDeprecated + propertyName + " != null && " + propertyName + ".Count > 0)");
                        file.WriteLine("            {");
                    }
                    else if (property.PropertyType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>))
                    {
                        propertyType = PropertyType.MAP;
                        genericType = property.PropertyType.GetGenericArguments()[1].Name;
                        tab = "    ";
                        obsoleteHandled = true;
                        deprecationHandled = true;
                        file.WriteLine("            if(" + isObsolete + isDeprecated + propertyName + " != null && " + propertyName + ".Count > 0)");
                        file.WriteLine("            {");
                    }
                }
                else if (property.PropertyType.IsEnum)
                {
                    propertyType = PropertyType.ENUM;
                }

                if(propertyType == PropertyType.STRING || propertyType == PropertyType.OBJECT || propertyType == PropertyType.GENERIC_OBJECT)
                {
                    tab = "    ";
                    obsoleteHandled = true;
                    deprecationHandled = true;
                    file.WriteLine("            if(" + isObsolete + isDeprecated + propertyName + " != null)");
                    file.WriteLine("            {");
                }

                if (deprecationHandled && !obsoleteHandled)
                {
                    tab = "    ";
                    file.WriteLine("            if(!omitObsolete)");
                    file.WriteLine("            {");
                }
                else if (!deprecationHandled && obsoleteHandled)
                {
                    tab = "    ";
                    file.WriteLine("            if(!DeprecatedAttribute.IsDeprecated(\"" + deprecationVersion + "\", currentVersion))");
                    file.WriteLine("            {");
                }
                else if (!deprecationHandled && !obsoleteHandled)
                {
                    tab = "    ";
                    file.WriteLine("            if(!omitObsolete && !DeprecatedAttribute.IsDeprecated(\"" + deprecationVersion + "\", currentVersion))");
                    file.WriteLine("            {");
                }

                string value = "propertyValue";
                switch (propertyType)
                {
                    case PropertyType.NATIVE:
                        value = propertyName;
                        break;

                    case PropertyType.ENUM:
                        value = propertyName + ".GetHashCode()";
                        break;

                    case PropertyType.BOOLEAN:
                        value = propertyName + ".ToString().ToLower()";
                        break;

                    case PropertyType.STRING:
                        value = "\"\\\"\" + " + propertyName + " + \"\\\"\"";
                        break;

                    case PropertyType.OBJECT:
                        file.WriteLine(tab + "            propertyValue = " + propertyName + ".ToJson(currentVersion, omitObsolete);");
                        break;

                    case PropertyType.GENERIC_OBJECT:
                        file.WriteLine(tab + "            propertyValue = (" + propertyName + " is IKalturaJsonable ? (" + propertyName + " as IKalturaJsonable).ToJson(currentVersion, omitObsolete) : JsonManager.GetInstance().Serialize(" + propertyName + "));");
                        break;

                    case PropertyType.ARRAY:
                        file.WriteLine(tab + "            propertyValue = \"[\" + String.Join(\", \", " + propertyName + ".Select(item => item.ToJson(currentVersion, omitObsolete))) + \"]\";");
                        break;

                    case PropertyType.MAP:
                        file.WriteLine(tab + "            propertyValue = \"{\" + String.Join(\", \", " + propertyName + ".Select(pair => \"\\\"\" + pair.Key + \"\\\": \" + pair.Value.ToJson(currentVersion, omitObsolete))) + \"}\";");
                        break;

                    case PropertyType.NUMERIC_ARRAY:
                        file.WriteLine(tab + "            propertyValue = \"[\" + String.Join(\", \", " + propertyName + ".Select(item => item.ToString())) + \"]\";");
                        break;
                }

                if (propertyType == PropertyType.CUSTOM)
                {
                    file.WriteLine(tab + "            ret.Add(" + propertyName + ".ToCustomJson(currentVersion, omitObsolete, \"" + dataMember.Name + "\"));");
                }
                else
                {
                    file.WriteLine(tab + "            ret.Add(\"\\\"" + dataMember.Name + "\\\": \" + " + value + ");");
                }

                IEnumerable<OldStandardPropertyAttribute> oldStandardProperties = property.GetCustomAttributes<OldStandardPropertyAttribute>(false);
                if (oldStandardProperties != null && oldStandardProperties.Count() > 0)
                {
                    foreach (OldStandardPropertyAttribute oldStandardProperty in oldStandardProperties)
                    {
                        if (oldStandardProperty.sinceVersion != null)
                        {
                            file.WriteLine(tab + "            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0 || currentVersion.CompareTo(new Version(\"" + oldStandardProperty.sinceVersion + "\")) > 0)");
                        }
                        else
                        {
                            file.WriteLine(tab + "            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)");
                        }
                        file.WriteLine(tab + "            {");
                        if (propertyType == PropertyType.CUSTOM)
                        {
                            file.WriteLine(tab + "            ret.Add(" + propertyName + ".ToCustomJson(currentVersion, omitObsolete, \"" + oldStandardProperty.oldName + "\"));");
                        }
                        else
                        {
                            file.WriteLine(tab + "                ret.Add(\"\\\"" + oldStandardProperty.oldName + "\\\": \" + " + value + ");");
                        }
                        file.WriteLine(tab + "            }");
                    }
                }

                if (tab.Length > 0)
                {
                    file.WriteLine("            }");
                }
            }
        }
    
        private string getTypeName(Type type)
        {
            if(type.IsGenericType)
            {
                Regex regex = new Regex("^[^`]+");
                Match match = regex.Match(type.Name);
                return match.Value + "<T>";

            }

            return type.Name;
        }

        private void wrtiePartialClass(Type type)
        {
            file.WriteLine("    public partial class " + getTypeName(type));
            file.WriteLine("    {");
            file.WriteLine("        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)");
            file.WriteLine("        {");
            file.WriteLine("            List<string> ret = new List<string>();");
            file.WriteLine("            string propertyValue;");
            wrtieSerializeTypeProperties(type);
            file.WriteLine("            return String.Join(\", \", ret);");
            file.WriteLine("        }");
            file.WriteLine("    }");
        }

        private void wrtieNamespace(string namespaceName)
        {
            file.WriteLine("");
            file.WriteLine("namespace " + namespaceName);
            file.WriteLine("{");

            foreach (Type type in types)
            {
                if (type.Namespace == namespaceName)
                {
                    wrtiePartialClass(type);
                }
            }

            file.WriteLine("}");
        }

        private void wrtieUsing()
        {
            HashSet<string> namespaces = new HashSet<string>();
            foreach (Type type in types)
            {
                if (!namespaces.Contains(type.Namespace))
                {
                    namespaces.Add(type.Namespace);
                    file.WriteLine("using " + type.Namespace + ";");
                }
            }
        }

        private void wrtiePartialClasses()
        {
            HashSet<string> namespaces = new HashSet<string>();
            foreach (Type type in types)
            {
                if (!namespaces.Contains(type.Namespace))
                {
                    namespaces.Add(type.Namespace);
                    wrtieNamespace(type.Namespace);
                }
            }
        }
    }
}
