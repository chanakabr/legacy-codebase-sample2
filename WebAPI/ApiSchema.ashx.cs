using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI
{
    /// <summary>
    /// XXX: SHOULD BE REFACTORED SOMETIME TO USE XMLDOCUMENT...
    /// </summary>
    public class ApiSchema : IHttpHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
            List<Type> enums = new List<Type>();
            List<Type> classes = new List<Type>();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            context.Response.ContentType = "text/xml";
            context.Response.Write(string.Format("<?xml version=\"1.0\"?>\n<xml apiVersion=\"{0}\" generatedDate=\"{1}\">\n",
                fvi.FileVersion, Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow)));

            List<Type> types = asm.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith("WebAPI.Models")).ToList();

            List<Field> fields = new List<Field>();
            foreach (Type tp in types)
            {
                if (tp.IsInterface)
                    continue;

                if (tp.IsEnum)
                    continue;

                if (tp.BaseType == typeof(Attribute))
                    continue;

                Field f = new Field();
                f.Name = tp.Name;
                List<string> dependant = new List<string>();

                foreach (var property in tp.GetProperties())
                {
                    if (property.PropertyType.IsGenericType)
                    {
                        foreach (var ga in property.PropertyType.GetGenericArguments())
                        {
                            if (isNeeded(ga))
                                dependant.Add(ga.Name);
                        }
                    }
                    else if (property.PropertyType.IsArray)
                    {
                        var arrType = property.PropertyType.GetElementType();
                        if (isNeeded(arrType))
                            dependant.Add(arrType.Name);
                    }
                    else
                    {
                        if (isNeeded(property.PropertyType))
                            dependant.Add(property.PropertyType.Name);
                    }
                }

                if (tp.BaseType != null && isNeeded(tp.BaseType))
                    dependant.Add(tp.BaseType.Name);

                //Removes duplicates and prevents circular dependency
                f.DependsOn = dependant.Distinct().Where(item => item != tp.Name).ToList();
                fields.Add(f);
            }

            int[] sortOrder = getTopologicalSortOrder(fields);

            List<Type> sortedTypes = new List<Type>();
            for (int i = 0; i < sortOrder.Length; i++)
            {
                var field = fields[sortOrder[i]];
                sortedTypes.Insert(0, types.Where(yy => yy.Name == field.Name).First());
            }

            enums = types.Where(zz => zz.IsEnum).ToList();

            //Printing enums
            context.Response.Write("<enums>\n");
            foreach (Type t in enums.OrderBy(c => c.Name))
            {
                bool isIntEnum = t.GetCustomAttribute<KalturaIntEnumAttribute>() != null;
                var etype = isIntEnum ? "int" : "string";

                context.Response.Write(string.Format("\t<enum name='{0}' enumType='{1}'>\n", t.Name, etype));

                //Print values
                foreach (var v in Enum.GetValues(t))
                {
                    string eValue = isIntEnum ? ((int)Enum.Parse(t, v.ToString())).ToString() : v.ToString();
                    context.Response.Write(string.Format("\t\t<const name='{0}' value='{1}' />\n", v.ToString().ToUpper(), eValue));
                }

                context.Response.Write("\t</enum>\n");
            }

            //Hardcoding the status codes
            var statusCodes = ClientsManager.ApiClient().GetErrorCodesDictionary();
            context.Response.Write("\t<enum name='KalturaStatusCodes' enumType='int'>\n");
            foreach (var kv in statusCodes)
                context.Response.Write(string.Format("\t\t<const name='{0}' value='{1}' />\n", kv.Key, kv.Value));
            foreach (var ee in Enum.GetValues(typeof(StatusCode)))
            {
                int eVal = (int)Enum.Parse(typeof(StatusCode), ee.ToString());
                //Prevents duplicates
                if (statusCodes.Where(xx => xx.Value == eVal).Count() == 0)
                    context.Response.Write(string.Format("\t\t<const name='{0}' value='{1}' />\n", ee.ToString(), eVal));
            }

            context.Response.Write("\t</enum>\n");

            context.Response.Write("</enums>\n");

            //Running on classes
            context.Response.Write("<classes>\n");
            foreach (Type t in sortedTypes)
            {
                //Skip master base class
                if (t == typeof(KalturaOTTObject))
                    continue;

                var classNode = x.SelectNodes(string.Format("//member[@name='T:{0}']", t.FullName));

                if (classNode == null)
                    continue;

                string baseName = t.BaseType != null && t.BaseType != typeof(Object) && t.BaseType != typeof(KalturaOTTObject) ?
                    string.Format("base='{0}'", t.BaseType.Name) : "";

                bool isAbstractOrInterface = t.IsInterface || t.IsAbstract;

                //No documentation
                if (classNode.Count == 0 || classNode[0].ChildNodes == null)
                {
                    log.Error("Empty description - " + t.Name);
                    context.Response.Write(string.Format("\t<class name='{0}' description='' {1}", t.Name,
                        baseName));

                    if (isAbstractOrInterface)
                        context.Response.Write("abstract='1'");

                    context.Response.Write(">\n");
                }
                else
                {
                    foreach (XmlElement child in classNode[0].ChildNodes)
                    {
                        context.Response.Write(string.Format("\t<class name='{0}' description='{1}' {2}", t.Name,
                            HttpUtility.HtmlEncode(child.InnerText.Trim()), baseName));

                        if (isAbstractOrInterface)
                            context.Response.Write("abstract='1'");

                        context.Response.Write(">\n");
                    }
                }

                List<PropertyInfo> props = t.GetProperties().ToList();

                //Remove properties from base
                if (t.BaseType != null)
                {
                    PropertyInfo[] baseProps = t.BaseType.GetProperties();
                    props.RemoveAll(xx => baseProps.Where(y => y.Name == xx.Name).Count() > 0);
                }

                runOnProperties(context, x, t.FullName, props.ToArray());

                context.Response.Write("\t</class>\n");
            }

            context.Response.Write("</classes>\n");

            var routePrefix = asm.GetType("WebAPI.Controllers.ServiceController").GetCustomAttribute<RoutePrefixAttribute>().Prefix;

            //Running on methods
            context.Response.Write("<services>\n");
            foreach (Type controller in asm.GetTypes().Where(t => t.Namespace != null &&
                t.Namespace.StartsWith("WebAPI.Controllers") && t.Name.EndsWith("Controller")).OrderBy(c => c.Name))
            {
                var controllerAttr = controller.GetCustomAttribute<ApiExplorerSettingsAttribute>(false);

                if (controllerAttr != null && controllerAttr.IgnoreApi)
                    continue;

                context.Response.Write(string.Format("\t<service name='{0}' id='{0}'>\n", controller.Name.Replace("Controller", "")));
                var methods = controller.GetMethods().OrderBy(z => z.Name);
                foreach (var method in methods)
                {
                    //Read only HTTP POST as we will have duplicates otherwise
                    var explorerAttr = method.GetCustomAttributes<ApiExplorerSettingsAttribute>(false);
                    if (explorerAttr.Count() > 0 && explorerAttr.First().IgnoreApi)
                        continue;

                    var attr = method.GetCustomAttribute<RouteAttribute>(false);
                    if (attr != null)
                    {
                        var classNode = x.SelectNodes(string.Format("//member[starts-with(@name,'M:{0}.{1}')]", controller.FullName, method.Name));

                        string desc = "";
                        string remarks = "";
                        //No documentation
                        if (classNode.Count > 0 && classNode[0].ChildNodes != null)
                        {
                            for (int i = 0; i < classNode[0].ChildNodes.Count; i++)
                            {
                                if (classNode[0].ChildNodes[i].Name == "summary")
                                    desc = classNode[0].ChildNodes[i].InnerText.Trim();

                                if (classNode[0].ChildNodes[i].Name == "remarks")
                                    remarks = classNode[0].ChildNodes[i].InnerText.Trim();
                            }
                        }

                        if (string.IsNullOrEmpty(desc))
                            log.Error("Empty description in method - " + method.Name);
                        else
                            desc += string.Format("{0} {1}", !desc.EndsWith(".") ? "." : "", remarks);

                        string deprecatedAttr = "";
                        if (method.GetCustomAttribute<ObsoleteAttribute>() != null)
                            deprecatedAttr = string.Format("deprecated='1'");

                        context.Response.Write(string.Format("\t\t<action name='{0}' enableInMultiRequest='0' supportedRequestFormats='json' supportedResponseFormats='json,xml' description='{1}' {2} path='/{3}/{4}/{5}'>\n",
                            method.Name, HttpUtility.HtmlEncode(desc.Trim().Replace('\'', '"')), deprecatedAttr,
                            routePrefix, controller.Name.Replace("Controller", ""), method.Name));

                        foreach (var par in method.GetParameters())
                        {
                            var descs = x.SelectNodes(string.Format("//member[starts-with(@name,'M:{0}.{1}')]/param[@name='{2}']",
                                controller.FullName, method.Name, par.Name));

                            string pdesc = "";
                            if (descs.Count > 0)
                                pdesc = descs[0].InnerText.Trim().Replace('\'', '"');

                            if (string.IsNullOrEmpty(pdesc))
                                log.Error("Empty description in method " + method + " parameter - " + par.Name);

                            context.Response.Write(string.Format("\t\t\t<param name='{0}' {1} description='{2}' optional='{3}'/>\n", par.Name,
                                getTypeAndArray(par.ParameterType), HttpUtility.HtmlEncode(pdesc), par.IsOptional ? "1" : "0"));
                        }

                        if (method.ReturnType != typeof(void))
                            context.Response.Write(string.Format("\t\t\t<result {0}/>\n", getTypeAndArray(method.ReturnType)));
                        else
                            context.Response.Write("\t\t\t<result />\n");

                        context.Response.Write("\t\t</action>\n");
                    }
                };

                context.Response.Write(string.Format("\t</service>\n"));
            }
            context.Response.Write("</services>\n");

            //Config section
            context.Response.Write("<configurations>\n");
            context.Response.Write("\t<client type='KalturaClientConfiguration'>\n");
            context.Response.Write("\t\t<clientTag type='string' />\n\t\t<apiVersion type='string'/>\n");
            context.Response.Write("\t</client>\n");
            context.Response.Write("\t<request type='KalturaRequestConfiguration'>\n");
            context.Response.Write("\t\t<partnerId type='int' description='Impersonated partner id'/>\n");
            context.Response.Write("\t\t<ks type='string' alias='sessionId' description='Kaltura API session'/>\n");
            context.Response.Write("\t</request>\n");
            context.Response.Write("</configurations>\n");

            context.Response.Write("</xml>");
        }

        private bool isNeeded(Type type)
        {
            return !type.IsPrimitive && type != typeof(object) && type != typeof(string) && !type.IsEnum && type != typeof(DateTime);
        }

        private string getTypeAndArray(Type type)
        {
            //Handling nullables
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = type.GetGenericArguments()[0];
            }

            //Handling Enums
            if (type.IsEnum)
            {
                return string.Format("type='string' enumType='{0}'", getTypeFriendlyName(type));
            }
            //Handling arrays
            else if (type.IsArray || type.IsGenericType)
            {
                string arrayType = "";
                string name = "";

                if (type.IsArray)
                {
                    arrayType = getTypeFriendlyName(type.GetElementType());
                    name = "array";
                }
                else if (type.IsGenericType)
                {
                    Type dictType = typeof(SerializableDictionary<,>);

                    //if List
                    if (type.GetGenericArguments().Count() == 1)
                    {
                        arrayType = getTypeFriendlyName(type.GetGenericArguments()[0]);
                        name = "array";
                    }
                    //if Dictionary
                    else if (type.GetGenericArguments().Count() == 2 &&
                        dictType.GetGenericArguments().Length == type.GetGenericArguments().Length &&
                        dictType.MakeGenericType(type.GetGenericArguments()) == type)
                    {
                        arrayType = getTypeFriendlyName(type.GetGenericArguments()[1]);
                        name = "map";
                    }
                    else if (type.GetGenericArguments().Count() == 2)
                    {
                        throw new Exception("Dont know how to handle");
                    }
                    else
                        throw new Exception("Generic type unknown");
                }

                return string.Format("type='{0}' arrayType='{1}'", name, arrayType);
            }

            return string.Format("type='{0}' default='{1}'", getTypeFriendlyName(type), getDefaultForType(type));
        }

        private string getDefaultForType(Type type)
        {
            if (type == typeof(String))
                return "null";
            if (type == typeof(DateTime))
                return "0";
            if (type == typeof(long) || type == typeof(Int64))
                return "0";
            if (type == typeof(Int32))
                return "0";
            if (type == typeof(double) || type == typeof(float))
                return "0";
            if (type == typeof(bool))
                return "false";

            return "null";
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

                    //Type eType = null;
                    var dataMemberAttr = pi.GetCustomAttribute<DataMemberAttribute>();

                    if (dataMemberAttr == null)
                        continue;

                    var attr = dataMemberAttr.Name;

                    context.Response.Write(string.Format("\t\t<property name='{0}' {1} description='{2}' readOnly='0' insertOnly='0' />\n", attr,
                           getTypeAndArray(pi.PropertyType), HttpUtility.HtmlEncode(pdesc)));
                }
                else
                {
                    throw new Exception("Unable to generate");
                }
            }
        }

        private static string getTypeFriendlyName(Type type)
        {
            if (type == typeof(String))
                return "string";
            if (type == typeof(DateTime))
                return "time";
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

            return type.Name;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        class TopologicalSorter
        {
            #region - Private Members -

            private readonly int[] _vertices; // list of vertices
            private readonly int[,] _matrix; // adjacency matrix
            private int _numVerts; // current number of vertices
            private readonly int[] _sortedArray;

            #endregion

            #region - CTors -

            public TopologicalSorter(int size)
            {
                _vertices = new int[size];
                _matrix = new int[size, size];
                _numVerts = 0;
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                        _matrix[i, j] = 0;
                _sortedArray = new int[size]; // sorted vert labels
            }

            #endregion

            #region - Public Methods -

            public int AddVertex(int vertex)
            {
                _vertices[_numVerts++] = vertex;
                return _numVerts - 1;
            }

            public void AddEdge(int start, int end)
            {
                _matrix[start, end] = 1;
            }

            public int[] Sort() // toplogical sort
            {
                while (_numVerts > 0) // while vertices remain,
                {

                    // get a vertex with no successors, or -1
                    int currentVertex = noSuccessors();
                    //HttpContext.Current.Response.Write("<p>vertx:" + i + "</p>");
                    //HttpContext.Current.Response.Flush();
                    if (currentVertex == -1) // must be a cycle                
                        throw new Exception("Graph has cycles");

                    // insert vertex label in sorted array (start at end)
                    _sortedArray[_numVerts - 1] = _vertices[currentVertex];

                    deleteVertex(currentVertex); // delete vertex
                }

                // vertices all gone; return sortedArray
                return _sortedArray;
            }

            #endregion

            #region - Private Helper Methods -

            // returns vert with no successors (or -1 if no such verts)
            private int noSuccessors()
            {
                for (int row = 0; row < _numVerts; row++)
                {
                    bool isEdge = false; // edge from row to column in adjMat
                    for (int col = 0; col < _numVerts; col++)
                    {
                        if (_matrix[row, col] > 0) // if edge to another,
                        {
                            isEdge = true;
                            break; // this vertex has a successor try another
                        }
                    }
                    if (!isEdge) // if no edges, has no successors
                        return row;
                }
                return -1; // no
            }

            private void deleteVertex(int delVert)
            {
                // if not last vertex, delete from vertexList
                if (delVert != _numVerts - 1)
                {
                    for (int j = delVert; j < _numVerts - 1; j++)
                        _vertices[j] = _vertices[j + 1];

                    for (int row = delVert; row < _numVerts - 1; row++)
                        moveRowUp(row, _numVerts);

                    for (int col = delVert; col < _numVerts - 1; col++)
                        moveColLeft(col, _numVerts - 1);
                }
                _numVerts--; // one less vertex
            }

            private void moveRowUp(int row, int length)
            {
                for (int col = 0; col < length; col++)
                    _matrix[row, col] = _matrix[row + 1, col];
            }

            private void moveColLeft(int col, int length)
            {
                for (int row = 0; row < length; row++)
                    _matrix[row, col] = _matrix[row, col + 1];
            }

            #endregion
        }

        private static int[] getTopologicalSortOrder(List<Field> fields)
        {
            TopologicalSorter g = new TopologicalSorter(fields.Count);
            Dictionary<string, int> _indexes = new Dictionary<string, int>();

            //add vertices
            for (int i = 0; i < fields.Count; i++)
            {
                _indexes[fields[i].Name.ToLower()] = g.AddVertex(i);
            }

            //add edges
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].DependsOn != null)
                {
                    for (int j = 0; j < fields[i].DependsOn.Count; j++)
                    {
                        g.AddEdge(i,
                            _indexes[fields[i].DependsOn[j].ToLower()]);
                    }
                }
            }

            int[] result = g.Sort();
            return result;

        }

        class Field
        {
            public string Name { get; set; }
            public List<string> DependsOn { get; set; }
        }
    }
}