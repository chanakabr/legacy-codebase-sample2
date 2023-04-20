using ApiObjects.Response;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace Validator.Managers.Scheme
{
    internal class SchemeValidator
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
            "Empty",
            "Contains"
        };

        private static readonly HashSet<eResponseStatus> mismatchEnums = new HashSet<eResponseStatus>() {
            eResponseStatus.OK,
            eResponseStatus.GroupDoesNotContainLanguage, 
            eResponseStatus.InvalidArgumentValue 
        };

        private static readonly Type kalturaListResponseType = typeof(KalturaListResponse);
        private const string FILES_STRUCTURE_ERRORS_EXCEL_FILE_NAME = "separate_kaltura_models";
        private const string UNKNOWN_CONTROLLER = "unknown";

        public SchemeHolder Holder;
        private bool ShouldValidateFileStructure;
        private Dictionary<string, string> map;
        private Dictionary<string, string> objectToRelatedController;

        public SchemeValidator(bool validateFileStructure)
        {
            Holder = new SchemeHolder();
            ShouldValidateFileStructure = validateFileStructure;
        }

        public bool Validate()
        {
            bool valid = true;

            Dictionary<string, string> objectToFileStructureErrors = null;
            if (ShouldValidateFileStructure)
            {
                objectToFileStructureErrors = new Dictionary<string, string>();
            }

            var objectsToValidate = Holder.Enums.OrderBy(myType => myType.Name).ToList();
            objectsToValidate.AddRange(Holder.Types.Where(x => x != typeof(KalturaOTTObject)));

            try
            {    
                valid = ValidateDuplicateObjects(objectsToValidate);

                foreach (Type objectType in objectsToValidate)
                {
                    List<string> fileStructureErrors = null;
                    if (ShouldValidateFileStructure)
                    {
                        fileStructureErrors = new List<string>();
                    }

                    if (!ValidateType(objectType, false, out fileStructureErrors))
                        valid = false;

                    if (fileStructureErrors?.Count > 0 && !objectToFileStructureErrors.ContainsKey(objectType.Name))
                    {
                        objectToFileStructureErrors.Add(objectType.Name, string.Join(";", fileStructureErrors));
                    }
                }

                foreach (Type controller in Holder.Controllers.OrderBy(controller => controller.Name))
                {
                    var controllerAttr = controller.GetCustomAttribute<ApiExplorerSettingsAttribute>(false);

                    if (controllerAttr != null && controllerAttr.IgnoreApi)
                        continue;

                    if (!ValidateType(controller, false, out List<string> _))
                        valid = false;
                }

                ValidateErrors(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error: there was an exception in Scheme.validate(): {0}", ex.ToString()));
                valid = false;
                throw;
            }

            if (objectToFileStructureErrors?.Count > 0)
            {
                SaveFilesStructureValidationErrorsToExcel(objectToFileStructureErrors, objectsToValidate);
            }

            return valid;
        }

        private bool ValidateDuplicateObjects(List<Type> objectsToValidate)
        {
            bool valid = true;
            if (ShouldValidateFileStructure)
            {
                var duplicates = objectsToValidate.GroupBy(x => x.Name).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
                if (duplicates.Count > 0)
                {
                    valid = false;
                    foreach (var duplicatedObjectName in duplicates)
                    {
                        var duplicatedObjects = objectsToValidate.Where(x => x.Name == duplicatedObjectName);
                        foreach (var duplicatedObject in duplicatedObjects)
                        {
                            logError("Error", duplicatedObject, $"object was delclered with the name [{duplicatedObjectName}] more than 1.");
                        }
                    }
                    objectsToValidate.RemoveAll(x => duplicates.Contains(x.Name));
                }
            }
            return valid;
        }

        public bool ValidateType(Type type, bool strict, out List<string> fileStructureErrors)
        {
            fileStructureErrors = null;
            bool valid = true;
            if (ShouldValidateFileStructure)
            {
                fileStructureErrors = ValidateFileStructure(type);
                valid = fileStructureErrors.Count == 0;
            }

            ObsoleteAttribute obsolete = type.GetCustomAttribute<ObsoleteAttribute>(true);
            if (obsolete != null)
            {
                return !strict;
            }

            if (typeof(IKalturaController).IsAssignableFrom(type))
            {
                return ValidateService(type, strict) && valid;
            }

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

        public bool ValidateMethod(MethodInfo method, bool strict, bool doNotValidateIsInternal = true)
        {
            bool valid = true;

            ObsoleteAttribute obsolete = method.GetCustomAttribute<ObsoleteAttribute>(true);
            if (obsolete != null)
                return !strict;

            ActionAttribute actionAttribute = method.GetCustomAttribute<ActionAttribute>(false);
            if (!doNotValidateIsInternal && actionAttribute.IsInternal)
            {
                return false;
            }

            Type controller = method.ReflectedType;
            string serviceId = SchemeManager.getServiceId(controller);

            if (actionAttribute == null)
            {
                logError("Error", controller, $"Action {serviceId}.{method.Name} ({controller.Name}) has no action attribute");
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

            if (method.ReturnType != null && method.ReturnType.IsSubclassOf(typeof(KalturaOTTObject)) && !ValidateType(method.ReturnType, strict, out var _))
            {
                logError("Error", controller, string.Format("Action {0}.{1} ({2}) returned type ({3}) failed validation", serviceId, actionId, controller.Name, method.ReturnType.Name));
                valid = false;
            }

            var parameters = method.GetParameters();
            foreach (var parameter in parameters)
            {
                if (parameter.ParameterType.IsSubclassOf(typeof(KalturaOTTObject)) && !ValidateType(parameter.ParameterType, strict, out var _))
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
            }

            // description
            string description = SchemeManager.GetMethodDescription(method, this.Holder.AssemblyXml).Trim();
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

        public bool ValidateErrors(bool strict)
        {
            bool valid = true;

            Dictionary<int, string> codes = new Dictionary<int, string>();
            var orderedErrors = this.Holder.Exceptions.OrderBy(exception => exception.Name).ToList();
            foreach (Type exception in orderedErrors)
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
                    else if (field.FieldType == typeof(ApiException.ClientExceptionType))
                    {
                        var type = (ApiException.ClientExceptionType)field.GetValue(null);
                        string error = string.Format("{0}.{1}", exception.Name, field.Name);
                        if (codes.ContainsKey((int)type.statusCode))
                        {
                            logError("Error", exception, string.Format("Error code {0} appears both in error {1} and error {2}", type.statusCode, error, codes[(int)type.statusCode]));
                            valid = false;
                        }
                        else
                        {
                            codes.Add((int)type.statusCode, error);
                        }
                    }
                }
            }

            var eResponseStatusType = typeof(eResponseStatus);
            var values = ((eResponseStatus[])Enum.GetValues(eResponseStatusType));
            foreach (var enumValue in values)
            {
                if (mismatchEnums.Contains(enumValue))
                {
                    continue;
                }

                if (!codes.ContainsKey((int)enumValue))
                {
                    logError("Error", eResponseStatusType, string.Format("Error code {0} does not exist as ClientExceptionType in ApiExceptionErrors.cs", enumValue));
                }
            }
            
            return valid;
        }

        private bool ValidateObject(Type type, bool strict)
        {
            bool valid = true;

            if (type != kalturaListResponseType && type.Name.EndsWith("ListResponse"))
            {
                if (!type.IsSubclassOf(kalturaListResponseType))
                {
                    logError("Error", type, string.Format("List response {0} must inherit KalturaListResponse", type.Name));
                    valid = false;
                }

                var objectsProperty = getObjectsProperty(type);
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

            var internalObject = type.GetCustomAttribute<InternalObjectAttribute>(true);
            if (internalObject != null && internalObject.IsInternal)
            {
                return true;
            }

            foreach (PropertyInfo property in type.GetProperties().Where(x => x.DeclaringType == type))
            {
                var obsolete = property.GetCustomAttribute<ObsoleteAttribute>(true);
                if (obsolete != null) { continue; }

                var dataMember = property.GetCustomAttribute<DataMemberAttribute>(false);
                if (dataMember == null) { continue; }

                valid = ValidateProperty(property, strict) && valid;
            }

            return valid;
        }

        private bool ValidateProperty(PropertyInfo property, bool strict)
        {
            bool valid = true;

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

        private bool ValidateAttribute(Type declaringClass, Type attribute, string description, bool strict)
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
            else if (attribute.IsClass && attribute != typeof(string) && attribute != typeof(KalturaOTTFile) && !attribute.IsSubclassOf(typeof(KalturaOTTObject)) && attribute != typeof(KalturaOTTObject))
            {
                logError("Error", declaringClass, string.Format("{0} object must inherit from KalturaOTTObject (or something that extends it)", description));
                valid = false;
            }

            return valid;
        }

        private bool ValidateFilter(Type type, bool strict)
        {
            bool valid = true;

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

        private bool ValidateEnum(Type type, bool strict)
        {
            return true;
        }

        private bool ValidateService(Type controller, bool strict)
        {
            if (controller.IsAbstract) { return true; }

            bool valid = true;
            string serviceId = SchemeManager.getServiceId(controller);

            ServiceAttribute serviceAttribute = controller.GetCustomAttribute<ServiceAttribute>();
            if (serviceAttribute == null)
            {
                return !strict;
            }

            var hasValidActions = false;
            var methods = controller.GetMethods().Where(x => x.IsPublic && x.DeclaringType.Namespace == "WebAPI.Controllers").OrderBy(method => method.Name).ToList();

            if (methods.Count == 0)
            {
                hasValidActions = true;
            }
            else
            {
                foreach (MethodInfo method in methods)
                {
                    hasValidActions = ValidateMethod(method, strict) || hasValidActions;
                };
            }

            return valid && hasValidActions;
        }

        private bool ValidateParameter(MethodInfo action, ParameterInfo param, bool strict)
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

        private List<string> ValidateFileStructure(Type objectType)
        {
            var problems = new List<string>();

            var methodsInObject = objectType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                   .Where(x => (x.DeclaringType == null || x.DeclaringType == objectType) && !x.IsSpecialName && !x.IsFamily)
                                   .ToList();

            var getDefaultOrderByValueMethod = methodsInObject.FirstOrDefault(x => x.Name == "GetDefaultOrderByValue");
            if (getDefaultOrderByValueMethod != null)
            {
                methodsInObject.Remove(getDefaultOrderByValueMethod);
            }

            if (methodsInObject.Count > 0)
            {
                problems.Add("have methods");
            }

            //var filePath = basePath + objectType.Namespace.Replace(".", "\\");
            var path = getFilePath(objectType); // $"{filePath}\\{objectType.Name}.cs";
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                problems.Add("not in a separate file");
            }
            else
            {
                var fileLines = File.ReadLines(path).ToList();
                if (fileLines.Count(line => line.Contains("class") || line.Contains("enum")) > 1)
                {
                    problems.Add("file contains more than one object");
                }
            }

            foreach (var problem in problems)
            {
                logError("Error", objectType, problem);
            }

            return problems;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="problematicModels">key: kaltura objectname, value:comma separated errors</param>
        private void SaveFilesStructureValidationErrorsToExcel(Dictionary<string, string> objectsToErrors, List<Type> allObjects)
        {
            var fileName = $"{FILES_STRUCTURE_ERRORS_EXCEL_FILE_NAME}.xlsx";
            var filePath = $"..\\..\\..\\{fileName}";
            var writeStream = new FileInfo(filePath).Create();

            if (objectsToErrors.Count == 0)
            {
                File.Delete(fileName);
                Console.WriteLine("[Info]: Excel models with problems was deleted");
                return;
            }

            Console.WriteLine("[Info]: starting to Save To Excel models with problems");

            var lowerObjects = allObjects.OrderBy(x => x.Name).ToDictionary(x => x.Name.Substring(7).ToLower());
            var lowerControllers = GetLoweredControllersToParameters();

            var aggregatedErrorsByController = new Dictionary<string, Dictionary<string, string>>();
            objectToRelatedController = new Dictionary<string, string>();

            objectsToErrors = objectsToErrors.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
            foreach (var objectErrors in objectsToErrors)
            {
                var controllerName = GetRelatedControllerName(lowerControllers, lowerObjects, objectErrors.Key);
                if (aggregatedErrorsByController.ContainsKey(controllerName))
                {
                    aggregatedErrorsByController[controllerName].Add(objectErrors.Key, objectErrors.Value);
                }
                else
                {
                    aggregatedErrorsByController.Add(controllerName, new Dictionary<string, string>() { { objectErrors.Key, objectErrors.Value } });
                }
            }

            var columnColor = Color.FromArgb(0, 204, 255);
            using (var pack = new ExcelPackage(writeStream))
            {
                var excelWorksheet = pack.Workbook.Worksheets.Add("Objects to handle");
                SetExcelColumn(columnColor, excelWorksheet, 1, "Developer");
                SetExcelColumn(columnColor, excelWorksheet, 2, "Service");
                SetExcelColumn(columnColor, excelWorksheet, 3, "Object");
                SetExcelColumn(columnColor, excelWorksheet, 4, "Problems");
                var rowIndex = 2;

                foreach (var fileToObjectsErrors in aggregatedErrorsByController)
                {
                    var controller = fileToObjectsErrors.Key;
                    var values = fileToObjectsErrors.Value.Select(x => new Tuple<string, string, string>(controller, x.Key, x.Value));
                    excelWorksheet.Cells[rowIndex, 2].LoadFromCollection(values, false);
                    rowIndex += fileToObjectsErrors.Value.Count;
                }

                pack.SaveAs(writeStream);
            }

            Console.WriteLine("[Info]: Save To Excel models with problems is done!!");
        }

        private Dictionary<string, HashSet<string>> GetLoweredControllersToParameters()
        {
            var controllers = Holder.Controllers.ToList();
            var lowerControllers = new Dictionary<string, HashSet<string>>(controllers.Count);
            for (int i = 0; i < controllers.Count; i++)
            {
                var controllerName = controllers[i].Name;
                var controllerIndex = controllerName.LastIndexOf("Controller");
                if (controllerIndex > 0)
                {
                    controllerName = controllerName.Remove(controllerIndex);
                }

                var lowerContollerName = controllerName.ToLower();
                lowerControllers.Add(lowerContollerName, new HashSet<string>());

                var methods = controllers[i].GetMethods().Where(x => x.IsPublic && x.DeclaringType.Namespace == "WebAPI.Controllers").OrderBy(method => method.Name).ToList();
                foreach (var method in methods)
                {
                    lowerControllers[lowerContollerName].Add(method.ReturnType.Name);
                    var parameters = method.GetParameters();
                    foreach (var parameter in parameters)
                    {
                        if (!lowerControllers[lowerContollerName].Contains(parameter.ParameterType.Name))
                        {
                            lowerControllers[lowerContollerName].Add(parameter.ParameterType.Name);
                        }
                    }
                }
            }

            return lowerControllers.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
        }

        private static void SetExcelColumn(Color columnColor, ExcelWorksheet excelWorksheet, int columnIndex, string columnName)
        {
            excelWorksheet.Cells[1, columnIndex].Value = columnName;
            excelWorksheet.Cells[1, columnIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
            excelWorksheet.Cells[1, columnIndex].Style.Font.Bold = true;
            excelWorksheet.Cells[1, columnIndex].Style.Fill.BackgroundColor.SetColor(columnColor);
        }

        private string GetRelatedControllerName(Dictionary<string, HashSet<string>> controllersToParameters, Dictionary<string, Type> lowerObjects, string objectName)
        {
            if (objectToRelatedController.ContainsKey(objectName))
            {
                return objectToRelatedController[objectName];
            }

            // remove "kaltura" from name and get lowered name
            var lowerObjectName = objectName.Substring(7).ToLower();
            if (controllersToParameters.ContainsKey(lowerObjectName))
            {
                objectToRelatedController.Add(objectName, lowerObjectName);
                return lowerObjectName;
            }

            // find which controllers are using this object
            var controllerParameter = controllersToParameters.Where(x => x.Value.Contains(objectName)).OrderByDescending(x => x.Key.Length).FirstOrDefault();
            if (!controllerParameter.Equals(default(KeyValuePair<string, HashSet<string>>)))
            {
                objectToRelatedController.Add(objectName, controllerParameter.Key);
                return controllerParameter.Key;
            }

            // find the longest controller name which contain the object name
            var longestControllerName = controllersToParameters.Keys.Where(x => lowerObjectName.Contains(x)).OrderByDescending(x => x.Length).FirstOrDefault();
            if (!string.IsNullOrEmpty(longestControllerName))
            {
                objectToRelatedController.Add(objectName, longestControllerName);
                return longestControllerName;
            }

            if (lowerObjects.ContainsKey(lowerObjectName))
            {
                var currObjectType = lowerObjects[lowerObjectName];

                if (currObjectType.Namespace == "WebAPI.Models.General")
                {
                    objectToRelatedController.Add(objectName, "general");
                    return "general";
                }

                if (currObjectType.BaseType != null && currObjectType.BaseType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    var relatedControllerName = GetRelatedControllerName(controllersToParameters, lowerObjects, currObjectType.BaseType.Name);
                    if (relatedControllerName != UNKNOWN_CONTROLLER)
                    {
                        objectToRelatedController.Add(objectName, relatedControllerName);
                        return relatedControllerName;
                    }
                }

                // find all types which hold a property of current type and get related controller
                var relatedTypes = lowerObjects.Values.Where(x => x.GetProperties().Any(p => p.PropertyType == currObjectType)).OrderByDescending(o => o.Name.Length).ToList();
                foreach (var relatedType in relatedTypes)
                {
                    var relatedControllerName = GetRelatedControllerName(controllersToParameters, lowerObjects, relatedType.Name);
                    if (relatedControllerName != UNKNOWN_CONTROLLER)
                    {
                        objectToRelatedController.Add(objectName, relatedControllerName);
                        return relatedControllerName;
                    }
                }
            }

            objectToRelatedController.Add(objectName, UNKNOWN_CONTROLLER);
            return UNKNOWN_CONTROLLER;
        }

        private bool hasValidationException(ICustomAttributeProvider attributeProvider, SchemeValidationType type)
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

        private string getApiName(PropertyInfo property)
        {
            DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>();
            if (dataMember == null)
                return null;

            return dataMember.Name;
        }

        private void logError(string category, Type type, string message, [CallerLineNumber] int lineNumber = 0)
        {
            Console.WriteLine(string.Format("{0}: {1}: [{2,3}] {3}", getFilePath(type), category, lineNumber, message));
        }

        private Dictionary<string, string> getFilesMap()
        {
            if (map != null)
                return map;

            var webAPIProjectPath = Path.Combine(GetProjectDir(), "WebApi");
            var allfiles = Directory.GetFiles(webAPIProjectPath, "*.cs", SearchOption.AllDirectories);

            map = new Dictionary<string, string>();

            if (allfiles != null && allfiles.Length > 0)
            {
                foreach (var filePath in allfiles)
                {
                    string type = filePath.Substring(filePath.LastIndexOf(@"\") + 1).Replace(".cs", "");
                    if (!map.ContainsKey(type))
                        map.Add(type, filePath);
                }
            }

            return map;
        }

        private string getFilePath(Type type)
        {
            Dictionary<string, string> map = getFilesMap();
            if (map.ContainsKey(type.Name))
                return map[type.Name];

            return "";
        }

        private string FirstCharacterToUpper(string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsUpper(str, 0))
                return str;

            return Char.ToUpper(str[0]) + str.Substring(1);
        }

        private PropertyInfo getObjectsProperty(Type listResponseType)
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

        private string GetProjectDir()
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string prefix = "file:///";
            if (assembly.CodeBase.StartsWith(prefix))
            {
                FileInfo dll = new FileInfo(assembly.Location);
                var projectDir = dll.Directory.Parent.Parent.Parent.Parent.Parent;
                return Path.Combine(projectDir.FullName, "Core");
            }
            else
            {
                throw new Exception("Could not ascertain assembly filename", null);
            }
        }

        private XmlDocument GetProjectXml()
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
    }
}