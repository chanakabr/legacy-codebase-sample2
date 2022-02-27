using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using WebAPI.Controllers;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Renderers;

namespace Validator.Managers.Scheme
{
    public class SchemeManager
    {
        public static void Generate(Stream stream)
        {
            Scheme writer = Scheme.getInstance();
            writer.RemoveInvalidObjects();
            writer.write(stream);
        }

        public static string getServiceId(Type controller)
        {
            ServiceAttribute service = controller.GetCustomAttribute<ServiceAttribute>(false);
            return service.Name;
        }

        public static bool IsCrudController(Type controller, out Dictionary<string, CrudActionAttribute> crudActionAttributes, out Dictionary<string, MethodInfo> crudActions)
        {
            crudActionAttributes = null;
            crudActions = null;
            if (controller.BaseType != null && controller.BaseType.IsGenericType && controller.BaseType.GetGenericTypeDefinition() == typeof(KalturaCrudController<,,,,>))
            {
                var actionAttributes = controller.GetCustomAttributes<CrudActionAttribute>(true).ToDictionary(x => x.GetName(), x => x);

                if (actionAttributes != null && actionAttributes.Count > 0)
                {
                    crudActionAttributes = actionAttributes;
                    crudActions = controller.BaseType.GetMethods().ToDictionary(x => x.Name.ToLower(), x => x);
                    return true;
                }
            }

            return false;
        }

        public static bool? IsParameterOptional(ParameterInfo parameter, Dictionary<string, bool> optionalParameters)
        {
            bool? isParamOptional = null;
            if (optionalParameters == null)
            {
                isParamOptional = parameter.IsOptional;
            }
            else if (optionalParameters.ContainsKey(parameter.Name))
            {
                isParamOptional = optionalParameters[parameter.Name];
            }
            
            return isParamOptional;
        }

        public static string VarToString(object value)
        {
            if (value == null || value is DBNull)
            {
                return "null";
            }

            if (value is bool)
            {
                return value.ToString().ToLower();
            }

            return value.ToString();
        }

        public static string GetMethodDescription(MethodInfo method, XmlDocument assemblyXml)
        {
            string description = string.Empty;
            if (method.GetParameters().Length > 0)
            {
                description = SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[starts-with(@name,'M:{0}.{1}(')]", method.ReflectedType.FullName, method.Name), assemblyXml);
                if (string.IsNullOrEmpty(description))
                {
                    description = SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[starts-with(@name,'M:{0}.{1}``1(')]", method.ReflectedType.FullName, method.Name), assemblyXml);
                }
            }
            else
            {
                description = SchemeManager.GetDescriptionByAssemblyXml(string.Format("//member[@name='M:{0}.{1}']", method.ReflectedType.FullName, method.Name), assemblyXml);
            }
            return description;
        }

        public static string GetDescriptionByAssemblyXml(string xPath, XmlDocument assemblyXml)
        {
            var classNode = assemblyXml.SelectNodes(xPath);
            if (classNode == null || classNode.Count == 0 || classNode[0].ChildNodes == null)
                return "";

            foreach (XmlNode child in classNode[0].ChildNodes)
            {
                if (child.Name == "summary")
                    return HttpUtility.HtmlEncode(child.InnerText.Trim());
            }

            return classNode[0].InnerText.Trim();
        }

        public static KalturaActionDetails GetCrudActionDetails(CrudActionAttribute actionAttribute, MethodInfo method)
        {
            var crudActionDetails = new KalturaActionDetails()
            {
                LoweredName = actionAttribute.GetName(),
                RealName = method.Name,
                Description = actionAttribute.Summary,
                IsGenericMethod = false,
                IsDeprecated = false,
                IsSessionRequired = true,
            };

            var parameters = method.GetParameters();
            foreach (var parameter in parameters)
            {
                var crudPrameter = GetCrudPrameterDetails(parameter, method, actionAttribute);
                if (crudPrameter != null)
                {
                    crudActionDetails.Prameters.Add(crudPrameter);
                }
            }

            if (method.ReturnType != typeof(void))
            {
                crudActionDetails.ReturnedTypes = GetParameterTypes(method.ReturnType);
            }

            // write throws
            if (actionAttribute.ApiThrows != null && actionAttribute.ApiThrows.Length > 0)
            {
                crudActionDetails.ApiThrows.AddRange(actionAttribute.ApiThrows);
            }

            if ((actionAttribute.ClientThrows != null && actionAttribute.ClientThrows.Length > 0))
            {
                crudActionDetails.ClientThrows.AddRange(actionAttribute.ClientThrows);
            }

            return crudActionDetails;
        }

