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
    class Program
    {
        static private Assembly assembly;
        static private IEnumerable<Type> types;
        static private IEnumerable<Type> controllers;
        static private StreamWriter file;

        static void Main(string[] args)
        {
            assembly = Assembly.Load("WebAPI");
            types = assembly.GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(KalturaOTTObject)));
            controllers = assembly.GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(ApiController)));

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
                foreach (MethodInfo action in controller.GetMethods())
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

                foreach (MethodInfo action in controller.GetMethods())
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
                foreach (MethodInfo action in controller.GetMethods())
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

                foreach (MethodInfo action in controller.GetMethods())
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
                foreach (PropertyInfo property in type.GetProperties())
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

                foreach (PropertyInfo property in type.GetProperties())
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
                foreach (PropertyInfo property in type.GetProperties())
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

                foreach (PropertyInfo property in type.GetProperties())
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
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (properties.Length == 0)
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
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (properties.Length == 0)
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
            file.WriteLine("        public static Dictionary<string, string> getOldMembers(MethodInfo action)");
            file.WriteLine("        {");
            file.WriteLine("            switch (action.DeclaringType.Name)");
            file.WriteLine("            {");

            foreach (Type controller in controllers)
            {
                bool hasOldStandard = false;
                foreach (MethodInfo method in controller.GetMethods())
                {
                    if (method.DeclaringType != controller)
                        continue;

                    OldStandardAttribute[] attributes = (OldStandardAttribute[])Attribute.GetCustomAttributes(method, typeof(OldStandardAttribute));
                    hasOldStandard = hasOldStandard || attributes.Length > 0;
                }

                if (!hasOldStandard)
                    continue;

                file.WriteLine("                case \"" + controller.Name + "\":");
                file.WriteLine("                    switch(action.Name)");
                file.WriteLine("                    {");

                foreach (MethodInfo method in controller.GetMethods())
                {
                    if (method.DeclaringType != controller)
                        continue;

                    OldStandardAttribute[] attributes = (OldStandardAttribute[])Attribute.GetCustomAttributes(method, typeof(OldStandardAttribute));
                    if (attributes.Length == 0)
                        continue;

                    file.WriteLine("                       case \"" + method.Name + "\":");
                    file.WriteLine("                            return new Dictionary<string, string>() { ");
                    foreach (OldStandardAttribute attribute in attributes)
                    {
                        file.WriteLine("                                {\"" + attribute.newMember + "\", \"" + attribute.oldMember + "\"},");
                    }
                    file.WriteLine("                           };");
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

        static void wrtieTypeOldMembersCase(Type type)
        {
            OldStandardAttribute[] attributes = (OldStandardAttribute[])Attribute.GetCustomAttributes(type, typeof(OldStandardAttribute));
            if (attributes.Length == 0)
                return;

            file.WriteLine("                case \"" + type.Name + "\":");
            file.WriteLine("                    return new Dictionary<string, string>() { ");
            foreach (OldStandardAttribute attribute in attributes)
            {
                file.WriteLine("                        {\"" + attribute.newMember + "\", \"" + attribute.oldMember + "\"},");
            }
            file.WriteLine("                    };");
            file.WriteLine("                    ");
        }

        static void wrtieTypeOldMembers()
        {
            file.WriteLine("        public static Dictionary<string, string> getOldMembers(Type type)");
            file.WriteLine("        {");
            file.WriteLine("            switch (type.Name)");
            file.WriteLine("            {");

            foreach (Type type in types)
            {
                wrtieTypeOldMembersCase(type);
            }

            foreach (Type type in controllers)
            {
                wrtieTypeOldMembersCase(type);
            }

            file.WriteLine("            }");
            file.WriteLine("            ");
            file.WriteLine("            return null;");
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
