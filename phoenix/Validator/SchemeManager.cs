using KLogMonitor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using WebAPI.Models.General;
using System.Web.Http;
using System.Xml;
using System.Globalization;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using System.Runtime.CompilerServices;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using System.Text.RegularExpressions;
using System.Net.Http;
using WebAPI.Controllers;
using WebAPI.Models.Renderers;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace Validator.Managers.Scheme
{
    public class SchemeManager
    {
        private static readonly string[] availableFilterSuffixes = new string[]{
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
        
        private static readonly Type kalturaListResponseType = typeof(KalturaListResponse);

        private static string GetProjectDir()
        {
            string filename = Assembly.GetExecutingAssembly().CodeBase;
            const string prefix = "file:///";

            if (filename.StartsWith(prefix))
            {
                FileInfo dll = new FileInfo(filename.Substring(prefix.Length));
                var projectDir = dll.Directory.Parent.Parent.Parent.Parent;
                return projectDir.FullName;
            }
            else
            {
                throw new Exception("Could not ascertain assembly filename", null);
            }
        }

        private static XmlDocument GetProjectXml()
        {   
            StreamReader streamReader;

            try
            {
                streamReader = new StreamReader(GetProjectDir() + @"\WebAPI\WebAPI.csproj");
            }
            catch (FileNotFoundException exception)
            {
                throw new Exception("XML documentation not present (make sure it is turned on in project properties when building)", exception);
            }

            XmlDocument projectXml = new XmlDocument();
            projectXml.Load(streamReader);

            return projectXml;
        }

        private static Dictionary<string, string> map;
        private static Dictionary<string, string> getFilesMap()
        {
            if (map != null)
                return map;

            var webAPIProjectPath = Path.Combine(GetProjectDir(),"WebApi");
            var allfiles = Directory.GetFiles(webAPIProjectPath, "*.cs", SearchOption.AllDirectories);

            map = new Dictionary<string, string>();
           
            if (allfiles != null && allfiles.Length > 0)
            {
                foreach (var filePath in allfiles)
                {
                    string type = filePath.Substring(filePath.LastIndexOf(@"\") + 1).Replace(".cs", "");
                    if(!map.ContainsKey(type))
                        map.Add(type, filePath);
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
            Scheme writer = Scheme.getInstance();
            writer.RemoveInvalidObjects();
            writer.write(stream);
        }

        public static bool Validate()
        {
            return Scheme.getInstance().validate();
        }

        public static bool ValidateErrors(IEnumerable<Type> exceptions, bool strict)
        {
            bool valid = true;

            Dictionary<int, string> codes = new Dictionary<int, string>();
            foreach (Type exception in exceptions.OrderBy(exception => exception.Name))
            {
                FieldInfo[] fields = exception.GetFields();
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(ApiException.ApiExceptionType))
                    {
                        ApiException.ApiExceptionType type = (ApiException.ApiExceptionType)field.GetValue(null);
                        string error = string.Format("{0}.{1}", exception.Name, field.Name);
                        if (codes.ContainsKey(type.statusCode))
                        {
                            logError("Error", exception, string.Format("Error code {0} appears both in error {1} and error {2}", type.statusCode, error, codes[type.statusCode]));
                            valid = false;
                        }
                        else
                        {
                            codes.Add(type.statusCode, error);
                        }
                    }
                }
            }

            return valid;
        }

        public static bool Validate(Type type, bool strict, XmlDocument assemblyXml)
        {
            ObsoleteAttribute obsolete = type.GetCustomAttribute<ObsoleteAttribute>(true);
            if (obsolete != null)
            {
                return !strict;
            }

            if (typeof(IKalturaController).IsAssignableFrom(type))
            {
                return ValidateService(type, strict, assemblyXml);
            }

            bool valid = true;

            if (!type.Name.StartsWith("Kaltura"))
            {
                logError("Error", type, string.Format("Type {0} doesn't have Kaltura prefix", type.Name));
                valid = false;
            }

            if (type.IsEnum)
            {
                return ValidateEnum(type, strict) && valid;
            }
                
            return ValidateObject(type, strict) && valid;
        }

        private static bool hasValidationException(ICustomAttributeProvider attributeProvider, SchemeValidationType type)
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

        private static bool ValidateProperty(PropertyInfo property, bool strict, ListResponseAttribute listResponseAttribute)
        {
            bool valid = true;
            if (property.DeclaringType != null && property.DeclaringType == typeof(KalturaCrudFilter<,,,>)) { return valid; }
            
            if (property.Name.Contains('_'))
            {
                logError("Error", property.DeclaringType, string.Format("Property {0}.{1} ({2}) name may not contain underscores", property.ReflectedType.Name, property.Name, property.PropertyType.Name));
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
                    logError("Error", property.DeclaringType, string.Format("Property {0}.{1} ({2}) data member ({3}) name may not contain underscores", property.ReflectedType.Name, property.Name, property.PropertyType.Name, apiName));
                    if (strict)
                        valid = false;
                }

                if (!Char.IsLower(apiName, 0))
                {
                    logError("Error", property.DeclaringType, string.Format("Property {0}.{1} ({2}) data member ({3}) must start with a small letter", property.ReflectedType.Name, property.Name, property.PropertyType.Name, apiName));
                    valid = false;
                }
            }

            // description
            var description = Scheme.getInstance().GetPropertyDescription(property);
            description = description ?? description.Trim();
            if (string.IsNullOrEmpty(description))
            {
                logError("Error", property.DeclaringType, string.Format("Property {0}.{1} ({2}) data member ({3}) has no description", property.ReflectedType.Name, property.Name, property.PropertyType.Name, apiName));
            }

            string errorDescription = string.Format("Property {0}.{1} ({2})", property.ReflectedType.Name, property.Name, property.PropertyType.Name);
            valid = ValidateAttribute(property.DeclaringType, property.PropertyType, errorDescription, strict) && valid;

            return valid;
        }

        private static bool ValidateFilter(Type type, bool strict)
        {
            bool valid = true;
            if (type == typeof(KalturaCrudFilter<,,,>)) { return valid; };
            
            foreach (PropertyInfo property in type.GetProperties().Where(x => x.DeclaringType == type && !hasValidationException(x, SchemeValidationType.FILTER_SUFFIX)))
            {
                ObsoleteAttribute obsolete = property.GetCustomAttribute<ObsoleteAttribute>(true);
                if (obsolete != null)
                    continue;
                
                DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>(false);
                if (dataMember == null) { continue; }

                JsonPropertyAttribute jsonProperty = property.GetCustomAttribute<JsonPropertyAttribute>(true);
                if (jsonProperty == null)
                {
                    valid = false;
                    logError("Error", property.DeclaringType, string.Format("Filter property {0}.{1} ({2}) data member is invalid", 
                        property.ReflectedType.Name, property.Name, property.PropertyType.Name));
                    continue;
                }

                if (!property.PropertyType.IsPrimitive && !property.PropertyType.IsEnum && property.PropertyType != typeof(string))
                {
                    if (!property.PropertyType.IsGenericType || property.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                    {
                        logError("Error", property.DeclaringType, string.Format("Filter property {0}.{1} ({2}) data member ({3}) must be primitive, string or enum", property.ReflectedType.Name, property.Name, property.PropertyType.Name, jsonProperty.PropertyName));
                        if (strict)
                            valid = false;
                    }
                }

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
                    logError("Error", property.DeclaringType, string.Format("Filter property {0}.{1} ({2}) data member ({3}) must use on of the following suffixes: {4}", property.ReflectedType.Name, property.Name, property.PropertyType.Name, jsonProperty.PropertyName, String.Join(", ", availableFilterSuffixes)));
                    valid = false;
                }
            }

            return valid;
        }

        private static void logError(string category, Type type, string message, [CallerLineNumber] int lineNumber = 0)
        {
            Console.WriteLine(string.Format("{0}: {1}: [{2,3}] {3}", getFilePath(type), category, lineNumber, message));
        }

        private static bool ValidateObject(Type type, bool strict)
        {
            bool valid = true;

            ListResponseAttribute listResponseAttribute = null;
            if (type != kalturaListResponseType && type.Name.EndsWith("ListResponse"))
            {
                var isNewListResponse = IsGenericListResponse(type, out listResponseAttribute, out PropertyInfo objectsProperty);
                if (!type.IsSubclassOf(kalturaListResponseType) && !isNewListResponse)
                {
                    logError("Error", type, string.Format("List response {0} must inherit KalturaListResponse", type.Name));
                    valid = false;
                }

                if (!isNewListResponse)
                {
                    objectsProperty = getObjectsProperty(type);
                }
                
                if (objectsProperty == null)
                {
                    logError("Error", type, string.Format("List response {0} must implement objects attribute", type.Name));
                    valid = false;
                }
            }
            
            if (type.IsSubclassOf(typeof(KalturaFilterPager)))
            {
                logError("Error", type, string.Format("Object {0} should not inherit KalturaFilterPager", type.Name));
                valid = false;
            }

            if (typeof(IKalturaFilter).IsAssignableFrom(type))
            {
                valid = ValidateFilter(type, strict) && valid;
            }
            else if (!type.Name.Equals("KalturaFilter") && type.Name.EndsWith("Filter"))
            {
                logError("Error", type, string.Format("Filter {0} must inherit KalturaFilter", type.Name));
                valid = false;
            }

            foreach (PropertyInfo property in type.GetProperties().Where(x => x.DeclaringType == type))
            {
                var obsolete = property.GetCustomAttribute<ObsoleteAttribute>(true);
                if (obsolete != null) { continue; }

                var dataMember = property.GetCustomAttribute<DataMemberAttribute>(false);
                if (dataMember == null) { continue; }

                valid = ValidateProperty(property, strict, listResponseAttribute) && valid;
            }

            return valid;
        }

        private static bool ValidateEnum(Type type, bool strict)
        {
            return true;
        }

        private static bool ValidateService(Type controller, bool strict, XmlDocument assemblyXml)
        {
            if (controller.IsAbstract) { return true; }
            
            bool valid = true;
            string serviceId = getServiceId(controller);

            ServiceAttribute serviceAttribute = controller.GetCustomAttribute<ServiceAttribute>();
            if (serviceAttribute == null) { return !strict; }
                
            var methods = controller.GetMethods().Where(x => x.IsPublic && x.DeclaringType.Namespace == "WebAPI.Controllers").OrderBy(method => method.Name).ToList();
            var hasValidActions = false;
            if (methods.Count == 0)
            {
                hasValidActions = true;
            }
            else
            {
                foreach (MethodInfo method in methods)
                {
                    hasValidActions = ValidateMethod(method, strict, assemblyXml) || hasValidActions;
                };
            }
            
            return valid && hasValidActions;
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
                    logError("Error", declaringClass, string.Format("{0} list must contain KalturaOTTObject objects (or something that extends it)", description));
                    valid = false;
                }
            }
            else if (attribute.IsClass && attribute != typeof(string) && attribute != typeof(KalturaOTTFile) && !attribute.IsSubclassOf(typeof(KalturaOTTObject)))
            {
                logError("Error", declaringClass, string.Format("{0} object must inherit from KalturaOTTObject (or something that extends it)", description));
                valid = false;
            }

            return valid;
        }

        private static bool ValidateParameter(MethodInfo action, ParameterInfo param, bool strict)
        {
            bool valid = true;

            ActionAttribute actionAttribute = action.GetCustomAttribute<ActionAttribute>(false);
            Type controller = action.ReflectedType;
            string serviceId = SchemeManager.getServiceId(controller);
            string actionId = actionAttribute.Name;

            if (param.Name.Contains('_'))
            {
                logError("Error", controller, string.Format("Parameter {0} in method {1}.{2} ({3}) may not contain underscores", param.Name, serviceId, actionId, controller.Name));
                valid = false;
            }

            if (!Char.IsLower(param.Name, 0))
            {
                logError("Error", controller, string.Format("Parameter {0} in method {1}.{2} ({3}) must start with a small letter", param.Name, serviceId, actionId, controller.Name));
                valid = false;
            }

            // description
            string description = Scheme.getInstance().getDescription(action, param);
            description = description.Trim();
            if (string.IsNullOrEmpty(description))
            {
                logError("Error", controller, string.Format("Parameter {0} in method {1}.{2} ({3}) has no description", param.Name, serviceId, actionId, controller.Name));
            }

            string errorDescription = string.Format("Parameter {0} in method {1}.{2} ({3})", param.Name, serviceId, actionId, controller.Name);
            valid = ValidateAttribute(controller, param.ParameterType, errorDescription, strict) && valid;

            return valid;
        }

        internal static bool ValidateMethod(MethodInfo method, bool strict, XmlDocument assemblyXml)
        {
            bool valid = true;

            ObsoleteAttribute obsolete = method.GetCustomAttribute<ObsoleteAttribute>(true);
            if (obsolete != null)
                return !strict;

            ActionAttribute actionAttribute = method.GetCustomAttribute<ActionAttribute>(false);
            Type controller = method.ReflectedType;
            string serviceId = SchemeManager.getServiceId(controller);

            if (actionAttribute == null)
            {
                logError("Error", controller, string.Format("Action {0}.{1} ({2}) has no action attribute", serviceId, method.Name, controller.Name));
                return false;
            }

            string actionId = actionAttribute.Name;
            if (actionId != "get" && actionId != "add" && actionId != "update" && actionId != "delete" && actionId != "list" && actionId != "updateStatus" && actionId != "serve" && !hasValidationException(method, SchemeValidationType.ACTION_NAME))
            {
                logError("Error", controller, string.Format("Action {0}.{1} ({2}) has non-standard name (add, update, get, delete and list are allowed)", serviceId, actionId, controller.Name));
                valid = false;
            }

            string expectedObjectType = string.Format("Kaltura{0}", FirstCharacterToUpper(serviceId));
            if (actionId == "get" || actionId == "add" || actionId == "update")
            {
                if (!hasValidationException(method, SchemeValidationType.ACTION_RETURN_TYPE) && method.ReturnType.Name.ToLower() != expectedObjectType.ToLower())
                {
                    logError("Error", controller, string.Format("Action {0}.{1} ({2}) returned type is {3}, expected {4}", serviceId, actionId, controller.Name, method.ReturnType.Name, expectedObjectType));
                    valid = false;
                }
            }

            if (method.ReturnType != null && method.ReturnType.IsSubclassOf(typeof(KalturaOTTObject)) && !Validate(method.ReturnType, strict, assemblyXml))
            {
                logError("Error", controller, string.Format("Action {0}.{1} ({2}) returned type ({3}) failed validation", serviceId, actionId, controller.Name, method.ReturnType.Name));
                valid = false;
            }

            var parameters = method.GetParameters();
            foreach (var parameter in parameters)
            {
                if (parameter.ParameterType.IsSubclassOf(typeof(KalturaOTTObject)) && !Validate(parameter.ParameterType, strict, assemblyXml))
                {
                    logError("Error", controller, string.Format("Action {0}.{1} ({2}) parameter {3} ({4}) failed validation", serviceId, actionId, controller.Name, parameter.Name, parameter.ParameterType.Name));
                    valid = false;
                }
            }

            if (!hasValidationException(method, SchemeValidationType.ACTION_ARGUMENTS))
            {
                if (actionId == "get" || actionId == "delete")
                {
                    if (parameters.Length > 1)
                    {
                        logError("Error", controller, string.Format("Action {0}.{1} ({2}) only one id arguments is expected", serviceId, actionId, controller.Name));
                        valid = false;
                    }
                    else if (parameters.Length == 0)
                    {
                        logError("Error", controller, string.Format("Action {0}.{1} ({2}) id arguments is expected", serviceId, actionId, controller.Name));
                        valid = false;
                    }
                    else
                    {
                        var idParam = parameters[0];
                        if (!idParam.ParameterType.IsPrimitive && idParam.ParameterType != typeof(string))
                        {
                            logError("Error", controller, string.Format("Action {0}.{1} ({2}) id argument type is {3}, primitive is expected", serviceId, actionId, controller.Name, idParam.ParameterType.Name));
                            valid = false;
                        }
                    }
                }

                if (actionId == "add")
                {
                    if (parameters.Length != 1)
                    {
                        logError("Error", controller, string.Format("Action {0}.{1} ({2}) only one argument is expected", serviceId, actionId, controller.Name));
                        valid = false;
                    }
                    else
                    {
                        var objectParam = parameters[0];
                        if (!hasValidationException(method, SchemeValidationType.ACTION_RETURN_TYPE) && objectParam.ParameterType.Name.ToLower() != expectedObjectType.ToLower())
                        {
                            logError("Error", controller, string.Format("Action {0}.{1} ({2}) argument type is {3}, expected {4}", serviceId, actionId, controller.Name, objectParam.ParameterType.Name, expectedObjectType));
                            valid = false;
                        }
                    }
                }

                if (actionId == "update")
                {
                    if (parameters.Length != 2)
                    {
                        logError("Error", controller, string.Format("Action {0}.{1} ({2}) two arguments, id and object are expected", serviceId, actionId, controller.Name));
                        valid = false;
                    }
                    else
                    {
                        var idParam = parameters[0];
                        if (!idParam.ParameterType.IsPrimitive && idParam.ParameterType != typeof(string))
                        {
                            logError("Error", controller, string.Format("Action {0}.{1} ({2}) id argument type is {3}, primitive is expected", serviceId, actionId, controller.Name, idParam.ParameterType.Name));
                            valid = false;
                        }

                        var objectParam = parameters[1];
                        if (objectParam.ParameterType.Name.ToLower() != expectedObjectType.ToLower())
                        {
                            logError("Error", controller, string.Format("Action {0}.{1} ({2}) object argument type is {3}, expected {4}", serviceId, actionId, controller.Name, objectParam.ParameterType.Name, expectedObjectType));
                            valid = false;
                        }
                    }
                }

                if (actionId == "list")
                {
                    if (parameters.Length > 0)
                    {
                        string expectedFilterType = string.Format("Kaltura{0}Filter", FirstCharacterToUpper(serviceId));

                        var filterParam = parameters[0];
                        string filterName = filterParam.ParameterType.Name;
                        Regex regex = new Regex(string.Format("^{0}(`1)?$", expectedFilterType), RegexOptions.IgnoreCase);
                        if (!regex.IsMatch(filterName))
                        {
                            logError("Error", controller, string.Format("Action {0}.{1} ({2}) first argument type is {3}, expected {4}", serviceId, actionId, controller.Name, filterParam.ParameterType.Name, expectedFilterType));
                            valid = false;
                        }
                    }

                    if (parameters.Length > 1)
                    {
                        var pagerParam = parameters[1];
                        if (pagerParam.ParameterType != typeof(KalturaFilterPager))
                        {
                            logError("Error", controller, string.Format("Action {0}.{1} ({2}) second argument type is {3}, expected KalturaFilterPager", serviceId, actionId, controller.Name, pagerParam.ParameterType.Name));
                            valid = false;
                        }
                    }

                    if (parameters.Length > 2)
                    {
                        logError("Error", controller, string.Format("Action {0}.{1} ({2}) only filter and pager arguments are expected", serviceId, actionId, controller.Name));
                        if (strict)
                            valid = false;
                    }
                }
            }

            if (actionId == "list" && !hasValidationException(method, SchemeValidationType.ACTION_RETURN_TYPE))
            {
                string expectedResponseType = string.Format("Kaltura{0}ListResponse", FirstCharacterToUpper(serviceId));
                if (method.ReturnType.Name.ToLower() != expectedResponseType.ToLower())
                {
                    logError("Error", controller, string.Format("Action {0}.{1} ({2}) returned type is {3}, expected {4}", serviceId, actionId, controller.Name, method.ReturnType.Name, expectedResponseType));
                    valid = false;
                }
                else
                {
                    var isNewListResponse = IsGenericListResponse(method.ReturnType, out ListResponseAttribute listResponseAttribute, out PropertyInfo objectsProperty);
                    if (!isNewListResponse)
                    {
                        objectsProperty = getObjectsProperty(method.ReturnType);
                    }
                    Type arrayType = objectsProperty.PropertyType.GetGenericArguments()[0];
                    if (arrayType.Name.ToLower() != expectedObjectType.ToLower())
                    {
                        logError("Error", controller, string.Format("Action {0}.{1} ({2}) returned list-response contains array of {3}, expected {4}", serviceId, actionId, controller.Name, arrayType.Name, expectedObjectType));
                        valid = false;
                    }
                }
            }

            // description
            string description = GetMethodDescription(method, assemblyXml).Trim();
            if (string.IsNullOrEmpty(description))
            {
                logError("Error", controller, string.Format("Action {0}.{1} ({2}) has no description", serviceId, actionId, controller.Name));
            }

            foreach (var param in parameters)
            {
                valid = ValidateParameter(method, param, strict) && valid;
            }

            return valid;
        }

        public static PropertyInfo getObjectsProperty(Type listResponseType)
        {
            PropertyInfo[] properties = listResponseType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var jsonProperty = property.GetCustomAttribute<JsonPropertyAttribute>(true);
                if (jsonProperty != null && jsonProperty.PropertyName == "objects")
                    return property;
            }

            return null;
        }

        public static string getServiceId(Type controller)
        {
            ServiceAttribute service = controller.GetCustomAttribute<ServiceAttribute>(false);
            return service.Name;
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

        public static bool IsCrudController(Type controller, out Dictionary<string, CrudActionAttribute> crudActionAttributes, out Dictionary<string, MethodInfo> crudActions)
        {
            crudActionAttributes = null;
            crudActions = null;
            if (controller.BaseType != null && controller.BaseType.IsGenericType && controller.BaseType.GetGenericTypeDefinition() == typeof(KalturaCrudController<,,,,,>))
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

        internal static bool IsGenericListResponse(Type type, out ListResponseAttribute listResponseAttribute, out PropertyInfo objectsProperty)
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

        public static bool IsParameterOptional(ParameterInfo parameter, HashSet<string> optionalParameters)
        {
            var isParamOptional = parameter.IsOptional || (optionalParameters != null && optionalParameters.Contains(parameter.Name));
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
        
        internal static string GetMethodDescription(MethodInfo method, XmlDocument assemblyXml)
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
        
        internal static string GetDescriptionByAssemblyXml(string xPath, XmlDocument assemblyXml)
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
    }
}