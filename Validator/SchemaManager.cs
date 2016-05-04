using KLogMonitor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using WebAPI.Models.General;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Xml;
using System.Globalization;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Schema;

namespace Validator.Managers.Schema
{
    public class SchemaManager
    {
        private static XmlDocument GetProjectXml()
        {
            string filename = Assembly.GetExecutingAssembly().CodeBase;
            const string prefix = "file:///";

            if (filename.StartsWith(prefix))
            {
                FileInfo dll = new FileInfo(filename.Substring(prefix.Length));
                var projectDir = dll.Directory.Parent.Parent.Parent;

                StreamReader streamReader;

                try
                {
                    streamReader = new StreamReader(projectDir.FullName + @"\WebAPI\WebAPI.csproj");
                }
                catch (FileNotFoundException exception)
                {
                    throw new Exception("XML documentation not present (make sure it is turned on in project properties when building)", exception);
                }

                XmlDocument projectXml = new XmlDocument();
                projectXml.Load(streamReader);

                return projectXml;
            }
            else
            {
                throw new Exception("Could not ascertain assembly filename", null);
            }
        }

        private static Dictionary<string, string> map;
        private static Dictionary<string, string> getFilesMap()
        {
            if (map != null)
                return map;

            map = new Dictionary<string, string>();
            XmlDocument xml = GetProjectXml();

            string xPath = "//*[local-name()='Compile']";
            var compileNodes = xml.SelectNodes(xPath);
            if (compileNodes != null && compileNodes.Count > 0)
            {
                foreach (XmlNode compileNode in compileNodes)
                {
                    string path = compileNode.Attributes["Include"].Value;
                    string type = path.Substring(path.LastIndexOf(@"\") + 1).Replace(".cs", "");
                    if(!map.ContainsKey(type))
                        map.Add(type, path);
                }
            }

            return map;
        }

        private static string getFilePath(Type type)
        {
            Dictionary<string, string> map = getFilesMap();
            if (map.ContainsKey(type.Name))
                return map[type.Name];

            return "";
        }

        public static void Generate(Stream stream)
        {
            Schema writer = new Schema(stream);
            writer.write();
        }

        public static bool Validate()
        {
            Schema validator = new Schema(true);
            return validator.validate();
        }

        public static bool Validate(Type type, bool strict)
        {
            if (type.IsSubclassOf(typeof(ApiController)))
                return ValidateService(type, strict);

            bool valid = true;

            if (!type.Name.StartsWith("Kaltura"))
            {
                logError("Error", type, string.Format("Type {0} doesn't have Kaltura prefix", type.Name));
                valid = false;
            }

            if (type.IsEnum)
                return ValidateEnum(type, strict) && valid;

            return ValidateObject(type, strict) && valid;
        }

        private static bool hasValidationException(ICustomAttributeProvider attributeProvider, SchemaValidationType type)
        {
            object[] attributes = attributeProvider.GetCustomAttributes(true);
            foreach (Attribute attribute in attributes)
            {
                if (attribute.GetType() == typeof(ValidationException))
                {
                    ValidationException exception = attribute as ValidationException;
                    if (exception.ValidationType == type)
                        return true;
                }
            }

            return false;
        }

        private static string getApiName(PropertyInfo property)
        {
            DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>();
            if (dataMember == null)
                return null;

            return dataMember.Name;
        }

        private static bool ValidateProperty(PropertyInfo property, bool strict)
        {
            bool valid = true;

            if (property.Name.Contains('_'))
            {
                logError("Warning", property.DeclaringType, string.Format("Property {0}.{1} ({2}) name may not contain underscores", property.ReflectedType.Name, property.Name, property.PropertyType.Name));
                if (strict)
                    valid = false;
            }

            string apiName = getApiName(property);
            if (apiName == null)
            {
                logError("Error", property.DeclaringType, string.Format("Data member attribute is not defined for property {0}.{1} ({2})", property.ReflectedType.Name, property.Name, property.PropertyType.Name));
                valid = false;
            }
            else
            {
                if (apiName.Contains('_'))
                {
                    logError("Warning", property.DeclaringType, string.Format("Property {0}.{1} ({2}) data member ({3}) name may not contain underscores", property.ReflectedType.Name, property.Name, property.PropertyType.Name, apiName));
                    if (strict)
                        valid = false;
                }

                if (!Char.IsLower(apiName, 0))
                {
                    logError("Warning", property.DeclaringType, string.Format("Property {0}.{1} ({2}) data member ({3}) must start with a small lette", property.ReflectedType.Name, property.Name, property.PropertyType.Name, apiName));
                    if (strict)
                        valid = false;
                }
            }

            string description = string.Format("Property {0}.{1} ({2})", property.ReflectedType.Name, property.Name, property.PropertyType.Name);
            valid = ValidateAttribute(property.DeclaringType, property.PropertyType, description, strict) && valid;

            return valid;
        }

        private static bool ValidateFilter(Type type, bool strict)
        {
            bool valid = true;

            string[] availableFilterSuffixes = new string[]{
			    "LessThan",
			    "LessThanOrEqual",
			    "GreaterThan",
			    "GreaterThanOrEqual",
			    "LessThanOrNull",
			    "LessThanOrEqualOrNull",
			    "GreaterThanOrNull",
			    "GreaterThanOrEqualOrNull",
			    "Equal",
			    "Like",
			    "MultiLikeOr",
			    "MultiLikeAnd",
			    "EndsWith",
			    "StartsWith",
			    "In",
			    "NotIn",
			    "NotEqual",
			    "BitAnd",
			    "BitOr",
			    "MatchOr",
			    "MatchAnd",
			    "NotContains",
			    "Empty"
            };

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (hasValidationException(property, SchemaValidationType.FILTER_SUFFIX))
                    continue;

                JsonPropertyAttribute jsonProperty = property.GetCustomAttribute<JsonPropertyAttribute>(true);
                bool hasRightSuffix = false;
                foreach (string suffix in availableFilterSuffixes)
                {
                    if (jsonProperty.PropertyName.EndsWith(suffix))
                    {
                        hasRightSuffix = true;
                        break;
                    }
                }

                if (!hasRightSuffix)
                {
                    logError("Warning", property.DeclaringType, string.Format("Filter property {0}.{1} ({2}) data member ({3}) must use on of the following suffixes: {4}", property.ReflectedType.Name, property.Name, property.PropertyType.Name, jsonProperty.PropertyName, String.Join(", ", availableFilterSuffixes)));
                    if (strict)
                        valid = false;
                }
            }

            return valid;
        }

        private static void logError(string category, Type type, string message)
        {
            Console.WriteLine(string.Format("{0}: {1}: {2}", getFilePath(type), category, message));
        }

        private static bool ValidateObject(Type type, bool strict)
        {
            bool valid = true;
            
            if (type.Name.EndsWith("ListResponse"))
            {
                if (type != typeof(KalturaListResponse) && !type.IsSubclassOf(typeof(KalturaListResponse)))
                {
                    logError("Error", type, string.Format("List response {0} must inherit KalturaListResponse", type.Name));
                    valid = false;
                }

                PropertyInfo objectsProperty = getObjectsProperty(type);
                if (objectsProperty == null)
                {
                    logError("Error", type, string.Format("List response {0} must implement objects attribute", type.Name));
                    valid = false;
                }
            }

            if (type.IsSubclassOf(typeof(KalturaFilterPager)))
            {
                logError("Warning", type, string.Format("Object {0} should not inherit KalturaFilterPager", type.Name));
                if (strict)
                    valid = false;
            }

            if (type.IsSubclassOf(typeof(KalturaFilter)))
            {
                valid = ValidateFilter(type, strict) && valid;
            }
            else if (type.Name.EndsWith("Filter"))
            {
                logError("Warning", type, string.Format("Filter {0} must inherit KalturaFilter", type.Name));
                if (strict)
                    valid = false;
            }

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.DeclaringType == type)
                    valid = ValidateProperty(property, strict) && valid;
            }

