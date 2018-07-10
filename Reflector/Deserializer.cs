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
using WebAPI.App_Start;
using System.Collections;

namespace Reflector
{
    class Deserializer : Base
    {
        public Deserializer() : base("..\\..\\..\\WebAPI\\Reflection\\KalturaJsonDeserializer.cs")
        {
            types.Remove(typeof(KalturaOTTObject));
            types.Remove(typeof(KalturaApiExceptionArg));
            types.Remove(typeof(KalturaFilter<>));
            types.Remove(typeof(KalturaGenericListResponse<>));
        }
        
        protected override void wrtieHeader()
        {
            file.WriteLine("// NOTICE: This is a generated file, to modify it, edit Program.cs in Reflector project");
            file.WriteLine("using Newtonsoft.Json.Linq;");
            file.WriteLine("using System;");
            file.WriteLine("using System.Web;");
            file.WriteLine("using System.Collections;");
            file.WriteLine("using System.Collections.Generic;");
            file.WriteLine("using WebAPI.Managers.Scheme;");
            file.WriteLine("using WebAPI.Filters;");
            file.WriteLine("using WebAPI.Reflection;");
        }

        protected override void wrtieBody()
        {
            wrtieUsing();
            wrtieDeserializer();
            wrtiePartialClasses();
        }

        protected override void wrtieFooter()
        {
        }
        
        private void writeDeserializeTypeProperty(string apiName, PropertyInfo property)
        {
            Type propertyType = property.PropertyType;

            file.WriteLine("                {");

            SchemePropertyAttribute schemaProperty = property.GetCustomAttribute<SchemePropertyAttribute>();
            if (schemaProperty != null)
            {
                file.WriteLine("                    if(!isOldVersion)");
                file.WriteLine("                    {");
                file.WriteLine("                        " + property.Name + "SchemaProperty.Validate(\"" + apiName + "\", parameters[\"" + apiName + "\"]);");
                file.WriteLine("                    }");
            }

            if(propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = Nullable.GetUnderlyingType(propertyType); ;
            }
            if (propertyType.IsPrimitive || propertyType == typeof(string))
            {
                file.WriteLine("                    " + property.Name + " = (" + propertyType.Name + ") Convert.ChangeType(parameters[\"" + apiName + "\"], typeof(" + propertyType.Name + "));");
            }
            else if (propertyType.IsEnum)
            {
                file.WriteLine("                    " + property.Name + " = (" + propertyType.Name + ") Enum.Parse(typeof(" + propertyType.Name + "), parameters[\"" + apiName + "\"].ToString(), true);");
            }
            else if (propertyType == typeof(DateTime))
            {
                file.WriteLine("                    " + property.Name + " = longToDateTime((long) parameters[\"" + apiName + "\"]);");
            }
            else if (propertyType.IsArray)
            {
                throw new Exception("shouldn'y happen");
            }
            else if (propertyType.IsGenericType) // nullable, list or map
            {
                if (typeof(IList).IsAssignableFrom(propertyType)) // list
                {
                    Type genericParam = propertyType.GetGenericArguments()[0];
                    string genericParamName = GetTypeName(genericParam);
                    file.WriteLine("                    if (parameters[\"" + apiName + "\"] is JArray)");
                    file.WriteLine("                    {");
                    if (typeof(IKalturaOTTObject).IsAssignableFrom(genericParam))
                    {
                        file.WriteLine("                        " + property.Name + " = buildList<" + genericParamName + ">(typeof(" + genericParamName + "), (JArray) parameters[\"" + apiName + "\"]);");
                    }
                    else
                    {
                        file.WriteLine("                        " + property.Name + " = buildNativeList<" + genericParamName + ">(typeof(" + genericParamName + "), (JArray) parameters[\"" + apiName + "\"]);");
                    }
                    file.WriteLine("                    }");
                }
                else if (typeof(IDictionary).IsAssignableFrom(propertyType)) // map
                {
                    string genericParamName = GetTypeName(propertyType.GetGenericArguments()[1]);
                    file.WriteLine("                    if (parameters[\"" + apiName + "\"] is JObject)");
                    file.WriteLine("                    {");
                    file.WriteLine("                        " + property.Name + " = buildDictionary<" + genericParamName + ">(typeof(" + genericParamName + "), ((JObject) parameters[\"" + apiName + "\"]).ToObject<Dictionary<string, object>>());");
                    file.WriteLine("                    }");
                }
            }
            else // object
            {
                file.WriteLine("                    if (parameters[\"" + apiName + "\"] is JObject)");
                file.WriteLine("                    {");
                file.WriteLine("                        " + property.Name + " = (" + propertyType.Name + ") Deserializer.deserialize(typeof(" + propertyType.Name + "), ((JObject) parameters[\"" + apiName + "\"]).ToObject<Dictionary<string, object>>());");
                file.WriteLine("                    }");
                file.WriteLine("                    else if (parameters[\"" + apiName + "\"] is IDictionary)");
                file.WriteLine("                    {");
                file.WriteLine("                        " + property.Name + " = (" + propertyType.Name + ") Deserializer.deserialize(typeof(" + propertyType.Name + "), (Dictionary<string, object>) parameters[\"" + apiName + "\"]);");
                file.WriteLine("                    }");
            }

            file.WriteLine("                }");
        }

