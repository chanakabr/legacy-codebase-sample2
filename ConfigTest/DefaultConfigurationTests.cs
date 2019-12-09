using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConfigurationManager;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Tests.ConfigTest
{
    [TestClass]
    public class DefaultConfigurationTests
    {
        /*        [ClassInitialize]
                public static void ClassInit(TestContext context)

                {
                //    TCMClient.Settings.Instance.Init();
                }*/



        [TestMethod]
        public void Test_AllConfigurationSettingsMustHaveDefultValue()
        {
            ApplicationConfiguration configuration = ApplicationConfiguration.Current;
            Type type = typeof(ApplicationConfiguration);
            TestIfConainNullDefaultValue(type, configuration);


        }

        private void TestIfConainNullDefaultValue(Type type, IBaseConfig baseConfig)
        {
            bool result = true;
            StringBuilder sb = new StringBuilder();
            List<FieldInfo> fields = type.GetFields().ToList();
  
            foreach (var field in fields)
            {
                object baseValueData = field.GetValue(baseConfig);
                if (baseValueData == null)
                {
                    // Assert.Fail($"{baseConfig.ToString()} contains null object");
                    sb.AppendLine($"object [{type.Name}] holds null objcet {field.Name}");
                    result = false;
                }
                else if (field.FieldType.Name.StartsWith("BaseValue"))
                {
                    var key = baseValueData.GetType().GetRuntimeFields().ToList()[0].GetValue(baseValueData);
                    var value = baseValueData.GetType().GetProperty("Value").GetValue(baseValueData);
                    
                    if (value == null || string.IsNullOrEmpty(value.ToString()) )
                    {
                        //Assert.Fail($"{baseConfig.ToString()} contains key with null default value");
                        sb.AppendLine($"object [{type.Name}] holds null default value [{field.Name}], Json key [{key}]");
                        result = false;
                    }
                    else if (int.TryParse(value.ToString(), out var res))
                    {
                        if (res == -1)
                        {
                            sb.AppendLine($"object [{type.Name}] integer problem, Json key [{key}]");
                            //Assert.Fail($"{baseConfig.ToString()} contains key with null default value");
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
                    Assert.Fail($"{baseConfig.ToString()} contains unkown param");
                }
            }

            Console.Out.WriteLine(sb.ToString());
           // Assert.IsTrue(result);
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

/*        [TestMethod]
        public void InitConfigurationUsingReflection()
        {

            var type = typeof(ApplicationConfiguration);
            var baseConfig = Activator.CreateInstance(type);

            var k = ApplicationConfiguration.Current.MetaFeaturesPattern.Value;
        }*/
    }
}
