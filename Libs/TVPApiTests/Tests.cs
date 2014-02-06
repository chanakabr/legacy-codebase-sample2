//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Newtonsoft.Json;
//using NUnit.Framework;

//namespace TVPApiTests
//{
//    [SetUpFixture]
//    public class SetupFixtureClass
//    {
//        [SetUp]
//        public void StartTesting()
//        {
//            //System.Diagnostics.Debugger.Launch();
//        }
//    }

//    [TestFixture]
//    public class Tests
//    {
//        TestHelper testHelper = new TestHelper();

//        [Test]
//        public void GetMediaInfo()
//        {
//            List<KeyValuePair<string, object>> requestParams = new List<KeyValuePair<string,object>>();

//            requestParams.Add(new KeyValuePair<string, object>("mediaId", (long)232429));
//            requestParams.Add(new KeyValuePair<string, object>("mediaType", 0));
//            requestParams.Add(new KeyValuePair<string, object>("picSize", "full"));
//            requestParams.Add(new KeyValuePair<string, object>("withDynamic", false));

//            try
//            {
//                testHelper.TestSanity("GetMediaInfo", requestParams, typeof(TVPApi.Media));
//            }
//            catch (Exception ex)
//            {
//                Assert.Fail(ex.Message);
//            }
//        }

//        [Test]
//        public void GetMediasInfo()
//        {
//            List<KeyValuePair<string, object>> requestParams = new List<KeyValuePair<string, object>>();

//            requestParams.Add(new KeyValuePair<string, object>("MediaID", new long[] { 232429, 232430 }));
//            requestParams.Add(new KeyValuePair<string, object>("mediaType", 0));
//            requestParams.Add(new KeyValuePair<string, object>("picSize", "full"));
//            requestParams.Add(new KeyValuePair<string, object>("withDynamic", false));

//            try
//            {
//                testHelper.TestSanity("GetMediasInfo", requestParams, typeof(TVPApi.Media[]));
//            }
//            catch (Exception ex)
//            {
//                Assert.Fail(ex.Message);
//            }
//        }
//    }
//}