            return valid;
        }

        private static bool ValidateEnumValue(Type type, string name, string value, bool strict)
        {
            bool valid = true;

            if (name.ToUpper() != name)
            {
                logError("Warning", type, string.Format("Enum value {0}.{1} should be upper case", type.Name, name));
                if (strict)
                    valid = false;
            }

            return valid;
        }

        private static bool ValidateEnum(Type type, bool strict)
        {
            bool valid = true;

            bool isIntEnum = type.GetCustomAttribute<KalturaIntEnumAttribute>() != null;

            foreach (var enumValue in Enum.GetValues(type))
            {
                string value = isIntEnum ? ((int)Enum.Parse(type, enumValue.ToString())).ToString() : enumValue.ToString();
                valid = ValidateEnumValue(type, enumValue.ToString(), value, strict) && valid;
            }

            return true;
        }

        private static bool ValidateService(Type controller, bool strict)
        {
            bool valid = true;
            string serviceId = getServiceId(controller);

            RoutePrefixAttribute routePrefix = controller.GetCustomAttribute<RoutePrefixAttribute>();
            string expectedRoutePrefix = String.Format("_service/{0}/action", serviceId);
            if (routePrefix.Prefix != expectedRoutePrefix)
            {
                logError("Error", controller, string.Format("Wrong route prefix [{0}] for controller {1}, expected [{2}]", routePrefix.Prefix, controller.Name, expectedRoutePrefix));
                valid = false;
            }

            var methods = controller.GetMethods().OrderBy(method => method.Name);
            foreach (MethodInfo method in methods)
            {
                if (!method.IsPublic || method.DeclaringType.Namespace != "WebAPI.Controllers")
                    continue;

                valid = Validate(method, strict) && valid;
            };

            return valid;
        }

