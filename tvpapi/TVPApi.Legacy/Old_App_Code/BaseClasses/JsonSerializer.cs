using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;


public class JsonSerializerForKnownTypes : IDataContractSurrogate
{
    private const String DATE_FORMAT = "dd/MM/yyyy HH:mm:ss";
    private const String DATE_TIME_FORMAT = "{0:dd/MM/yyyy HH:mm:ss}";
    public JsonSerializerForKnownTypes()
	{
		
	}

    public object GetCustomDataToExport(Type clrType, Type dataContractType)
    {
        return null;
    }

    public object GetCustomDataToExport(System.Reflection.MemberInfo memberInfo, Type dataContractType)
    {
        return null;
    }

    public Type GetDataContractType(Type type)
    {        
        return type;
    }

    public object GetDeserializedObject(object obj, Type targetType)
    {
        if (obj is String && targetType == typeof(DateTime))
        {
            return DateTime.ParseExact((string)obj, DATE_FORMAT, null);
        }
        return obj;
    }

    public void GetKnownCustomDataTypes(System.Collections.ObjectModel.Collection<Type> customDataTypes)
    {
        throw new NotImplementedException();
    }

    public object GetObjectToSerialize(object obj, Type targetType)
    {
        if ((!targetType.FullName.StartsWith("System") || targetType.Name == "DateTime") &&
            targetType.IsClass && 
            targetType.Name != "String")
        {
            do
            {
                if( targetType.Name == "DateTime" )
                {
                    obj = DateTime.ParseExact(String.Format(DATE_TIME_FORMAT,obj),DATE_FORMAT,null);
                    break;
                }

                foreach( FieldInfo field in targetType.GetFields())
                {
                    if (field.FieldType.Name == "DateTime")
                    {
                        DateTime date = (DateTime)targetType.GetField(field.Name).GetValue(obj);
                        if (date != null)
                        {
                            DateTime convertedDateTime = DateTime.ParseExact(String.Format(DATE_TIME_FORMAT, date), DATE_FORMAT, null);
                            targetType.GetField(field.Name).SetValue(obj, convertedDateTime);
                        }
                    }                    
                } 
            }while(false);                       
        }
        return obj;
    }

    public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
    {
        return null;
    }

    public System.CodeDom.CodeTypeDeclaration ProcessImportedType(System.CodeDom.CodeTypeDeclaration typeDeclaration, System.CodeDom.CodeCompileUnit compileUnit)
    {
        return typeDeclaration;
    }
}