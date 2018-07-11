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
    enum SerializeType
    {
        JSON,
        XML
    }

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
        public Serializer() : base("..\\..\\..\\WebAPI\\Reflection\\KalturaJsonSerializer.cs", typeof(IKalturaSerializable))
        {
            types.Remove(typeof(KalturaOTTObject));
            types.Remove(typeof(KalturaSerializable));
            types.Remove(typeof(KalturaMultilingualString));
        }
        
        protected override void wrtieHeader()
        {
            file.WriteLine("// NOTICE: This is a generated file, to modify it, edit Program.cs in Reflector project");
            file.WriteLine("using System;");
            file.WriteLine("using System.Linq;");
            file.WriteLine("using System.Collections.Generic;");
            file.WriteLine("using WebAPI.Managers.Scheme;");
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

        private void wrtieSerializeTypeProperties(Type type, SerializeType serializeType)
        {
            List<PropertyInfo> properties = type.GetProperties().ToList();
            properties.Sort(new PropertyInfoComparer());
            foreach (PropertyInfo property in properties)
            {
                if (property.DeclaringType != type)
                    continue;

                DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>(false);
                string propertyName = property.Name;
                List<string> conditions = new List<string>();

                OnlyNewStandardAttribute onlyNewStandard = property.GetCustomAttribute<OnlyNewStandardAttribute>(false);
                if (onlyNewStandard != null)
                {
                    conditions.Add("!isOldVersion");
                }

                if (IsObsolete(property))
                {
                    conditions.Add("!omitObsolete");
                }

                string deprecationVersion = DeprecationVersion(property);
                if(deprecationVersion != null)
                {
                    conditions.Add("!DeprecatedAttribute.IsDeprecated(\"" + deprecationVersion + "\", currentVersion)");
                }                

                PropertyType propertyType = PropertyType.NATIVE;
                string tab = "";
                string realType = property.PropertyType.Name;

                if (typeof(IKalturaSerializable).IsAssignableFrom(property.PropertyType))
                {
                    propertyType = PropertyType.OBJECT;
                    if((serializeType == SerializeType.JSON && property.PropertyType.GetMethod("ToCustomJson") != null) || (serializeType == SerializeType.XML && property.PropertyType.GetMethod("ToCustomXml") != null))
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
                        realType = InnerType.Name;
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
                        conditions.Add(propertyName + ".HasValue");
                    }
                    else if (property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        propertyType = typeof(IKalturaOTTObject).IsAssignableFrom(property.PropertyType.GetGenericArguments()[0]) ? PropertyType.ARRAY : PropertyType.NUMERIC_ARRAY;
                        realType = property.PropertyType.GetGenericArguments()[0].Name;
                        switch (realType)
                        {
                            case "Int64":
                                realType = "long";
                                break;

                            case "Int32":
                                realType = "int";
                                break;
                        }
                        conditions.Add(propertyName + " != null");
                    }
                    else if (property.PropertyType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>))
                    {
                        propertyType = PropertyType.MAP;
                        realType = property.PropertyType.GetGenericArguments()[1].Name;
                        conditions.Add(propertyName + " != null");
                    }
                }
                else if (property.PropertyType.IsEnum)
                {
                    propertyType = PropertyType.ENUM;
                }

                if (propertyType == PropertyType.STRING || propertyType == PropertyType.OBJECT || propertyType == PropertyType.GENERIC_OBJECT)
                {
                    conditions.Add(propertyName + " != null");
                }

                if(conditions.Count > 0)
                {
                    tab = "    ";
                    file.WriteLine("            if(" + String.Join(" && ", conditions) + ")");
                    file.WriteLine("            {");
                }

                string value = "propertyValue";
                switch (propertyType)
                {
                    case PropertyType.NATIVE:
                        value = propertyName;
                        break;

                    case PropertyType.ENUM:
                        value = "\"\\\"\" + Enum.GetName(typeof(" + realType + "), " + propertyName + ") + \"\\\"\"";
                        break;

                    case PropertyType.BOOLEAN:
                        value = propertyName + ".ToString().ToLower()";
                        break;

                    case PropertyType.STRING:
                        if (serializeType == SerializeType.JSON)
                        {
                            value = "\"\\\"\" + EscapeJson(" + propertyName + ") + \"\\\"\"";
                        }
                        else
                        {
                            value = "EscapeXml(" + propertyName + ")";
                        }
                        break;

                    case PropertyType.OBJECT:
                        if (serializeType == SerializeType.JSON)
                        {
                            file.WriteLine(tab + "            propertyValue = " + propertyName + ".ToJson(currentVersion, omitObsolete);");
                        }
                        else
                        {
                            file.WriteLine(tab + "            propertyValue = " + propertyName + ".PropertiesToXml(currentVersion, omitObsolete);");
                        }
                        break;

                    case PropertyType.GENERIC_OBJECT:
                        if (serializeType == SerializeType.JSON)
                        {
                            file.WriteLine(tab + "            propertyValue = (" + propertyName + " is IKalturaSerializable ? (" + propertyName + " as IKalturaSerializable).ToJson(currentVersion, omitObsolete) : JsonManager.GetInstance().Serialize(" + propertyName + "));");
                        }
                        else
                        {
                            file.WriteLine(tab + "            propertyValue = (" + propertyName + " is IKalturaSerializable ? (" + propertyName + " as IKalturaSerializable).PropertiesToXml(currentVersion, omitObsolete) : " + propertyName + ".ToString());");
                        }
                        break;

                    case PropertyType.ARRAY:
                        if (serializeType == SerializeType.JSON)
                        {
                            file.WriteLine(tab + "            propertyValue = \"[\" + String.Join(\", \", " + propertyName + ".Select(item => item.ToJson(currentVersion, omitObsolete))) + \"]\";");
                        }
                        else
                        {
                            file.WriteLine(tab + "            propertyValue = \"<item>\" + String.Join(\"</item><item>\", " + propertyName + ".Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + \"</item>\";");
                        }
                        break;

                    case PropertyType.MAP:
                        if (serializeType == SerializeType.JSON)
                        {
                            file.WriteLine(tab + "            propertyValue = \"{\" + String.Join(\", \", " + propertyName + ".Select(pair => \"\\\"\" + pair.Key + \"\\\": \" + pair.Value.ToJson(currentVersion, omitObsolete))) + \"}\";");
                        }
                        else
                        {
                            file.WriteLine(tab + "            propertyValue = \"<item>\" + String.Join(\"</item><item>\", " + propertyName + ".Select(pair => \"<itemKey>\" + pair.Key + \"</itemKey>\" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + \"</item>\";");
                        }
                        break;

                    case PropertyType.NUMERIC_ARRAY:
                        if (serializeType == SerializeType.JSON)
                        {
                            file.WriteLine(tab + "            propertyValue = \"[\" + String.Join(\", \", " + propertyName + ".Select(item => item.ToString())) + \"]\";");
                        }
                        else
                        {
                            file.WriteLine(tab + "            propertyValue = \"<item>\" + String.Join(\"</item><item>\", " + propertyName + ".Select(item => item.ToString())) + \"</item>\";");
                        }
                        break;
                }

                if (serializeType == SerializeType.JSON)
                {
                    if (propertyType == PropertyType.CUSTOM)
                    {
                        file.WriteLine(tab + "            ret.Add(" + propertyName + ".ToCustomJson(currentVersion, omitObsolete, \"" + dataMember.Name + "\"));");
                    }
                    else
                    {
                        file.WriteLine(tab + "            ret.Add(\"\\\"" + dataMember.Name + "\\\": \" + " + value + ");");
                    }
                }
                else
                {
                    if (propertyType == PropertyType.CUSTOM)
                    {
                        file.WriteLine(tab + "            ret += " + propertyName + ".ToCustomXml(currentVersion, omitObsolete, \"" + dataMember.Name + "\");");
                    }
                    else
                    {
                        file.WriteLine(tab + "            ret += \"<" + dataMember.Name + ">\" + " + value + " + \"</" + dataMember.Name + ">\";");
                    }
                }

                IEnumerable<OldStandardPropertyAttribute> oldStandardProperties = property.GetCustomAttributes<OldStandardPropertyAttribute>(false);
                if (oldStandardProperties != null && oldStandardProperties.Count() > 0)
                {
                    foreach (OldStandardPropertyAttribute oldStandardProperty in oldStandardProperties)
                    {
                        if (oldStandardProperty.sinceVersion != null)
                        {
                            file.WriteLine(tab + "            if (currentVersion == null || isOldVersion || currentVersion.CompareTo(new Version(\"" + oldStandardProperty.sinceVersion + "\")) > 0)");
                        }
                        else
                        {
                            file.WriteLine(tab + "            if (currentVersion == null || isOldVersion)");
                        }
                        file.WriteLine(tab + "            {");
                        if (serializeType == SerializeType.JSON)
                        {
                            if (propertyType == PropertyType.CUSTOM)
                            {
                                file.WriteLine(tab + "            ret.Add(" + propertyName + ".ToCustomJson(currentVersion, omitObsolete, \"" + oldStandardProperty.oldName + "\"));");
                            }
                            else
                            {
                                file.WriteLine(tab + "                ret.Add(\"\\\"" + oldStandardProperty.oldName + "\\\": \" + " + value + ");");
                            }
                        }
                        else
                        {
                            if (propertyType == PropertyType.CUSTOM)
                            {
                                file.WriteLine(tab + "            ret+ = " + propertyName + ".ToCustomXml(currentVersion, omitObsolete, \"" + oldStandardProperty.oldName + "\");");
                            }
                            else
                            {
                                file.WriteLine(tab + "            ret += \"<" + oldStandardProperty.oldName + ">\" + " + value + " + \"</" + oldStandardProperty.oldName + ">\";");
                            }
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
    
        private void wrtiePartialClass(Type type)
        {
            file.WriteLine("    public partial class " + GetTypeName(type, true));
            file.WriteLine("    {");
            file.WriteLine("        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)");
            file.WriteLine("        {");
            file.WriteLine("            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);");
            file.WriteLine("            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);");
            file.WriteLine("            string propertyValue;");
            wrtieSerializeTypeProperties(type, SerializeType.JSON);
            file.WriteLine("            return ret;");
            file.WriteLine("        }");
            file.WriteLine("        ");
            file.WriteLine("        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)");
            file.WriteLine("        {");
            file.WriteLine("            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);");
            file.WriteLine("            string ret = base.PropertiesToXml(currentVersion, omitObsolete);");
            file.WriteLine("            string propertyValue;");
            wrtieSerializeTypeProperties(type, SerializeType.XML);
            file.WriteLine("            return ret;");
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
