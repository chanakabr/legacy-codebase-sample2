using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConfigurationManager;
using System.Text;

namespace Tests.ConfigTest
{
    [TestClass]
    public class DefaultConfigurationTests
    {
        const int BaseValueKeyLocation = 0;
        const int BaseValueDefaultValueLocation = 1;
        const int BaseValueMustBeOverwriteInTcmLocation = 2;

        [TestMethod]
        public void Test_AllConfigurationSettingsMustHaveDefultValue()
        {

            ApplicationConfiguration configuration = ApplicationConfiguration.Current;
            Type type = typeof(ApplicationConfiguration);
            var result = TestIfConainNullDefaultValue(type, configuration);
            Assert.IsTrue(result);

        }


        [TestMethod]
        public void Test_StubInCaseOfMustBeOverwriteInTcmInTcm()
        {
            ApplicationConfiguration configuration = ApplicationConfiguration.Current;
            Type type = typeof(ApplicationConfiguration);
            TestStubKey(type, configuration);
        }



        [TestMethod]
        public void Test_ValidateDuplicateKeys()
        {
            ApplicationConfiguration configuration = ApplicationConfiguration.Current;
            Type type = typeof(ApplicationConfiguration);
            TestIfConainsDuplicateKey(type, configuration);
        }


        private void TestStubKey(Type type, IBaseConfig baseConfig)
        {
            List<FieldInfo> fields = type.GetFields().ToList();
            foreach (var field in fields)
            {
                object baseValueData = field.GetValue(baseConfig);
                if (baseValueData == null)
                {

                    Assert.Fail($"object [{type.Name}] holds null objcet {field.Name}");
                }
                else if (field.FieldType.Name.StartsWith("BaseValue"))
                {

                    try
                    {
                        var value = baseValueData.GetType().GetRuntimeFields().ToList()[BaseValueDefaultValueLocation].GetValue(baseValueData) as string;
                        bool? mustBeOverwriteInTcmLocation = baseValueData.GetType().GetRuntimeFields().ToList()[BaseValueMustBeOverwriteInTcmLocation].GetValue(baseValueData) as bool?;
                        if ( mustBeOverwriteInTcmLocation.Value && value != TcmObjectKeys.Stub)
                        {
                            Assert.Fail($"case of mustBeOverwriteInTcm key must be Stub key.  object: [{type.Name}]");
                        }
                    }
                    catch
                    {

                    }
                }
                else if (field.FieldType == type && field.Name == "Current")
                {
                    continue;
                }
                else if (IsBaseStartWithName(field.FieldType, "BaseConfig") &&
                    field.FieldType.GetInterface("IBaseConfig") != null)
                {
                    TestStubKey(field.FieldType, baseValueData as IBaseConfig);
                }
                else
                {
                    Assert.Fail($"{baseConfig.ToString()} contains unkown param");
                }
            }
        }

        private void TestIfConainsDuplicateKey(Type type, IBaseConfig baseConfig)
        {
            List<FieldInfo> fields = type.GetFields().ToList();
            Dictionary<string,string> uniqueKeys = new Dictionary<string,string>();
            foreach (var field in fields)
            {
                object baseValueData = field.GetValue(baseConfig);
                if (baseValueData == null)
                {

                    Assert.Fail($"object [{type.Name}] holds null objcet {field.Name}");
                }
                else if (field.FieldType.Name.StartsWith("BaseValue"))
                {
                    var key = baseValueData.GetType().GetRuntimeFields().ToList()[BaseValueKeyLocation].GetValue(baseValueData) as string;
                    if (uniqueKeys.TryGetValue(key, out var decleredType))
                    {
                        if (decleredType == field.DeclaringType.Name)
                        {
                            Assert.Fail($"Duplicate key exist for the same object: [{type.Name}], Key; {key}");
                        }
                        continue;
                    }
                    uniqueKeys.Add(key,field.DeclaringType.Name);
                }
                else if (field.FieldType == type && field.Name == "Current")
                {
                    continue;
                }
                else if (IsBaseStartWithName(field.FieldType, "BaseConfig") &&
                    field.FieldType.GetInterface("IBaseConfig") != null)
                {
                    TestIfConainsDuplicateKey(field.FieldType, baseValueData as IBaseConfig);
                }
                else
                {
                   Assert.Fail($"{baseConfig.ToString()} contains unkown param");
                }
            }
        }

        private bool TestIfConainNullDefaultValue(Type type, IBaseConfig baseConfig)
        {
            bool result = true;
            StringBuilder sb = new StringBuilder();
            List<FieldInfo> fields = type.GetFields().ToList();
  
            foreach (var field in fields)
            {
                object baseValueData = field.GetValue(baseConfig);
                if (baseValueData == null)
                {
                    sb.AppendLine($"object [{type.Name}] holds null objcet {field.Name}");
                    result = false;
                }
                else if (field.FieldType.Name.StartsWith("BaseValue"))
                {
                    var key = baseValueData.GetType().GetRuntimeFields().ToList()[BaseValueKeyLocation].GetValue(baseValueData);
                    var value = baseValueData.GetType().GetRuntimeFields().ToList()[BaseValueDefaultValueLocation].GetValue(baseValueData);

                    if (value == null || string.IsNullOrEmpty(value.ToString()) )
                    {
                        sb.AppendLine($"object [{type.Name}] holds null default value [{field.Name}], Json key [{key}]");
                        result = false;
                    }
                    else if (int.TryParse(value.ToString(), out var res))
                    {
                        if (res == -1)
                        {
                            sb.AppendLine($"object [{type.Name}] integer problem, Json key [{key}]");
                            result = false;
                        }
                    }
                }
                else if (field.FieldType == type && field.Name == "Current")
                {
                    continue;
                }
                else if (IsBaseStartWithName(field.FieldType, "BaseConfig") &&
                    field.FieldType.GetInterface("IBaseConfig") != null)
                {
                    TestIfConainNullDefaultValue(field.FieldType, baseValueData as IBaseConfig);
                }
                else
                {
                    result = false;
                    sb.AppendLine($"{baseConfig.ToString()} contains unkown param");
                }
            }
            Console.Out.Write(sb.ToString());
            return result;
           
        }



        private static bool IsBaseStartWithName(Type fieldType, string typeName)
        {
            while (fieldType != typeof(object))
            {
                if (fieldType.Name.StartsWith(typeName))
                {
                    return true;
                }
                fieldType = fieldType.BaseType;
            }
            return false;
        }

    }
}