        private void wrtieDeserializeTypeSchemaProperties(Type type)
        {
            List<PropertyInfo> schemePropertyProperties = typeof(SchemePropertyAttribute).GetProperties().ToList();

            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (!property.CanWrite || property.DeclaringType != type)
                {
                    continue;
                }

                SchemePropertyAttribute schemaProperty = property.GetCustomAttribute<SchemePropertyAttribute>();
                if (schemaProperty == null)
                {
                    continue;
                }

                file.WriteLine("        private static RuntimeSchemePropertyAttribute " + property.Name + "SchemaProperty = new RuntimeSchemePropertyAttribute(\"" + type.Name + "\")");
                file.WriteLine("        {");
                schemePropertyProperties.ForEach(schemePropertyProperty => {
                    object val = schemePropertyProperty.GetValue(schemaProperty);
                    if (val != null)
                    {
                        if (schemePropertyProperty.Name != "Name" && schemePropertyProperty.Name != "TypeId")
                        {
                            if (schemePropertyProperty.PropertyType == typeof(bool))
                            {
                                file.WriteLine("            " + schemePropertyProperty.Name + " = " + val.ToString().ToLower() + ",");
                            }
                            else if (schemePropertyProperty.PropertyType == typeof(int))
                            {
                                if ((int)val != int.MinValue && (int)val != int.MaxValue)
                                {
                                    file.WriteLine("            " + schemePropertyProperty.Name + " = " + val.ToString().ToLower() + ",");
                                }
                            }
                            else if (schemePropertyProperty.PropertyType == typeof(long))
                            {
                                if ((long)val != long.MinValue && (long)val != long.MaxValue)
                                {
                                    file.WriteLine("            " + schemePropertyProperty.Name + " = " + val.ToString().ToLower() + ",");
                                }
                            }
                            else if (schemePropertyProperty.PropertyType == typeof(float))
                            {
                                if ((float)val != float.MinValue && (float)val != float.MaxValue)
                                {
                                    file.WriteLine("            " + schemePropertyProperty.Name + " = " + val.ToString().ToLower() + ",");
                                }
                            }
                            else if (schemePropertyProperty.PropertyType == typeof(Type))
                            {
                                file.WriteLine("            " + schemePropertyProperty.Name + " = typeof(" + ((Type)val).Name + "),");
                            }
                            else
                            {
                                file.WriteLine("            " + schemePropertyProperty.Name + " = " + val + ",");
                            }
                        }
                    }
                });

                file.WriteLine("        };");
            }
        }

