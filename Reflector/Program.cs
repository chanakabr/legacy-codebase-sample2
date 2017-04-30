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

    class Program
    {
        static private Assembly assembly;
        static private List<Type> types;
        static private List<Type> controllers;
        static private StreamWriter file;

        static void Main(string[] args)
        {
            assembly = Assembly.Load("WebAPI");
            types = assembly.GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(KalturaOTTObject))).ToList();
            types.Sort(new TypeComparer());

            controllers = assembly.GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(ApiController))).ToList();
            controllers.Sort(new TypeComparer());

            writeDataModel();
        }

        static void writeDataModel()
        {
            string path = "..\\..\\..\\WebAPI\\Reflection\\DataModel.cs";
            file = new StreamWriter(path);

            wrtieHeader();
            wrtieBody();
            wrtieFooter();

            file.Close();
        }

        static void wrtieHeader()
        {
            file.WriteLine("// NOTICE: This is a generated file, to modify it, edit Program.cs in Reflector project");
            file.WriteLine("using System;");
            file.WriteLine("using System.Collections.Generic;");
            file.WriteLine("using System.Linq;");
            file.WriteLine("using System.Reflection;");
            file.WriteLine("using System.Web;");
            file.WriteLine("using WebAPI.Managers;");
            file.WriteLine("using WebAPI.Managers.Scheme;");
            file.WriteLine("");
            file.WriteLine("namespace WebAPI.Reflection");
            file.WriteLine("{");
            file.WriteLine("    public class DataModel");
            file.WriteLine("    {");
        }

        static void wrtieBody()
        {
            wrtieIsDeprecated();
            wrtieIsObsolete();
            wrtieMethodOldMembers();
            wrtieTypeOldMembers();
            wrtiePropertyApiName();
            wrtieValidateAuthorization();
            wrtieServeActionContentType();
            wrtieIsNewStandardOnly();
        }

        static void wrtieServeActionContentType()
        {
            file.WriteLine("        public static string getServeActionContentType(MethodInfo action)");
            file.WriteLine("        {");
            file.WriteLine("            switch (action.DeclaringType.Name)");
            file.WriteLine("            {");

            bool needed;

            foreach (Type controller in controllers)
            {
                needed = false;
                List<MethodInfo> actions = controller.GetMethods().ToList();
                actions.Sort(new MethodInfoComparer());

                foreach (MethodInfo action in actions)
                {
                    if (action.DeclaringType != controller)
                        continue;

                    SchemeServeAttribute serve = action.GetCustomAttribute<SchemeServeAttribute>(true);
                    if (serve != null)
                    {
                        needed = true;
                    }
                }

                if (!needed)
                    continue;

                file.WriteLine("                case \"" + controller.Name + "\":");
                file.WriteLine("                    switch(action.Name)");
                file.WriteLine("                    {");

                foreach (MethodInfo action in actions)
                {
                    if (action.DeclaringType != controller)
                        continue;

                    SchemeServeAttribute serve = action.GetCustomAttribute<SchemeServeAttribute>(true);
                    if (serve != null)
                    {
                        file.WriteLine("                        case \"" + action.Name + "\":");
                        file.WriteLine("                            return \"" + serve.ContentType + "\";");
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

        static void wrtieValidateAuthorization()
        {
            file.WriteLine("        public static void validateAuthorization(MethodInfo action, string serviceName, string actionName)");
            file.WriteLine("        {");
            file.WriteLine("            bool silent = false;");
            file.WriteLine("            switch (action.DeclaringType.Name)");
            file.WriteLine("            {");

            bool authNeeded;
            bool nonSilentNeeded;
            bool returnNeeded;

            foreach (Type controller in controllers)
            {
                authNeeded = false;
                nonSilentNeeded = false;
                returnNeeded = false;

                List<MethodInfo> actions = controller.GetMethods().ToList();
                actions.Sort(new MethodInfoComparer());

                foreach (MethodInfo action in actions)
                {
                    if (action.DeclaringType != controller)
                        continue;

                    ApiAuthorizeAttribute authorization = action.GetCustomAttribute<ApiAuthorizeAttribute>(true);
                    if (authorization == null)
                    {
                        returnNeeded = true;
                    }
                    else
                    {
                        authNeeded = true;
                        nonSilentNeeded = nonSilentNeeded || authorization.Silent;
                    }
                }
                if (!authNeeded)
                {
                    file.WriteLine("                case \"" + controller.Name + "\":");
                    file.WriteLine("                    return;");
                    file.WriteLine("                    ");
                    continue;
                }

                if (!nonSilentNeeded && !returnNeeded)
                    continue;

                file.WriteLine("                case \"" + controller.Name + "\":");
                file.WriteLine("                    switch(action.Name)");
                file.WriteLine("                    {");

                foreach (MethodInfo action in actions)
                {
                    if (action.DeclaringType != controller)
                        continue;

                    ApiAuthorizeAttribute authorization = action.GetCustomAttribute<ApiAuthorizeAttribute>(true);
                    if (authorization == null)
                    {
                        file.WriteLine("                        case \"" + action.Name + "\":");
                        file.WriteLine("                            return;");
                        file.WriteLine("                            ");
                    }
                    else if (authorization.Silent)
                    {
                        file.WriteLine("                        case \"" + action.Name + "\":");
                        file.WriteLine("                            silent = true;");
                        file.WriteLine("                            break;");
                        file.WriteLine("                            ");
                    }
                }

                file.WriteLine("                    }");
                file.WriteLine("                    break;");
                file.WriteLine("                    ");
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            RolesManager.ValidateActionPermitted(serviceName, actionName, silent);");
            file.WriteLine("        }");
            file.WriteLine("        ");
        }

        static void wrtieIsNewStandardOnly()
        {
            file.WriteLine("        public static bool isNewStandardOnly(PropertyInfo property)");
            file.WriteLine("        {");
            file.WriteLine("            switch (property.DeclaringType.Name)");
            file.WriteLine("            {");

            bool needed = false;

            foreach (Type type in types)
            {
                needed = false;

                List<PropertyInfo> properties = type.GetProperties().ToList();
                properties.Sort(new PropertyInfoComparer());

                foreach (PropertyInfo property in properties)
                {
                    if (property.DeclaringType != type)
                        continue;

                    OnlyNewStandardAttribute onlyNewStandard = property.GetCustomAttribute<OnlyNewStandardAttribute>();
                    if (onlyNewStandard != null)
                    {
                        needed = true;
                    }
                }
                if (!needed)
                    continue;

                file.WriteLine("                case \"" + type.Name + "\":");
                file.WriteLine("                    switch(property.Name)");
                file.WriteLine("                    {");

                foreach (PropertyInfo property in properties)
                {
                    if (property.DeclaringType != type)
                        continue;

                    OnlyNewStandardAttribute onlyNewStandard = property.GetCustomAttribute<OnlyNewStandardAttribute>();
                    if (onlyNewStandard != null)
                    {
                        file.WriteLine("                        case \"" + property.Name + "\":");
                        file.WriteLine("                            return true;");
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

        static void wrtiePropertyApiName()
        {
            file.WriteLine("        public static string getApiName(PropertyInfo property)");
            file.WriteLine("        {");
            file.WriteLine("            switch (property.DeclaringType.Name)");
            file.WriteLine("            {");

            bool needed = false;

            foreach (Type type in types)
            {
                needed = false;

                List<PropertyInfo> properties = type.GetProperties().ToList();
                properties.Sort(new PropertyInfoComparer());

                foreach (PropertyInfo property in properties)
                {
                    if (property.DeclaringType != type)
                        continue;

                    DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>();
                    if (dataMember != null && !dataMember.Name.Equals(property.Name))
                    {
                        needed = true;
                    }
                }
                if (!needed)
                    continue;

                file.WriteLine("                case \"" + type.Name + "\":");
                file.WriteLine("                    switch(property.Name)");
                file.WriteLine("                    {");

                foreach (PropertyInfo property in properties)
                {
                    if (property.DeclaringType != type)
                        continue;

                    DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>();
                    if (dataMember != null && !dataMember.Name.Equals(property.Name))
                    {
                        file.WriteLine("                        case \"" + property.Name + "\":");
                        file.WriteLine("                            return \"" + dataMember.Name + "\";");
                    }
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

        static void wrtieIsObsoleteCase(Type type)
        {
            List<PropertyInfo> properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            properties.Sort(new PropertyInfoComparer());

            if (properties.Count == 0)
                return;

            List<string> propertyNames = new List<string>();
            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<ObsoleteAttribute>() != null)
                {
                    propertyNames.Add(property.Name);
                }
            }
            if (propertyNames.Count == 0)
                return;

            propertyNames.Sort();

            file.WriteLine("                case \"" + type.Name + "\":");
            file.WriteLine("                    switch (propertyName)");
            file.WriteLine("                    {");

            foreach (string propertyName in propertyNames)
            {
                file.WriteLine("                        case \"" + propertyName + "\":");
            }

            file.WriteLine("                            return true;");
            file.WriteLine("                    };");
            file.WriteLine("                    break;");
            file.WriteLine("                    ");
        }

        static void wrtieIsDeprecatedCase(Type type)
        {
            List<PropertyInfo> properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            properties.Sort(new PropertyInfoComparer());

            if (properties.Count == 0)
                return;

            Dictionary<string, string> propertyNames = new Dictionary<string, string>();
            foreach (PropertyInfo property in properties)
            {
                DeprecatedAttribute deprecated = property.GetCustomAttribute<DeprecatedAttribute>();
                if (deprecated != null)
                {
                    propertyNames.Add(property.Name, deprecated.SinceVersion);
                }
            }
            if (propertyNames.Count == 0)
                return;
            
            file.WriteLine("                case \"" + type.Name + "\":");
            file.WriteLine("                    switch (propertyName)");
            file.WriteLine("                    {");

            foreach (KeyValuePair<string, string> property in propertyNames)
            {
                file.WriteLine("                        case \"" + property.Key + "\":");
                file.WriteLine("                            return DeprecatedAttribute.IsDeprecated(\"" + property.Value + "\");");
            }

            file.WriteLine("                    };");
            file.WriteLine("                    break;");
            file.WriteLine("                    ");
        }

        static void wrtieIsDeprecated()
        {
            file.WriteLine("        public static bool IsDeprecated(Type type, string propertyName)");
            file.WriteLine("        {");
            file.WriteLine("            switch (type.Name)");
            file.WriteLine("            {");

            foreach (Type type in types)
            {
                wrtieIsDeprecatedCase(type);
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            return false;");
            file.WriteLine("        }");
            file.WriteLine("        ");
        }

        static void wrtieIsObsolete()
        {
            file.WriteLine("        public static bool IsObsolete(Type type, string propertyName)");
            file.WriteLine("        {");
            file.WriteLine("            switch (type.Name)");
            file.WriteLine("            {");

            foreach (Type type in types)
            {
                wrtieIsObsoleteCase(type);
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            return IsDeprecated(type, propertyName);");
            file.WriteLine("        }");
            file.WriteLine("        ");
        }

        static void wrtieMethodOldMembers()
        {
            file.WriteLine("        public static Dictionary<string, string> getOldMembers(MethodInfo action, Version currentVersion)");
            file.WriteLine("        {");
            file.WriteLine("            Dictionary<string, string> ret = null;");
            file.WriteLine("            switch (action.DeclaringType.Name)");
            file.WriteLine("            {");

            foreach (Type controller in controllers)
            {
                bool hasOldStandard = false;

                List<MethodInfo> actions = controller.GetMethods().ToList();
                actions.Sort(new MethodInfoComparer());

                foreach (MethodInfo method in actions)
                {
                    if (method.DeclaringType != controller)
                        continue;

                    OldStandardArgumentAttribute[] attributes = (OldStandardArgumentAttribute[])Attribute.GetCustomAttributes(method, typeof(OldStandardArgumentAttribute));
                    hasOldStandard = hasOldStandard || attributes.Length > 0;
                }

                if (!hasOldStandard)
                    continue;

                file.WriteLine("                case \"" + controller.Name + "\":");
                file.WriteLine("                    switch(action.Name)");
                file.WriteLine("                    {");

                foreach (MethodInfo method in actions)
                {
                    if (method.DeclaringType != controller)
                        continue;

                    List<OldStandardArgumentAttribute> attributes = ((IEnumerable<OldStandardArgumentAttribute>)Attribute.GetCustomAttributes(method, typeof(OldStandardArgumentAttribute))).ToList();
                    if (attributes.Count == 0)
                        continue;

                    attributes.Sort(new OldStandardArgumentAttributeComparer());

                    file.WriteLine("                        case \"" + method.Name + "\":");
                    file.WriteLine("                            ret = new Dictionary<string, string>() { ");
                    foreach (OldStandardArgumentAttribute attribute in attributes)
                    {
                        if (attribute.sinceVersion == null)
                        {
                            file.WriteLine("                                 {\"" + attribute.newName + "\", \"" + attribute.oldName + "\"},");
                        }
                    }
                    file.WriteLine("                            };");
                    string lastVersion = null;
                    foreach (OldStandardArgumentAttribute attribute in attributes)
                    {
                        if (attribute.sinceVersion == null)
                        {
                            continue;
                        }
                        file.WriteLine("                            if (currentVersion != null && currentVersion.CompareTo(new Version(\"" + attribute.sinceVersion + "\")) < 0 && currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) > 0)");
                        file.WriteLine("                            {");
                        file.WriteLine("                                if (ret.ContainsKey(\"" + attribute.newName + "\"))");
                        file.WriteLine("                                {");
                        file.WriteLine("                                    ret.Remove(\"" + attribute.newName + "\");");
                        file.WriteLine("                                }");
                        file.WriteLine("                                ret.Add(\"" + attribute.newName + "\", \"" + attribute.oldName + "\");");
                        file.WriteLine("                            }");
                    }
                    file.WriteLine("                            break;");
                }

                file.WriteLine("                    }");
                file.WriteLine("                    break;");
                file.WriteLine("                    ");
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            return ret;");
            file.WriteLine("        }");
            file.WriteLine("        ");
        }

        static void wrtieTypeOldMembersCase(Type type)
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

        static void wrtieControllerOldMembersCase(Type controller)
        {
            List<MethodInfo> actions = controller.GetMethods().ToList();
            actions.Sort(new MethodInfoComparer());

            List<MethodInfo> oldStandardActions = new List<MethodInfo>();
            foreach (MethodInfo action in actions)
            {
                if (action.DeclaringType != controller)
                    continue;

                OldStandardActionAttribute oldStandardAction = action.GetCustomAttribute<OldStandardActionAttribute>(true);
                if (oldStandardAction != null)
                {
                    oldStandardActions.Add(action);
                }
            }

            if (oldStandardActions.Count == 0)
                return;

            file.WriteLine("                case \"" + controller.Name + "\":");
            file.WriteLine("                    ret = new Dictionary<string, string>() { ");
            foreach (MethodInfo action in oldStandardActions)
            {
                OldStandardActionAttribute oldStandardAction = action.GetCustomAttribute<OldStandardActionAttribute>(true);
                RouteAttribute route = action.GetCustomAttribute<RouteAttribute>();

                string newName = action.Name;
                if (route != null)
                {
                    newName = route.Template;
                }

                file.WriteLine("                        {\"" + newName + "\", \"" + oldStandardAction.oldName + "\"},");
            }
            file.WriteLine("                    };");
            file.WriteLine("                    break;");
            file.WriteLine("                    ");
        }

        static void wrtieTypeOldMembers()
        {
            file.WriteLine("        public static Dictionary<string, string> getOldMembers(Type type, Version currentVersion)");
            file.WriteLine("        {");
            file.WriteLine("            Dictionary<string, string> ret = null;");
            file.WriteLine("            switch (type.Name)");
            file.WriteLine("            {");

            foreach (Type type in types)
            {
                wrtieTypeOldMembersCase(type);
            }

            foreach (Type type in controllers)
            {
                wrtieControllerOldMembersCase(type);
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            return ret;");
            file.WriteLine("        }");
            file.WriteLine("        ");
        }

        static void wrtieFooter()
        {
            file.WriteLine("    }");
            file.WriteLine("}");
        }
    }
}