        private static bool ValidateAttribute(Type declaringClass, Type attribute, string description, bool strict)
        {
            bool valid = true;

            if (attribute == typeof(DateTime))
            {
                logError("Error", declaringClass, string.Format("{0} is DateTime, use long instead", description));
                valid = false;
            }
            else if (attribute.IsArray)
            {
                Type valueType = attribute.GetElementType();
                if (!valueType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    logError("Error", declaringClass, string.Format("{0} array must contain KalturaOTTObject objects (or something that extends it)", description));
                    valid = false;
                }
            }
            else if (attribute.IsGenericType && (attribute.GetGenericTypeDefinition() == typeof(Dictionary<,>) || attribute.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>)))
            {
                Type keyType = attribute.GetGenericArguments()[0];
                if (keyType != typeof(string))
                {
                    logError("Error", declaringClass, string.Format("{0} map key must be a string", description));
                    valid = false;
                }

                Type valueType = attribute.GetGenericArguments()[1];
                if (!valueType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    logError("Error", declaringClass, string.Format("{0} map must contain KalturaOTTObject objects (or something that extends it)", description));
                    valid = false;
                }
            }
            else if (attribute.IsGenericType && attribute.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type valueType = attribute.GetGenericArguments()[0];
                if (!valueType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    logError("Warning", declaringClass, string.Format("{0} list must contain KalturaOTTObject objects (or something that extends it)", description));
                    if (strict)
                        valid = false;
                }
            }
            else if (attribute.IsGenericType && attribute.GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                logError("Error", declaringClass, string.Format("{0} unexpected type", description));
                valid = false;
            }
            else if (attribute.IsClass && attribute != typeof(string) && !attribute.IsSubclassOf(typeof(KalturaOTTObject)))
            {
                logError("Error", declaringClass, string.Format("{0} object must inherit from KalturaOTTObject (or something that extends it)", description));
                valid = false;
            }