        private void wrtieDeserializeTypeProperties(Type type)
        {
            PropertyInfo[] properties = type.GetProperties();
            SchemePropertyAttribute schemaProperty;
            IEnumerable<OldStandardPropertyAttribute> oldStandards;


            bool hasProperties = false;
            bool addIsOldVersion = false;
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite && property.DeclaringType == type)
                {
                    hasProperties = true;

                    schemaProperty = property.GetCustomAttribute<SchemePropertyAttribute>();
                    if (schemaProperty != null)
                    {
                        addIsOldVersion = true;
                        break;
                    }

                    oldStandards = property.GetCustomAttributes<OldStandardPropertyAttribute>();
                    foreach (OldStandardPropertyAttribute oldStandard in oldStandards)
                    {
                        addIsOldVersion = true;
                        break;
                    }
                }
            }

            if(!hasProperties)
            {
                return;
            }

            file.WriteLine("            if (parameters != null)");
            file.WriteLine("            {");

            if (addIsOldVersion)
            {
                file.WriteLine("                Version currentVersion = (Version)HttpContext.Current.Items[RequestParser.REQUEST_VERSION];");
                file.WriteLine("                bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);");
            }

            foreach (PropertyInfo property in properties)
            {
                if (!property.CanWrite || property.DeclaringType != type)
                {
                    continue;
                }

                string apiName = property.Name;
                DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>();                
                if (dataMember != null)
                {
                    apiName = dataMember.Name;
                }
                if (typeof(KalturaMultilingualString).IsAssignableFrom(property.PropertyType))
                {
                    apiName = KalturaMultilingualString.GetMultilingualName(apiName);
                }
                
                file.WriteLine("                if (parameters.ContainsKey(\"" + apiName + "\") && parameters[\"" + apiName + "\"] != null)");
                writeDeserializeTypeProperty(apiName, property);
                oldStandards = property.GetCustomAttributes<OldStandardPropertyAttribute>();
                foreach (OldStandardPropertyAttribute oldStandard in oldStandards)
                {
                    apiName = oldStandard.oldName;
                    if (typeof(KalturaMultilingualString).IsAssignableFrom(property.PropertyType))
                    {
                        apiName = KalturaMultilingualString.GetMultilingualName(apiName);
                    }

                    if (oldStandard.sinceVersion == null)
                    {
                        file.WriteLine("                if (parameters.ContainsKey(\"" + apiName + "\") && parameters[\"" + apiName + "\"] != null && isOldVersion)");
                    }
                    else
                    {
                        file.WriteLine("                if (parameters.ContainsKey(\"" + apiName + "\") && parameters[\"" + apiName + "\"] != null && (isOldVersion || currentVersion.CompareTo(new Version(\"" + oldStandard.sinceVersion + "\")) > 0))");
                    }
                    writeDeserializeTypeProperty(apiName, property);
                }
            }
            file.WriteLine("            }");
        }

        private void wrtiePartialClass(Type type)
        {
            file.WriteLine("    public partial class " + GetTypeName(type, true));
            file.WriteLine("    {");
            wrtieDeserializeTypeSchemaProperties(type);
            file.WriteLine("        public " + GetTypeName(type) + "(Dictionary<string, object> parameters = null) : base(parameters)");
            file.WriteLine("        {");
            wrtieDeserializeTypeProperties(type);
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

        private void wrtieDeserializer()
        {
            file.WriteLine("");
            file.WriteLine("namespace WebAPI.Reflection");
            file.WriteLine("{");
            file.WriteLine("    public class Deserializer");
            file.WriteLine("    {");
            file.WriteLine("        public static KalturaOTTObject deserialize(Type type, Dictionary<string, object> parameters)");
            file.WriteLine("        {");
            file.WriteLine("            string objectType = type.Name;");
            file.WriteLine("            if (parameters.ContainsKey(\"objectType\"))");
            file.WriteLine("            {");
            file.WriteLine("                objectType = parameters[\"objectType\"].ToString();");
            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            switch (objectType)");
            file.WriteLine("            {");

            foreach (Type type in types)
            {
                file.WriteLine("                case \"" + GetTypeName(type) + "\":");
                NewObjectTypeAttribute newObjectTypeAttribue = type.GetCustomAttribute<NewObjectTypeAttribute>(false);
                if (newObjectTypeAttribue != null)
                {
                    file.WriteLine("                    return new " + GetTypeName(newObjectTypeAttribue.type) + "(parameters);");
                }
                else if (type.IsAbstract)
                {
                    file.WriteLine("                    throw new RequestParserException(RequestParserException.ABSTRACT_PARAMETER, objectType);");
                }
                else
                {
                    file.WriteLine("                    return new " + GetTypeName(type) + "(parameters);");
                }
                file.WriteLine("                    ");
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            throw new RequestParserException(RequestParserException.INVALID_OBJECT_TYPE, objectType);");
            file.WriteLine("        }");
            file.WriteLine("    }");
            file.WriteLine("}");
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
