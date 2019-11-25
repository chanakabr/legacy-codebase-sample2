using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tests.ConfigTest
{
    [TestClass]
    public class DefaultConfigurationTests
    {
        [TestMethod]
        public void Test_AllConfigurationSettingsHaveDefultValue()
        {
            var type = typeof(IBaseConfig);
 
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract).ToList();
            foreach (Type tt in types)
            {
                var baseConfig = Activator.CreateInstance(tt);
                List<FieldInfo> fields = baseConfig.GetType().GetFields().ToList();
                foreach (var field in fields)
                {
                    object value = field.GetValue(baseConfig);
                    if(field.FieldType == typeof(BaseValue<string>))
                    {

                    }
                    if (value != null && !string.IsNullOrEmpty(value.ToString()))
                    {

                    }
                    else
                    {
                        Assert.Fail($"field {field.Name} is null under path {tt.FullName}. fields couldn't be null");
                    }
                }

            }
        }
    }
}
