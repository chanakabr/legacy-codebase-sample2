using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Controllers;
using System.Net;
using Validator.Managers.Scheme;
using Validator;
using WebAPI.Filters;

namespace Reflector
{
    class DataModel : Base
    {
        public static string GetDataModelCSFilePath()
        {
            var currentLocation = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDir = Directory.GetParent(currentLocation).Parent.Parent.Parent.Parent.Parent;
            var filePath = Path.Combine(solutionDir.FullName, "Core", "WebAPI", "Reflection", "DataModel.cs");
            return filePath;
        }

        public DataModel() : base(GetDataModelCSFilePath())
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        }

        protected override void writeHeader()
        {
            file.WriteLine("// NOTICE: This is a generated file, to modify it, edit Program.cs in Reflector project");
            file.WriteLine("// disable compiler warning due to generation of empty usages ot unused vars");
            file.WriteLine("// ReSharper disable RedundantUsingDirective");
            file.WriteLine("// ReSharper disable CheckNamespace");
            file.WriteLine("// ReSharper disable NotAccessedVariable");
            file.WriteLine("// ReSharper disable UnusedVariable");
            file.WriteLine("// ReSharper disable RedundantAssignment");
            file.WriteLine("// ReSharper disable PossibleMultipleEnumeration");
            file.WriteLine("// ReSharper disable UnusedParameter.Local");
            file.WriteLine("// ReSharper disable PossibleNullReferenceException");
            file.WriteLine("// ReSharper disable AssignNullToNotNullAttribute");
            file.WriteLine("// ReSharper disable BadChildStatementIndent");
            file.WriteLine("// ReSharper disable StringLiteralTypo");
            file.WriteLine("// ReSharper disable RedundantArgumentDefaultValue");
            file.WriteLine("// ReSharper disable ExpressionIsAlwaysNull");
            file.WriteLine("#pragma warning disable 168");
            file.WriteLine("#pragma warning disable 1522");
            file.WriteLine("#pragma warning disable 612");
            file.WriteLine("using System;");
            file.WriteLine("using System.Net;");
            file.WriteLine("using System.Collections.Generic;");
            file.WriteLine("using System.Linq;");
            file.WriteLine("using System.Reflection;");
            file.WriteLine("using System.Web;");
            file.WriteLine("using WebAPI.Controllers;");
            file.WriteLine("using WebAPI.Exceptions;");
            file.WriteLine("using WebAPI.Filters;");
            file.WriteLine("using WebAPI.Managers;");
            file.WriteLine("using WebAPI.Managers.Scheme;");
            file.WriteLine("using WebAPI.Models.MultiRequest;");
            file.WriteLine("using TVinciShared;");
            file.WriteLine("using KalturaRequestContext;");
            file.WriteLine("using WebAPI.ModelsValidators;");
            file.WriteLine("using WebAPI.ObjectsConvertor.Extensions;");

            types.GroupBy(type => type.Namespace)
                 .Select(group => group.First().Namespace)
                 .ToList()
                 .ForEach(name => file.WriteLine("using " + name + ";"));

            file.WriteLine("");
            file.WriteLine("namespace WebAPI.Reflection");
            file.WriteLine("{");
            file.WriteLine("    public class DataModel");
            file.WriteLine("    {");
        }

        protected override void writeBody()
        {
            wrtiePropertyApiName();
            WriteExecAction();
            WriteGetMethodParams();
            wrtieGetFailureHttpCode();
            WriteCustomStatusCodeResponseEnabled();
        }

        protected override void writeFooter()
        {
            file.WriteLine("    }");
            file.WriteLine("}");
        }

