using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Linq;
using WebAPI.Models.General;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Models;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web;

namespace WebAPI.Managers
{
    class Field
    {
        private string _name;
        private List<string> _dependsOn;

        public Field(Type type)
        {
            _name = type.Name;
            _dependsOn = new List<string>();

            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType.IsGenericType)
                {
                    foreach (var genericArgument in property.PropertyType.GetGenericArguments())
                    {
                        AddDependency(genericArgument);
                    }
                }
                else if (property.PropertyType.IsArray)
                {
                    var arrType = property.PropertyType.GetElementType();
                    AddDependency(arrType);
                }
                else if (property.PropertyType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    AddDependency(property.PropertyType);
                }
            }

            if (type.BaseType != null)
                AddDependency(type.BaseType);
        }

        private void AddDependency(Type type)
        {
            if (type.Name == _name)
                return;

            if (type.IsGenericType)
            {
                foreach (var genericArgument in type.GetGenericArguments())
                {
                    AddDependency(genericArgument);
                }
            }
            else if (type.IsArray)
            {
                var arrType = type.GetElementType();
                AddDependency(arrType);
            }
            else if (type.IsSubclassOf(typeof(KalturaOTTObject)))
            {
                _dependsOn.Add(type.Name);
            }
        }

        public string Name 
        {
            get
            {
                return _name;
            }
        }

        public List<string> DependsOn 
        { 
            get{
                return _dependsOn;
            }
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


    public class SchemaWriter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private Stream stream;
        private Assembly assembly;
        private XmlDocument assemblyXml;
        private XmlWriter writer;

        private List<Type> enums = new List<Type>();
        private List<Type> types;
        private IEnumerable<Type> controllers;

        public SchemaWriter(Stream stream)
        {
            this.stream = stream;
            this.assembly = Assembly.GetExecutingAssembly();
            this.assemblyXml = GetAssemblyXml(assembly);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            writer = XmlWriter.Create(stream, settings);

            Load();
        }

        private void Load()
        {
            controllers = assembly.GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(ApiController)));
            List<Type> allTypes = assembly.GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(KalturaOTTObject))).ToList();

            List<Field> fields = new List<Field>();
            foreach (Type type in allTypes)
            {   
                fields.Add(new Field(type));
                LoadEnums(type);
            }

            int[] sortedFields = Sort(fields);

            types = new List<Type>();
            for (int i = 0; i < sortedFields.Length; i++)
            {
                var field = fields[sortedFields[i]];
                types.Insert(0, allTypes.Where(myType => myType.Name == field.Name).First());
            }

            foreach (Type controller in controllers)
            {
                var methods = controller.GetMethods().OrderBy(z => z.Name);
                foreach (var method in methods)
                {
                    foreach (var param in method.GetParameters())
                    {
                        LoadEnums(param.ParameterType);
                    }
                };
            }
        }

        private void LoadEnums(Type type)
        {
            if (type.IsEnum && !enums.Contains(type))
            {
                enums.Add(type);
                return;
            }

            if (type.IsSubclassOf(typeof(KalturaOTTObject)))
            {
                List<PropertyInfo> properties = type.GetProperties().ToList();
                foreach (var property in properties)
                {
                    LoadEnums(property.PropertyType);
                }
            }
        }

        private XmlDocument GetAssemblyXml(Assembly assembly)
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

        internal void write()
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            writer.WriteStartDocument();
            writer.WriteStartElement("xml");
            writer.WriteAttributeString("apiVersion", fileVersionInfo.FileVersion);
            writer.WriteAttributeString("generatedDate", Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow).ToString());
            
            //Printing enums
            writer.WriteStartElement("enums");
            foreach (Type type in enums.OrderBy(myType => myType.Name))
            {
                if (!SchemaManager.Validate(type))
                    continue;

                writeEnum(type);
            }

            //Hardcoding the status codes
            //var statusCodes = ClientsManager.ApiClient().GetErrorCodesDictionary();
            //writer.WriteStartElement("enum");
            //writer.WriteAttributeString("name", "KalturaStatusCodes");
            //writer.WriteAttributeString("enumType", "int");
            //foreach (var enumValue in statusCodes)
            //{
            //    writer.WriteStartElement("const");
            //    writer.WriteAttributeString("name", enumValue.Key);
            //    writer.WriteAttributeString("value", enumValue.Value.ToString());
            //    writer.WriteEndElement(); // const
            //}
            //foreach (var enumValue in Enum.GetValues(typeof(StatusCode)))
            //{
            //    int eVal = (int)Enum.Parse(typeof(StatusCode), enumValue.ToString());

            //    if (statusCodes.Where(status => status.Value == eVal).Count() == 0)
            //    {
            //        writer.WriteStartElement("const");
            //        writer.WriteAttributeString("name", enumValue.ToString());
            //        writer.WriteAttributeString("value", eVal.ToString());
            //        writer.WriteEndElement(); // const
            //    }
            //}
            //writer.WriteEndElement(); // enum

            writer.WriteEndElement(); // enums

            //Running on classes
            writer.WriteStartElement("classes");
            foreach (Type type in types)
            {
                //Skip master base class
                if (type == typeof(KalturaOTTObject))
                    continue;

                if (!SchemaManager.Validate(type))
                    continue;

                writeType(type);
            }

            writer.WriteEndElement(); // classes

            //Running on methods
            writer.WriteStartElement("services");
            foreach (Type controller in controllers.OrderBy(controller => controller.Name))
            {
                var controllerAttr = controller.GetCustomAttribute<ApiExplorerSettingsAttribute>(false);

                if (controllerAttr != null && controllerAttr.IgnoreApi)
                    continue;

                if (!SchemaManager.Validate(controller))
                    continue;

                writeService(controller);
            }
            writer.WriteEndElement(); // services



            //Config section
            writer.WriteStartElement("configurations");

            writer.WriteStartElement("client");
            writer.WriteAttributeString("type", "KalturaClientConfiguration");

            writer.WriteStartElement("clientTag");
            writer.WriteAttributeString("type", "string");
            writer.WriteEndElement(); // clientTag

            writer.WriteStartElement("apiVersion");
            writer.WriteAttributeString("type", "string");
            writer.WriteEndElement(); // apiVersion

            writer.WriteEndElement(); // client


            writer.WriteStartElement("request");
            writer.WriteAttributeString("type", "KalturaRequestConfiguration");

            writer.WriteStartElement("partnerId");
            writer.WriteAttributeString("type", "int");
            writer.WriteAttributeString("description", "Impersonated partner id");
            writer.WriteEndElement(); // partnerId

            writer.WriteStartElement("ks");
            writer.WriteAttributeString("type", "string");
            writer.WriteAttributeString("alias", "sessionId");
            writer.WriteAttributeString("description", "Kaltura API session");
            writer.WriteEndElement(); // ks

            writer.WriteEndElement(); // request

            writer.WriteEndElement(); // configurations

            writer.WriteEndElement(); // xml

            writer.WriteEndDocument();
            writer.Dispose();
        }

        private int[] Sort(List<Field> fields)
        {
            TopologicalSorter topologicalSorter = new TopologicalSorter(fields.Count);
            Dictionary<string, int> _indexes = new Dictionary<string, int>();

            //add vertices
            for (int i = 0; i < fields.Count; i++)
            {
                _indexes[fields[i].Name.ToLower()] = topologicalSorter.AddVertex(i);
            }

            //add edges
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].DependsOn != null)
                {
                    for (int j = 0; j < fields[i].DependsOn.Count; j++)
                    {
                        var name = fields[i].DependsOn[j];
                        if (!_indexes.ContainsKey(name.ToLower()))
                            throw new Exception(string.Format("Unable to find dependency [{0}] for type [{1}]", name, fields[i].Name));

                        topologicalSorter.AddEdge(i, _indexes[name.ToLower()]);
                    }
                }
            }

            return topologicalSorter.Sort();
        }

        public static string FirstCharacterToLower(string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
                return str;

            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public static string getServiceId(Type controller)
        {
            return FirstCharacterToLower(controller.Name.Replace("Controller", ""));
        }

        private void writeService(Type controller)
        {
            var serviceId = getServiceId(controller);

            writer.WriteStartElement("service");
            writer.WriteAttributeString("name", serviceId);
            writer.WriteAttributeString("id", serviceId);

            var methods = controller.GetMethods().OrderBy(method => method.Name);
            foreach (var method in methods)
            {
                //Read only HTTP POST as we will have duplicates otherwise
                var explorerAttr = method.GetCustomAttributes<ApiExplorerSettingsAttribute>(false);
                if (explorerAttr.Count() > 0 && explorerAttr.First().IgnoreApi)
                    continue;

                if (!SchemaManager.Validate(method))
                    continue;

                writeAction(method);
            };

            writer.WriteEndElement(); // service
        }

        private void writeAction(MethodInfo method)
        {
            var controller = method.ReflectedType;
            var serviceId = getServiceId(controller);
            var actionId = FirstCharacterToLower(method.Name);
            var attr = method.GetCustomAttribute<RouteAttribute>(false);
            var routePrefix = assembly.GetType("WebAPI.Controllers.ServiceController").GetCustomAttribute<RoutePrefixAttribute>().Prefix;
            
            writer.WriteStartElement("action");
            writer.WriteAttributeString("name", method.Name);
            writer.WriteAttributeString("enableInMultiRequest", "0");
            writer.WriteAttributeString("supportedRequestFormats", "json");
            writer.WriteAttributeString("supportedResponseFormats", "json,xml");
            writer.WriteAttributeString("description", getDescription(method));
            writer.WriteAttributeString("path", string.Format("/{0}/{1}/{2}", routePrefix, serviceId, actionId));
            if (method.GetCustomAttribute<ObsoleteAttribute>() != null)
                writer.WriteAttributeString("deprecated", "1");

            foreach (var param in method.GetParameters())
            {
                writer.WriteStartElement("param");
                writer.WriteAttributeString("name", param.Name);
                appendType(param.ParameterType);

                if (param.IsOptional)
                    writer.WriteAttributeString("default", param.DefaultValue == null ? "null" : param.DefaultValue.ToString());

                writer.WriteAttributeString("description", getDescription(method, param));
                writer.WriteAttributeString("optional", param.IsOptional ? "1" : "0");
                writer.WriteEndElement(); // param
            }

            writer.WriteStartElement("result");
            appendType(method.ReturnType);
            writer.WriteEndElement(); // result

            writer.WriteEndElement(); // action
        }

        private string getDescription(PropertyInfo property)
        {
            return getDescription(string.Format("//member[@name='P:{0}.{1}']", property.ReflectedType, property.Name));
        }

        private string getDescription(Type type)
        {
            return getDescription(string.Format("//member[@name='T:{0}']", type.FullName));
        }

        private string getDescription(MethodInfo method)
        {
            return getDescription(string.Format("//member[starts-with(@name,'M:{0}.{1}')]", method.ReflectedType.FullName, method.Name));
        }

        private string getDescription(MethodInfo method, ParameterInfo param)
        {
            return getDescription(string.Format("//member[starts-with(@name,'M:{0}.{1}')]/param[@name='{2}']", method.ReflectedType.FullName, method.Name, param.Name));
        }
        
        private string getDescription(string xPath)
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
        
        private void writeType(Type type)
        {
            writer.WriteStartElement("class");
            writer.WriteAttributeString("name", type.Name);
            writer.WriteAttributeString("description", getDescription(type));

            if (type.BaseType != null && type.BaseType != typeof(KalturaOTTObject))
                writer.WriteAttributeString("base", type.BaseType.Name);

            if (type.IsInterface || type.IsAbstract)
                writer.WriteAttributeString("abstract", "1");


            List<PropertyInfo> properties = type.GetProperties().ToList();

            //Remove properties from base
            PropertyInfo[] baseProps = type.BaseType.GetProperties();
            properties.RemoveAll(myProperty => baseProps.Where(baseProperty => baseProperty.Name == myProperty.Name).Count() > 0);

            foreach (var property in properties)
            {
                //Type eType = null;
                var dataMemberAttr = property.GetCustomAttribute<DataMemberAttribute>();
                if (dataMemberAttr == null)
                    continue;

                writer.WriteStartElement("property");
                writer.WriteAttributeString("name", dataMemberAttr.Name);
                appendType(property.PropertyType);
                writer.WriteAttributeString("description", getDescription(property));
                writer.WriteAttributeString("readOnly", "0");
                writer.WriteAttributeString("insertOnly", "0");
                writer.WriteEndElement(); // property
            }

            writer.WriteEndElement(); // class
        }

        private void writeEnum(Type type)
        {
            bool isIntEnum = type.GetCustomAttribute<KalturaIntEnumAttribute>() != null;
            var etype = isIntEnum ? "int" : "string";

            writer.WriteStartElement("enum");
            writer.WriteAttributeString("name", type.Name);
            writer.WriteAttributeString("enumType", isIntEnum ? "int" : "string");

            //Print values
            foreach (var enumValue in Enum.GetValues(type))
            {
                string eValue = isIntEnum ? ((int)Enum.Parse(type, enumValue.ToString())).ToString() : enumValue.ToString();
                writer.WriteStartElement("const");
                writer.WriteAttributeString("name", enumValue.ToString().ToUpper());
                writer.WriteAttributeString("value", eValue);
                writer.WriteEndElement(); // const
            }

            writer.WriteEndElement(); // enum
        }

        private string getTypeName(Type type)
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

        private void appendType(Type type)
        {
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

                writer.WriteAttributeString("type", isIntEnum ? "int" : "string");
                writer.WriteAttributeString("enumType", getTypeName(type));
                return;
            }

            //Handling arrays
            if (type.IsArray)
            {
                writer.WriteAttributeString("type", "array");
                writer.WriteAttributeString("arrayType", getTypeName(type.GetElementType()));
                return;
            }

            if (type.IsGenericType)
            {

                //if List
                if (type.GetGenericArguments().Count() == 1)
                {
                    writer.WriteAttributeString("type", "array");
                    writer.WriteAttributeString("arrayType", getTypeName(type.GetGenericArguments()[0]));
                    return;
                }

                //if Dictionary
                Type dictType = typeof(SerializableDictionary<,>);
                if (type.GetGenericArguments().Count() == 2 &&
                    dictType.GetGenericArguments().Length == type.GetGenericArguments().Length &&
                    dictType.MakeGenericType(type.GetGenericArguments()) == type)
                {
                    writer.WriteAttributeString("type", "map");
                    writer.WriteAttributeString("arrayType", getTypeName(type.GetGenericArguments()[1]));
                    return;
                }

                if (type.GetGenericArguments().Count() == 2)
                    throw new Exception("Dont know how to handle");

                throw new Exception("Generic type unknown");
            }

            writer.WriteAttributeString("type", getTypeName(type));
        }

    }

    public class SchemaManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static void Generate(Stream stream)
        {
            var writer = new SchemaWriter(stream);
            writer.write();
        }

        public static bool Validate()
        {
            return true;
        }

        public static bool Validate(Type type)
        {
            return true;
        }

        internal static bool Validate(MethodInfo method)
        {
            var attr = method.GetCustomAttribute<RouteAttribute>(false);
            if (attr == null)
                return false;

            return true;
        }
    }
}