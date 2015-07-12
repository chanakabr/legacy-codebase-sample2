using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Xml;

namespace WebAPI
{
    /// <summary>
    /// Summary description for DocGenerator
    /// </summary>
    public class DocGenerator : IHttpHandler
    {
        private static XmlDocument XMLFromAssemblyNonCached(Assembly assembly)
        {
            string assemblyFilename = assembly.CodeBase;

            const string prefix = "file:///";

            if (assemblyFilename.StartsWith(prefix))
            {
                StreamReader streamReader;

                try
                {
                    streamReader = new StreamReader(Path.ChangeExtension(assemblyFilename.Substring(prefix.Length), ".xml"));
                }
                catch (FileNotFoundException exception)
                {
                    throw new Exception("XML documentation not present (make sure it is turned on in project properties when building)", exception);
                }

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(streamReader);
                return xmlDocument;
            }
            else
            {
                throw new Exception("Could not ascertain assembly filename", null);
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var x = XMLFromAssemblyNonCached(asm);

            //Running on models first
            foreach (Type type in asm.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith("WebAPI.Models")))
            {
                var classNode = x.SelectNodes(string.Format("//member[@name='T:{0}']", type.FullName));

                if (classNode == null)
                    continue;

                if (type.BaseType == typeof(Enum))
                {
                    context.Response.Write(string.Format("<enum name='{0}' enumType='string'>\n", type.Name));

                    //Print values
                    foreach (var v in Enum.GetValues(type))
                        context.Response.Write(string.Format("\t<const name='{0}' />\n", v));

                    context.Response.Write("</enum>\n");
                }
                else
                {
                    //No documentation
                    if (classNode.Count == 0 || classNode[0].ChildNodes == null)
                        context.Response.Write(string.Format("<class name='{0}' description=''>\n", type.Name));
                    else
                    {
                        foreach (XmlElement child in classNode[0].ChildNodes)
                        {
                            context.Response.Write(string.Format("<class name='{0}' description='{1}'>\n", type.Name,
                                child.InnerText.Trim()));
                        }
                    }

                    runOnProperties(context, x, type.FullName, type.GetProperties());
                    context.Response.Write("</class>\n");
                }
            }

            //Running on methods
            foreach (Type controller in asm.GetTypes().Where(t => t.Namespace != null &&
                t.Namespace.StartsWith("WebAPI.Controllers") && t.Name.EndsWith("Controller")))
            {
                context.Response.Write(string.Format("<service name='{0}'>\n", controller.Name.Replace("Controller", "")));
                var methods = controller.GetMethods();
                foreach (var method in methods)
                {
                    //Read only HTTP POST as we will have duplicates otherwise
                    if (method.GetCustomAttributes<HttpPostAttribute>(false).Count() == 0)
                        continue;

                    var attr = method.GetCustomAttribute<RouteAttribute>(false);
                    if (attr != null)
                    {
                        var classNode = x.SelectNodes(string.Format("//member[starts-with(@name,'M:{0}.{1}')]", controller.FullName, method.Name));

                        string desc = "";
                        //No documentation
                        if (classNode.Count > 0 && classNode[0].ChildNodes != null)
                        {
                            for (int i = 0; i < classNode[0].ChildNodes.Count; i++)
                            {
                                if (classNode[0].ChildNodes[i].Name == "summary")
                                {
                                    desc = classNode[0].ChildNodes[i].InnerText.Trim();
                                    break;
                                }
                            }
                        }

                        context.Response.Write(string.Format("\t<action name='{0}' description='{1}'>\n", ((RouteAttribute)attr).Template, desc.Trim().Replace('\'', '"')));
                        foreach (var par in method.GetParameters())
                        {
                            var descs = x.SelectNodes(string.Format("//member[starts-with(@name,'M:{0}.{1}')]/param[@name='{2}']", 
                                controller.FullName, method.Name, par.Name));

                            string pdesc = "";
                            if (descs.Count > 0)
                                pdesc = descs[0].InnerText.Trim().Replace('\'', '"');

                            context.Response.Write(string.Format("\t\t<param name='{0}' type='{1}' description='{2}'/>\n", par.Name,
                                getTypeFriendlyName(par.ParameterType), pdesc));
                        }
                        context.Response.Write(string.Format("\t\t<result type='{0}'/>\n", getTypeFriendlyName(method.ReturnType)));
                        context.Response.Write("\t</action>\n");
                    }
                };

                context.Response.Write(string.Format("</service>\n"));
            }
        }

        private void runOnProperties(HttpContext context, XmlDocument x, string className, MemberInfo[] members)
        {
            foreach (var property in members)
            {
                if (property is PropertyInfo)
                {
                    var pi = (PropertyInfo)property;

                    var descs = x.SelectNodes(string.Format("//member[@name='P:{0}.{1}']//summary",
                                className, pi.Name));

                    string pdesc = "";
                    if (descs.Count > 0)
                        pdesc = descs[0].InnerText.Trim().Replace('\'', '"');

                    if (pi.PropertyType.IsEnum)
                    {
                        context.Response.Write(string.Format("\t<property name='{0}' type='string' enumType='{1}' description='{2}' />\n", pi.Name,
                            getTypeFriendlyName(pi.PropertyType), pdesc));
                    }
                    else
                    {
                        context.Response.Write(string.Format("\t<property name='{0}' type='{1}' description='{2}' />\n", pi.Name,
                        getTypeFriendlyName(pi.PropertyType), pdesc));
                    }
                }
                else
                {
                    context.Response.Write(string.Format("\t<property name='{0}' type='{1}' />\n", property.Name,
                    "error"));
                }
            }
        }

        private static string getTypeFriendlyName(Type type)
        {
            if (type == typeof(String))
                return "string";
            if (type == typeof(long) || type == typeof(Int64))
                return "bigint";
            if (type == typeof(Int32))
                return "int";
            if (type == typeof(double) || type == typeof(float))
                return "float";
            if (type == typeof(bool))
                return "bool";

            return type.Name;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}