        private void wrtieGetFailureHttpCode()
        {
            file.WriteLine("        public static HttpStatusCode? getFailureHttpCode(string service, string action)");
            file.WriteLine("        {");
            file.WriteLine("            service = service.ToLower();");
            file.WriteLine("            action = action.ToLower();");
            file.WriteLine("            switch (service)");
            file.WriteLine("            {");

            bool needed;

            foreach (Type controller in controllers)
            {
                ServiceAttribute serviceAttribute = controller.GetCustomAttribute<ServiceAttribute>(true);
                if (serviceAttribute == null)
                {
                    continue;
                }

                needed = false;
                List<MethodInfo> actions = controller.GetMethods().ToList();
                actions.Sort(new MethodInfoComparer());

                foreach (MethodInfo action in actions)
                {
                    if (action.DeclaringType != controller)
                        continue;

                    FailureHttpCodeAttribute failureHttpCodeAttribute = action.GetCustomAttribute<FailureHttpCodeAttribute>(true);
                    if (failureHttpCodeAttribute != null)
                    {
                        needed = true;
                    }
                }

                if (!needed)
                    continue;

                file.WriteLine("                case \"" + serviceAttribute.Name.ToLower() + "\":");
                file.WriteLine("                    switch(action)");
                file.WriteLine("                    {");

                foreach (MethodInfo action in actions)
                {
                    if (action.DeclaringType != controller)
                        continue;

                    ActionAttribute actionAttribute = action.GetCustomAttribute<ActionAttribute>(true);
                    FailureHttpCodeAttribute failureHttpCodeAttribute = action.GetCustomAttribute<FailureHttpCodeAttribute>(true);
                    if (failureHttpCodeAttribute != null && actionAttribute != null)
                    {
                        file.WriteLine("                        case \"" + actionAttribute.Name.ToLower() + "\":");
                        file.WriteLine("                            return HttpStatusCode." + Enum.GetName(typeof(HttpStatusCode), failureHttpCodeAttribute.HttpStatusCode) + ";");
                        file.WriteLine("                            ");
                    }
                }

                file.WriteLine("                    }");
                file.WriteLine("                    break;");
                file.WriteLine("                    ");
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            return null;");
            file.WriteLine("        }");
            file.WriteLine("        ");
        }
        
        private void WriteGetMethodParams()
        {
            List<PropertyInfo> schemeArgumentProperties = typeof(SchemeArgumentAttribute).GetProperties().ToList();

            file.WriteLine("        public static Dictionary<string, MethodParam> getMethodParams(string service, string action)");
            file.WriteLine("        {");
            file.WriteLine("            service = service.ToLower();");
            file.WriteLine("            action = action.ToLower();");
            file.WriteLine("            Dictionary<string, MethodParam> ret = new Dictionary<string, MethodParam>();");
            file.WriteLine("            Version currentVersion = (Version)HttpContext.Current.Items[RequestContextConstants.REQUEST_VERSION];");
            file.WriteLine("            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);");
            file.WriteLine("            string paramName;");
            file.WriteLine("            string newParamName = null;");

            file.WriteLine("            if(isOldVersion)");
            file.WriteLine("            {");

            file.WriteLine("                switch (service)");
            file.WriteLine("                {");

            // run over all controllers to get all OldStandardActions methods names 
            foreach (Type controller in controllers)
            {
                List<MethodInfo> actions = controller.GetMethods().ToList();
                bool hasOldVersionActions = false;

                // run over all methods and check if there are any OldStandardActions (stop if exists)
                foreach (MethodInfo action in actions)
                {
                    if (action.DeclaringType != controller)
                    {
                        continue;
                    }

                    OldStandardActionAttribute oldStandardActionAttribute = action.GetCustomAttribute<OldStandardActionAttribute>(true);
                    if (oldStandardActionAttribute != null)
                    {
                        hasOldVersionActions = true;
                        break;
                    }
                }

                if (!hasOldVersionActions)
                {
                    continue;
                }

                ServiceAttribute serviceAttribute = controller.GetCustomAttribute<ServiceAttribute>(true);
                if (serviceAttribute == null)
                {
                    continue;
                }

                file.WriteLine("                    case \"" + serviceAttribute.Name.ToLower() + "\":");
                file.WriteLine("                        switch(action)");
                file.WriteLine("                        {");

                actions.Sort(new MethodInfoComparer());

                foreach (MethodInfo action in actions)
                {
                    if (action.DeclaringType != controller)
                    {
                        continue;
                    }

                    ActionAttribute actionAttribute = action.GetCustomAttribute<ActionAttribute>(true);
                    if (actionAttribute == null)
                    {
                        continue;
                    }

                    OldStandardActionAttribute oldStandardActionAttribute = action.GetCustomAttribute<OldStandardActionAttribute>(true);
                    if (oldStandardActionAttribute == null)
                    {
                        continue;
                    }
                    file.WriteLine("                            case \"" + oldStandardActionAttribute.oldName.ToLower() + "\":");
                    file.WriteLine("                                action = \"" + actionAttribute.Name.ToLower() + "\";");
                    file.WriteLine("                                break;");
                    file.WriteLine("                                ");
                }
                file.WriteLine("                        }");
                file.WriteLine("                        break;");
                file.WriteLine("                        ");
            }
            file.WriteLine("                }");
            file.WriteLine("            }");

            file.WriteLine("            switch (service)");
            file.WriteLine("            {");

            foreach (Type controller in controllers)
            {
                var serviceAttribute = controller.GetCustomAttribute<ServiceAttribute>(true);
                if (serviceAttribute == null) { continue; }
                
                file.WriteLine("                case \"" + serviceAttribute.Name.ToLower() + "\":");
                file.WriteLine("                    switch(action)");
                file.WriteLine("                    {");

                List<MethodInfo> actions = controller.GetMethods().ToList();
                actions.Sort(new MethodInfoComparer());

                foreach (MethodInfo action in actions)
                {
                    if (action.DeclaringType != controller) { continue; }
                    
                    var actionAttribute = action.GetCustomAttribute<ActionAttribute>(true);
                    if (actionAttribute == null) { continue; }

                    WriteActionParams(actionAttribute, action, schemeArgumentProperties, serviceAttribute);
                }

                file.WriteLine("                    }");
                file.WriteLine("                    break;");
                file.WriteLine("                    ");
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            throw new RequestParserException(RequestParserException.INVALID_ACTION, service, action);");
            file.WriteLine("        }");
            file.WriteLine("        ");
        }
        
        private void WriteActionParams(ActionAttribute actionAttribute, MethodInfo action, List<PropertyInfo> schemeArgumentProperties, ServiceAttribute serviceAttribute, Dictionary<string, bool> optionalParameters = null)
        {
            file.WriteLine("                        case \"" + actionAttribute.Name.ToLower() + "\":");

            var parameters = action.GetParameters();
            var schemaArguments = action.GetCustomAttributes<SchemeArgumentAttribute>();
            var oldStandardAttributesMap = GetOldStandardAttributesMap(action);

            foreach (var parameter in parameters)
            {
                var isParameterOptional = SchemeManager.IsParameterOptional(parameter, optionalParameters);
                if (!isParameterOptional.HasValue)
                {
                    continue;
                }

                string paramName = "paramName";
                bool hasOldStandard = false;
                if (oldStandardAttributesMap.ContainsKey(parameter.Name))
                {
                    hasOldStandard = true;
                }
                
                if (hasOldStandard)
                {
                    file.WriteLine("                            paramName = \"" + parameter.Name + "\";");
                    file.WriteLine("                            newParamName = null;");
                    
                    foreach (var oldStandardArgumentAttribute in oldStandardAttributesMap[parameter.Name])
                    {
                        if (!string.IsNullOrEmpty(oldStandardArgumentAttribute.Key))
                        {
                            file.WriteLine("                            if(isOldVersion || currentVersion.CompareTo(new Version(\"" + oldStandardArgumentAttribute.Key + "\")) < 0)");
                            file.WriteLine("                            {");
                            file.WriteLine("                                paramName = \"" + oldStandardArgumentAttribute.Value.oldName + "\";");
                            file.WriteLine("                                newParamName = \"" + oldStandardArgumentAttribute.Value.newName + "\";");
                            file.WriteLine("                            }");
                        }
                    }
                    foreach (var oldStandardArgumentAttribute in oldStandardAttributesMap[parameter.Name])
                    {
                        if (string.IsNullOrEmpty(oldStandardArgumentAttribute.Key))
                        {
                            file.WriteLine("                            if(isOldVersion)");
                            file.WriteLine("                            {");
                            file.WriteLine("                                paramName = \"" + oldStandardArgumentAttribute.Value.oldName + "\";");
                            file.WriteLine("                                newParamName = \"" + oldStandardArgumentAttribute.Value.newName + "\";");
                            file.WriteLine("                            }");
                        }
                    }
                }
                else
                {
                    paramName = "\"" + parameter.Name + "\"";
                }

                file.WriteLine("                            ret.Add(" + paramName + ", new MethodParam(){");
                file.WriteLine("                                NewName = newParamName,");
                if (isParameterOptional.Value)
                {
                    file.WriteLine("                                IsOptional = true,");
                    file.WriteLine("                                DefaultValue = " + SchemeManager.VarToString(parameter.DefaultValue) + ",");
                }

                // write param type
                if (parameter.ParameterType.IsGenericType)
                {
                    if (parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        file.WriteLine("                                IsNullable = true,");
                        Type nullableType = Nullable.GetUnderlyingType(parameter.ParameterType);
                        file.WriteLine("                                Type = typeof(" + nullableType.Name + "),");
                        if (nullableType.IsEnum)
                        {
                            file.WriteLine("                                IsEnum = true,");
                        }
                    }
                    else if (parameter.ParameterType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        file.WriteLine("                                IsList = true,");
                        file.WriteLine("                                GenericType = typeof(" + GetTypeName(parameter.ParameterType.GetGenericArguments()[0]) + "),");
                        file.WriteLine("                                Type = typeof(List<" + GetTypeName(parameter.ParameterType.GetGenericArguments()[0]) + ">),");
                    }
                    else if (parameter.ParameterType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>))
                    {
                        file.WriteLine("                                IsMap = true,");
                        file.WriteLine("                                GenericType = typeof(" + GetTypeName(parameter.ParameterType.GetGenericArguments()[1]) + "),");
                        file.WriteLine("                                Type = typeof(SerializableDictionary<string, " + GetTypeName(parameter.ParameterType.GetGenericArguments()[1]) + ">),");
                    }
                    else if (parameter.ParameterType.IsSubclassOf(typeof(KalturaOTTObject)))
                    {
                        file.WriteLine("                                IsKalturaObject = true,");
                        file.WriteLine("                                Type = typeof(" + GetTypeName(parameter.ParameterType) + "<>),");
                    }
                }
                else if (parameter.ParameterType.IsEnum)
                {
                    file.WriteLine("                                IsEnum = true,");
                    file.WriteLine("                                Type = typeof(" + parameter.ParameterType.Name + "),");
                }
                else
                {
                    if (parameter.ParameterType.IsSubclassOf(typeof(KalturaOTTObject)))
                    {
                        file.WriteLine("                                IsKalturaObject = true,");
                        if (typeof(KalturaMultilingualString).IsAssignableFrom(parameter.ParameterType))
                        {
                            file.WriteLine("                                IsKalturaMultilingualString = true,");
                        }
                    }
                    else if (parameter.ParameterType == typeof(DateTime))
                    {
                        file.WriteLine("                                IsDateTime = true,");
                    }
                    file.WriteLine("                                Type = typeof(" + GetTypeName(parameter.ParameterType) + "),");
                }

                foreach (SchemeArgumentAttribute schemaArgument in schemaArguments)
                {
                    if (schemaArgument.Name.Equals(parameter.Name))
                    {
                        file.WriteLine("                                SchemeArgument = new RuntimeSchemeArgumentAttribute(\"" + parameter.Name + "\", \"" + serviceAttribute.Name + "\", \"" + actionAttribute.Name + "\") {");
                        schemeArgumentProperties.ForEach(schemeArgumentProperty =>
                        {
                            object val = schemeArgumentProperty.GetValue(schemaArgument);
                            if (val != null)
                            {
                                if (schemeArgumentProperty.Name != "Name" && schemeArgumentProperty.Name != "TypeId")
                                {
                                    if (schemeArgumentProperty.PropertyType == typeof(bool))
                                    {
                                        file.WriteLine("                                    " + schemeArgumentProperty.Name + " = " + val.ToString().ToLower() + ",");
                                    }
                                    else if (schemeArgumentProperty.PropertyType == typeof(int))
                                    {
                                        if ((int)val != int.MinValue && (int)val != int.MaxValue)
                                        {
                                            file.WriteLine("                                    " + schemeArgumentProperty.Name + " = " + val.ToString().ToLower() + ",");
                                        }
                                    }
                                    else if (schemeArgumentProperty.PropertyType == typeof(long))
                                    {
                                        if ((long)val != long.MinValue && (long)val != long.MaxValue)
                                        {
                                            file.WriteLine("                                    " + schemeArgumentProperty.Name + " = " + val.ToString().ToLower() + ",");
                                        }
                                    }
                                    else if (schemeArgumentProperty.PropertyType == typeof(double))
                                    {
                                        if ((double)val != double.MinValue && (double)val != double.MaxValue)
                                        {
                                            file.WriteLine("                                    " + schemeArgumentProperty.Name + " = " + val.ToString().ToLower() + ",");
                                        }
                                    }
                                    else if (schemeArgumentProperty.PropertyType == typeof(float))
                                    {
                                        if ((float)val != float.MinValue && (float)val != float.MaxValue)
                                        {
                                            file.WriteLine("                                    " + schemeArgumentProperty.Name + " = " + val.ToString().ToLower() + ",");
                                        }
                                    }
                                    else
                                    {
                                        file.WriteLine("                                    " + schemeArgumentProperty.Name + " = " + val + ",");
                                    }
                                }
                            }
                        });
                        file.WriteLine("                                },");
                    }
                }
                file.WriteLine("                            });");
            }

            file.WriteLine("                            return ret;");
            file.WriteLine("                            ");
        }

        private Dictionary<string, Dictionary<string, OldStandardArgumentAttribute>> GetOldStandardAttributesMap(MethodInfo action)
        {
            var oldStandardArgumentAttributes = action.GetCustomAttributes<OldStandardArgumentAttribute>(true);
            var oldStandardAttributesMap = new Dictionary<string, Dictionary<string, OldStandardArgumentAttribute>>();
            foreach (var oldStandardArgumentAttribute in oldStandardArgumentAttributes)
            {
                if (!oldStandardAttributesMap.ContainsKey(oldStandardArgumentAttribute.newName))
                {
                    oldStandardAttributesMap.Add(oldStandardArgumentAttribute.newName, new Dictionary<string, OldStandardArgumentAttribute>());
                }

                var sinceVersion = oldStandardArgumentAttribute.sinceVersion ?? string.Empty;
                oldStandardAttributesMap[oldStandardArgumentAttribute.newName].Add(sinceVersion, oldStandardArgumentAttribute);
            }

            return oldStandardAttributesMap;
        }

        private void WriteExecAction(MethodInfo action, bool indent, string permissionActionName = null)
        {
            string tab = indent ? "    " : "";
            Type controller = action.DeclaringType;
            ServiceAttribute serviceAttribute = controller.GetCustomAttribute<ServiceAttribute>(true);
            ActionAttribute actionAttribute = action.GetCustomAttribute<ActionAttribute>(true);

            BlockHttpMethodsAttribute blockHttpMethods = action.GetCustomAttribute<BlockHttpMethodsAttribute>(true);
            if (blockHttpMethods != null && blockHttpMethods.HttpMethods != null && blockHttpMethods.HttpMethods.Count > 0)
            {
                List<string> conditions = new List<string>();
                foreach (string httpMethod in blockHttpMethods.HttpMethods)
                {
                    conditions.Add("HttpContext.Current.Request.GetHttpMethod().ToLower() == \"" + httpMethod.ToLower() + "\"");
                }
                file.WriteLine(tab + "                            if(" + String.Join(" || ", conditions) + ")");
                file.WriteLine(tab + "                            {");
                file.WriteLine(tab + "                                throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.GetHttpMethod().ToUpper());");
                file.WriteLine(tab + "                            }");
            }

            ApiAuthorizeAttribute authorization = action.GetCustomAttribute<ApiAuthorizeAttribute>(true);
            if (authorization != null)
            {
                if (permissionActionName == null)
                {
                    permissionActionName = actionAttribute.Name;
                }

                file.WriteLine(tab + "                            RolesManager.ValidateActionPermitted(\"" + serviceAttribute.Name + "\", \"" + permissionActionName + "\", WebAPI.Managers.eKSValidation." + authorization.KSValidation + ");");
            }

            SchemeServeAttribute serve = action.GetCustomAttribute<SchemeServeAttribute>(true);
            if (serve != null)
            {
                file.WriteLine(tab + "                            HttpContext.Current.Items[RequestContextConstants.REQUEST_SERVE_CONTENT_TYPE] = \"" + serve.ContentType + "\";");
            }

            string args = String.Join(", ", action.GetParameters()
                .Select(paramInfo => "(" + GetTypeName(paramInfo.ParameterType, true) + ") methodParams[" + paramInfo.Position + "]"));

            if (action.IsGenericMethod)
            {
                file.WriteLine(tab + "                            return ServiceController.ExecGeneric(typeof(" + controller.Name + ").GetMethod(\"" + action.Name + "\"), methodParams);");
            }
            else if (action.ReturnType == typeof(void))
            {
                file.WriteLine(tab + "                            " + controller.Name + "." + action.Name + "(" + args + ");");
                file.WriteLine(tab + "                            return null;");
            }
            else
            {
                file.WriteLine(tab + "                            return " + controller.Name + "." + action.Name + "(" + args + ");");
            }
        }

        private void WriteExecAction()
        {
            file.WriteLine("        public static object execAction(string service, string action, List<object> methodParams)");
            file.WriteLine("        {");
            file.WriteLine("            service = service.ToLower();");
            file.WriteLine("            action = action.ToLower();");
            file.WriteLine("            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion();");
            file.WriteLine("            switch (service)");
            file.WriteLine("            {");

            OldStandardActionAttribute oldStandardActionAttribute;
            
            foreach (Type controller in controllers)
            {
                var controllerName = controller.Name;
                ServiceAttribute serviceAttribute = controller.GetCustomAttribute<ServiceAttribute>(true);
                if (serviceAttribute == null)
                {
                    continue;
                }
                var serviceName = serviceAttribute.Name.ToLower();
                file.WriteLine("                case \"" + serviceName  + "\":");
                file.WriteLine("                    switch(action)");
                file.WriteLine("                    {");

                List<MethodInfo> actions = controller.GetMethods().ToList();
                actions.Sort(new MethodInfoComparer());

                List<string> leftOldStandard = new List<string>();
                List<string> doneOldStandard = new List<string>();

                foreach (MethodInfo action in actions)
                {
                    if (action.DeclaringType != controller)
                    {
                        continue;
                    }

                    ActionAttribute actionAttribute = action.GetCustomAttribute<ActionAttribute>(true);
                    if (actionAttribute == null)
                    {
                        continue;
                    }
                    doneOldStandard.Add(actionAttribute.Name);
                    if (leftOldStandard.Contains(actionAttribute.Name))
                    {
                        leftOldStandard.Remove(actionAttribute.Name);
                    }

                    oldStandardActionAttribute = action.GetCustomAttribute<OldStandardActionAttribute>(true);
                    if (oldStandardActionAttribute != null && !doneOldStandard.Contains(oldStandardActionAttribute.oldName))
                    {
                        leftOldStandard.Add(oldStandardActionAttribute.oldName);
                    }

                    file.WriteLine("                        case \"" + actionAttribute.Name.ToLower() + "\":");

                    foreach (MethodInfo oldAction in actions)
                    {
                        oldStandardActionAttribute = oldAction.GetCustomAttribute<OldStandardActionAttribute>(true);
                        if (oldStandardActionAttribute != null && oldStandardActionAttribute.oldName == actionAttribute.Name)
                        {
                            file.WriteLine("                            if(isOldVersion)");
                            file.WriteLine("                            {");
                            WriteExecAction(oldAction, true, oldStandardActionAttribute.oldName);
                            file.WriteLine("                            }");
                            break;
                        }
                    }

                    WriteExecAction(action, false);
                    file.WriteLine("                            ");
                }

                foreach (string oldStandardName in leftOldStandard)
                {
                    file.WriteLine("                        case \"" + oldStandardName.ToLower() + "\":");

                    foreach (MethodInfo oldAction in actions)
                    {
                        oldStandardActionAttribute = oldAction.GetCustomAttribute<OldStandardActionAttribute>(true);
                        if (oldStandardActionAttribute != null && oldStandardActionAttribute.oldName == oldStandardName)
                        {
                            file.WriteLine("                            if(isOldVersion)");
                            file.WriteLine("                            {");
                            WriteExecAction(oldAction, true, oldStandardActionAttribute.oldName);
                            file.WriteLine("                            }");
                            break;
                        }
                    }

                    file.WriteLine("                            break;");
                    file.WriteLine("                            ");
                }

                file.WriteLine("                    }");
                file.WriteLine("                    break;");
                file.WriteLine("                    ");
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            throw new RequestParserException(RequestParserException.INVALID_ACTION, service, action);");
            file.WriteLine("        }");
            file.WriteLine("        ");
        }

        private void wrtiePropertyApiName()
        {
            file.WriteLine("        public static string getApiName(PropertyInfo property)");
            file.WriteLine("        {");
            file.WriteLine("            switch (property.DeclaringType.Name)");
            file.WriteLine("            {");

            bool needed = false;

            foreach (Type type in types)
            {
                needed = false;

                List<PropertyInfo> properties = type.GetProperties().Where(x => x.DeclaringType == type).ToList();
                properties.Sort(new PropertyInfoComparer());
                var propertyToDataMemberAttribute = new Dictionary<string, string>();
                foreach (PropertyInfo property in properties)
                {
                    DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>();
                    if (dataMember != null && !dataMember.Name.Equals(property.Name))
                    {
                        propertyToDataMemberAttribute.Add(property.Name, dataMember.Name);
                        needed = true;
                    }
                }

                if (!needed)
                    continue;

                file.WriteLine("                case \"" + type.Name + "\":");
                file.WriteLine("                    switch(property.Name)");
                file.WriteLine("                    {");

                foreach (var propertyToDataMember in propertyToDataMemberAttribute)
                {
                    file.WriteLine("                        case \"" + propertyToDataMember.Key + "\":");
                    file.WriteLine("                            return \"" + propertyToDataMember.Value + "\";");
                }

                file.WriteLine("                    }");
                file.WriteLine("                    break;");
                file.WriteLine("                    ");
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            return property.Name;");
            file.WriteLine("        }");
            file.WriteLine("        ");
        }

        private void wrtieTypeOldMembersCase(Type type)
        {
            List<PropertyInfo> properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy).ToList();
            properties.Sort(new PropertyInfoComparer());

            List<PropertyInfo> oldStandardProperties = new List<PropertyInfo>();
            foreach (PropertyInfo property in properties)
            {
                List<OldStandardPropertyAttribute> oldStandardPropertyAttributes = property.GetCustomAttributes<OldStandardPropertyAttribute>().ToList();
                if (oldStandardPropertyAttributes.Count > 0)
                {
                    oldStandardProperties.Add(property);
                }
            }

            if (oldStandardProperties.Count == 0)
                return;

            file.WriteLine("                case \"" + type.Name + "\":");
            file.WriteLine("                    ret = new Dictionary<string, string>() { ");
            foreach (PropertyInfo property in oldStandardProperties)
            {
                List<OldStandardPropertyAttribute> attributes = property.GetCustomAttributes<OldStandardPropertyAttribute>().ToList();

                foreach (OldStandardPropertyAttribute attribute in attributes)
                {
                    if (attribute.sinceVersion == null)
                    {
                        DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>();

                        string newName = property.Name;
                        if (dataMember != null)
                        {
                            newName = dataMember.Name;
                        }

                        file.WriteLine("                        {\"" + newName + "\", \"" + attribute.oldName + "\"},");
                    }
                }
            }
            file.WriteLine("                    };");

            foreach (PropertyInfo property in oldStandardProperties)
            {
                List<OldStandardPropertyAttribute> attributes = property.GetCustomAttributes<OldStandardPropertyAttribute>().ToList();

                foreach (OldStandardPropertyAttribute attribute in attributes)
                {
                    if (attribute.sinceVersion != null)
                    {
                        DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>();

                        string newName = property.Name;
                        if (dataMember != null)
                        {
                            newName = dataMember.Name;
                        }

                        file.WriteLine("                    if (currentVersion != null && currentVersion.CompareTo(new Version(\"" + attribute.sinceVersion + "\")) < 0 && currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) > 0)");
                        file.WriteLine("                    {");
                        file.WriteLine("                        if (ret.ContainsKey(\"" + newName + "\"))");
                        file.WriteLine("                        {");
                        file.WriteLine("                            ret.Remove(\"" + newName + "\");");
                        file.WriteLine("                        }");
                        file.WriteLine("                        ret.Add(\"" + newName + "\", \"" + attribute.oldName + "\");");
                        file.WriteLine("                    }");
                    }
                }
            }

            file.WriteLine("                    break;");
            file.WriteLine("                    ");
        }

        private void WriteCustomStatusCodeResponseEnabled()
        {
            WriteCustomStatusCodeResponseEnabled<AllowContentNotModifiedResponseAttribute>();
            WriteCustomStatusCodeResponseEnabled<AllowUnauthorizedResponseAttribute>();
        }

        private void WriteCustomStatusCodeResponseEnabled<T>() where T : Attribute
        {
            const string allowPrefix = "Allow";
            const string attributeSuffix = "Attribute";
            var statusCodeName = typeof(T).Name;
            if (statusCodeName.StartsWith(allowPrefix) && statusCodeName.EndsWith(attributeSuffix))
            {
                statusCodeName = statusCodeName.Substring(allowPrefix.Length, statusCodeName.Length - allowPrefix.Length - attributeSuffix.Length);
            }

            file.WriteLine("        public static bool " + statusCodeName + "Enabled(string service, string action)");
            file.WriteLine("        {");
            file.WriteLine("            service = service.ToLower();");
            file.WriteLine("            action = action.ToLower();");
            file.WriteLine("            switch (service)");
            file.WriteLine("            {");

            foreach (var controller in controllers)
            {
                var serviceAttribute = controller.GetCustomAttribute<ServiceAttribute>(true);
                if (serviceAttribute == null)
                {
                    continue;
                }

                var actions = controller.GetMethods()
                    .Where(x => x.DeclaringType == controller)
                    .ToList();
                actions.Sort(new MethodInfoComparer());

                var anyActionHasAttribute = actions.Any(x => x.GetCustomAttribute<T>(true) != null);
                if (!anyActionHasAttribute)
                {
                    continue;
                }

                file.WriteLine("                case \"" + serviceAttribute.Name.ToLower() + "\":");
                file.WriteLine("                    switch(action)");
                file.WriteLine("                    {");

                foreach (var action in actions)
                {
                    var actionAttribute = action.GetCustomAttribute<ActionAttribute>(true);
                    var responseAttribute = action.GetCustomAttribute<T>(true);
                    if (actionAttribute != null && responseAttribute != null)
                    {
                        file.WriteLine("                        case \"" + actionAttribute.Name.ToLower() + "\":");
                        file.WriteLine("                            return true;");
                        file.WriteLine("                            ");
                    }
                }

                file.WriteLine("                    }");
                file.WriteLine("                    break;");
                file.WriteLine("                    ");
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            return false;");
            file.WriteLine("        }");
            file.WriteLine("        ");
        }
    }
}
