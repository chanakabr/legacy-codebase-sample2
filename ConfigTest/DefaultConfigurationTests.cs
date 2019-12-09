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

        [TestMethod]
        public void Test_AllConfigurationSettingsMustHaveDefultValue()
        {

            ApplicationConfiguration configuration = ApplicationConfiguration.Current;
            Type type = typeof(ApplicationConfiguration);
            var result = TestIfConainNullDefaultValue(type, configuration);
            Assert.IsTrue(result);

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
                    var key = baseValueData.GetType().GetRuntimeFields().ToList()[0].GetValue(baseValueData);
                    var value = baseValueData.GetType().GetProperty("Value").GetValue(baseValueData);
                    
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