        private static KalturaPrameterDetails GetCrudPrameterDetails(ParameterInfo parameter, MethodInfo method, CrudActionAttribute actionAttribute)
        {
            var isOptional = SchemeManager.IsParameterOptional(parameter, actionAttribute.GetOptionalParameters());
            if (!isOptional.HasValue) { return null; } // parameter does not exist

            var prameterDetails = new KalturaPrameterDetails()
            {
                ParameterType = parameter.ParameterType,
                Name = parameter.Name,
                ParameterTypes = GetParameterTypes(parameter.ParameterType),
                IsOptional = isOptional.Value,
                Description = actionAttribute.GetDescription(parameter.Name),
                Position = parameter.Position
            };

            if (prameterDetails.IsOptional)
            {
                prameterDetails.DefaultValue = SchemeManager.VarToString(parameter.DefaultValue);
            }

            return prameterDetails;
        }

        public static Dictionary<string, string> GetParameterTypes(Type type)
        {
            var parameterTypes = new Dictionary<string, string>();

            //Handling nullables
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = type.GetGenericArguments()[0];
            }

            //Handling Enums
            if (type.IsEnum)
            {
                bool isIntEnum = type.GetCustomAttribute<KalturaIntEnumAttribute>() != null;
                var etype = isIntEnum ? "int" : "string";
                parameterTypes.Add("type", isIntEnum ? "int" : "string");
                parameterTypes.Add("enumType", GetTypeName(type));
                return parameterTypes;
            }

            //Handling arrays
            if (type.IsArray)
            {
                parameterTypes.Add("type", "array");
                parameterTypes.Add("arrayType", GetTypeName(type.GetElementType()));
                return parameterTypes;
            }

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(List<>))
                {
                    parameterTypes.Add("type", "array");
                    parameterTypes.Add("arrayType", GetTypeName(type.GetGenericArguments()[0]));
                    return parameterTypes;
                }

                if (genericTypeDefinition == typeof(Dictionary<,>) || genericTypeDefinition == typeof(SerializableDictionary<,>))
                {
                    parameterTypes.Add("type", "map");
                    parameterTypes.Add("arrayType", GetTypeName(type.GetGenericArguments()[1]));
                    return parameterTypes;
                }
            }

            parameterTypes.Add("type", GetTypeName(type));
            return parameterTypes;
        }

        public static string GetTypeName(Type type)
        {
            if (type == typeof(String))
                return "string";
            if (type == typeof(DateTime))
                return "int";
            if (type == typeof(long) || type == typeof(Int64))
                return "bigint";
            if (type == typeof(Int32))
                return "int";
            if (type == typeof(double) || type == typeof(float))
                return "float";
            if (type == typeof(bool))
                return "bool";
            if (type.IsEnum)
                return type.Name;

            if (type == typeof(KalturaOTTFile))
                return "file";

            if (type == typeof(KalturaOTTObject))
                return "KalturaObject";

            if (typeof(KalturaRenderer).IsAssignableFrom(type))
                return "file";

            Regex regex = new Regex("^[^`]+");
            Match match = regex.Match(type.Name);
            return match.Value;
        }

        public static bool IsGenericListResponse(Type type, out ListResponseAttribute listResponseAttribute, out PropertyInfo objectsProperty)
        {
            bool isGenericListResponse = false;
            listResponseAttribute = null;
            objectsProperty = null;

            if (type.BaseType != null && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(KalturaListResponse<>))
            {
                isGenericListResponse = true;
                listResponseAttribute = type.GetCustomAttribute<ListResponseAttribute>(true);
                objectsProperty = type.BaseType.GetProperty("Objects");
            }

            return isGenericListResponse;
        }
    }
}