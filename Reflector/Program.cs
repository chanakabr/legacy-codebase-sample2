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
using System.Text.RegularExpressions;
using WebAPI.Models.Renderers;

namespace Reflector
{
    class Program
    {
        static void Main(string[] args)
        {
            DataModel dataModel = new DataModel();
            dataModel.write();

            Serializer serializer = new Serializer();
            serializer.write();

            Deserializer deserializer = new Deserializer();
            deserializer.write();
        }
    }
}