            return valid;
        }

        private static bool ValidateParameter(MethodInfo action, ParameterInfo param, bool strict)
        {
            bool valid = true;

            RouteAttribute route = action.GetCustomAttribute<RouteAttribute>(false);
            Type controller = action.ReflectedType;
            string serviceId = SchemaManager.getServiceId(controller);
            string actionId = route.Template;

            if (param.Name.Contains('_'))
            {
                logError("Warning", controller, string.Format("Parameter {0} in method {1}.{2} ({3}) may not contain underscores", param.Name, serviceId, actionId, controller.Name));
                if (strict)
                    valid = false;
            }

            if (!Char.IsLower(param.Name, 0))
            {
                logError("Warning", controller, string.Format("Parameter {0} in method {1}.{2} ({3}) must start with a small letter", param.Name, serviceId, actionId, controller.Name));
                if (strict)
                    valid = false;
            }

            string description = string.Format("Parameter {0} in method {1}.{2} ({3})", param.Name, serviceId, actionId, controller.Name);
            valid = ValidateAttribute(controller, param.ParameterType, description, strict) && valid;

            return valid;
        }

        internal static bool Validate(MethodInfo action, bool strict)
        {
            bool valid = true;

            RouteAttribute route = action.GetCustomAttribute<RouteAttribute>(false);
            Type controller = action.ReflectedType;
            string serviceId = SchemaManager.getServiceId(controller);

            if (route == null)
            {
                logError("Error", controller, string.Format("Action {0}.{1} ({2}) has no routing attribute", serviceId, action.Name, controller.Name));
                return false;
            }

            string actionId = route.Template;
            if (actionId != "get" && actionId != "add" && actionId != "update" && actionId != "delete" && actionId != "list" && !hasValidationException(action, SchemaValidationType.ACTION_NAME))
            {
                logError("Warning", controller, string.Format("Action {0}.{1} ({2}) has non-standard name (add, update, get, delete and list are allowed)", serviceId, actionId, controller.Name));
                if (strict)
                    valid = false;
            }

            if (actionId == "get" || actionId == "add" || actionId == "update")
            {
                string expectedObjectType = string.Format("Kaltura{0}", FirstCharacterToUpper(serviceId));
                if (action.ReturnType.Name != expectedObjectType)
                {
                    logError("Warning", controller, string.Format("Action {0}.{1} ({2}) returned type is {3}, expected {4}", serviceId, actionId, controller.Name, action.ReturnType.Name, expectedObjectType));
                    if (strict)
                        valid = false;
                }
            }

            if (actionId == "list")
            {
                string expectedObjectType = string.Format("Kaltura{0}ListResponse", FirstCharacterToUpper(serviceId));
                string expectedResponseType = string.Format("Kaltura{0}ListResponse", FirstCharacterToUpper(serviceId));
                if (action.ReturnType.Name != expectedResponseType)
                {
                    logError("Warning", controller, string.Format("Action {0}.{1} ({2}) returned type is {3}, expected {4}", serviceId, actionId, controller.Name, action.ReturnType.Name, expectedResponseType));
                    if (strict)
                        valid = false;
                }
                else
                {
                    PropertyInfo objectsProperty = getObjectsProperty(action.ReturnType);
                    Type arrayType = objectsProperty.PropertyType.GetGenericArguments()[0];
                    if (arrayType.Name != expectedObjectType)
                    {
                        logError("Warning", controller, string.Format("Action {0}.{1} ({2}) returned list-response contains array of {3}, expected {4}", serviceId, actionId, controller.Name, arrayType.Name, expectedObjectType));
                        if (strict)
                            valid = false;
                    }
                }
            }

            foreach (var param in action.GetParameters())
            {
                valid = ValidateParameter(action, param, strict) && valid;
            }

            return valid;
        }

        public static PropertyInfo getObjectsProperty(Type listResponseType)
        {
            foreach (PropertyInfo property in listResponseType.GetProperties())
            {
                JsonPropertyAttribute jsonProperty = property.GetCustomAttribute<JsonPropertyAttribute>(true);
                if (jsonProperty.PropertyName == "objects")
                    return property;
            }

            return null;
        }

        public static string getServiceId(Type controller)
        {
            return FirstCharacterToLower(controller.Name.Replace("Controller", ""));
        }

        public static string FirstCharacterToLower(string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
                return str;

            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public static string FirstCharacterToUpper(string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsUpper(str, 0))
                return str;

            return Char.ToUpper(str[0]) + str.Substring(1);
        }
    }
}