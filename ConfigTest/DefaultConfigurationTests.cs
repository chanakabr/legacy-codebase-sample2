using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConfigurationManager;
using Newtonsoft.Json.Linq;

namespace Tests.ConfigTest
{
    [TestClass]
    public class DefaultConfigurationTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)

        {
        //    TCMClient.Settings.Instance.Init();
        }



        [TestMethod]
        public void Test_AllConfigurationSettingsMustHaveDefultValue()
        {
            ApplicationConfiguration configuration = ApplicationConfiguration.Current;
            Type type = typeof(ApplicationConfiguration);
            TestIfConainNullDefaultValue(type, configuration);


        }

        private void TestIfConainNullDefaultValue(Type type, IBaseConfig baseConfig)
        {
            List<FieldInfo> fields = type.GetFields().ToList();
  
            foreach (var field in fields)
            {
                object baseValueData = field.GetValue(baseConfig);
                if (baseValueData == null)
                {
                    Assert.Fail($"{baseConfig.ToString()} contains null object");
                }
                if (field.FieldType.Name.StartsWith("BaseValue"))
                {
                    var value = baseValueData.GetType().GetProperty("Value").GetValue(baseValueData);

                    if (value == null || string.IsNullOrEmpty(value.ToString()) )
                    {
                        Assert.Fail($"{baseConfig.ToString()} contains key with null default value");
                    }
                    if (int.TryParse(value.ToString(), out var res))
                    {
                        if (res == -1)
                        {
                            Assert.Fail($"{baseConfig.ToString()} contains key with null default value");
                        }
                    }
                }
                else if (field.FieldType == type && field.Name == "Current")
                {
                    continue;
                }
                else if (field.FieldType.BaseType.Name.StartsWith("BaseConfig") && field.FieldType.GetInterface("IBaseConfig") != null)
                {
                    TestIfConainNullDefaultValue(field.FieldType, baseValueData as IBaseConfig);
                }
                else
                {
                    Assert.Fail($"{baseConfig.ToString()} contains unkown param");
                }
            }
        }


  


        [TestMethod]
        public void InitConfigurationUsingReflection()
        {

            var type = typeof(ApplicationConfiguration);
            var baseConfig = Activator.CreateInstance(type);

            var k = ApplicationConfiguration.Current.MetaFeaturesPattern.Value;
        }
    }
}
