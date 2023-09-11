using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ApiObjects;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using ApiObjects.Rules;
using AutoMapper.Configuration;
using Core.Catalog;
using Core.ConditionalAccess;
using Core.Pricing;
using GrpcAPI.Services;
using GrpcAPI.Utils;
using MoreLinq;
using MoreLinq.Extensions;
using ProtoBuf;
using ProtoBuf.Meta;
using WebAPI.Models.Catalog;
using KeyValuePair = ApiObjects.KeyValuePair;
using MetaType = ProtoBuf.Meta.MetaType;

namespace Generator.Grpc
{
    class Program
    {
        static void Main(string[] args)
        {
            generateProto<RuleActionType>();
        }
        
        public static void generateProto<T>()
        {
            string protoSyntx = "syntax = \"proto3\";";
            string namespacePhoenix = "option csharp_namespace = \"phoenix\";";
            string goPackage = "option go_package = \"./phoenix\";";
            string packagePhoenix = "package phoenix;";
            
            var objectType = typeof(T);

            // AddTypeToModel<T>(RuntimeTypeModel.Default);
            
            var proto = ProtoBuf.Serializer.GetProto<T>();
            var file = new StreamWriter(GetProtoSerializerCSFilePath(objectType.Name));
            // remove the syntax row and rewrite it again with all the data needed
            proto = proto.Substring(proto.IndexOf(protoSyntx, 0) + protoSyntx.Length);
            file.WriteLine(protoSyntx);
            file.WriteLine(namespacePhoenix);
            file.WriteLine(goPackage);
            file.WriteLine(packagePhoenix);
            file.WriteLine(proto);
            file.Close();
        }
        private static MetaType AddTypeToModel<T>(RuntimeTypeModel typeModel)
        {            
            var properties = typeof(T).GetProperties().Select(p => p.Name).OrderBy(name => name);//OrderBy added, thanks MG
            return typeModel.Add(typeof(T), false).Add(properties.ToArray());            
        }
        
        public static string GetProtoSerializerCSFilePath(string className)
        {
            var currentLocation = AppDomain.CurrentDomain.BaseDirectory;
            var solutionDir = Directory.GetParent(currentLocation).Parent.Parent.Parent.Parent.Parent;
            var filePath = Path.Combine(solutionDir.FullName, "Core", "GrpcAPI", "definition","api", $"{className}.proto");
            return filePath;
        }
    }
}