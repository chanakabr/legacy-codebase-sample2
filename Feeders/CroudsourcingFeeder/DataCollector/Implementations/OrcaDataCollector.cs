using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using ApiObjects.CrowdsourceItems;
using ApiObjects.CrowdsourceItems.Base;
using ApiObjects.CrowdsourceItems.Implementations;
using CrowdsourcingFeeder.DataCollector.Base;
using CrowdsourcingFeeder.WS_Catalog;
using KLogMonitor;
using Newtonsoft.Json;

namespace CrowdsourcingFeeder.DataCollector.Implementations
{
    internal class OrcaDataCollector : BaseDataCollector
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private eGalleryType _galleryType;

        public OrcaDataCollector(int groupId, eGalleryType galleryType)
            : base(0, groupId, eCrowdsourceType.Orca)
        {
            _galleryType = galleryType;
        }

        protected override int[] Collect()
        {
            try
            {
                WebClient webClient = new WebClient();
                TVPApiRequest request = new TVPApiRequest()
                {
                    initObj = new TVPApiRequest.InitObj()
                    {
                        ApiUser = string.Format("tvpapi_{0}", GroupId),
                        ApiPass = "11111",
                        Platform = "iPad",
                        Locale = new TVPApiRequest.Locale()
                        {
                            LocaleUserState = "Unknown"
                        }
                    },
                    galleryType = _galleryType.ToString(),
                    parentalLevel = 0,
                    picSize = "",
                };
                string respJson = webClient.UploadString(TVinciShared.WS_Utils.GetTcmConfigValue(string.Format("crowdsourcer.{0}_TVPApiURL", GroupId)), JsonConvert.SerializeObject(request));
                TVPApiResponse response = JsonConvert.DeserializeObject<TVPApiResponse>(respJson);
                return response.Content.Select(x => int.Parse(x.MediaID)).ToArray();
            }
            catch (Exception ex)
            {
                log.Error("Crowdsource - " + string.Format("Collector: {0} - Error collecting items - Exception: \n {1}", CollectorType, ex.Message), ex);
                return null;
            }
        }

        protected override Dictionary<int, BaseCrowdsourceItem> Normalize(SingularItem item)
        {
            try
            {

                Dictionary<int, BaseCrowdsourceItem> retDict = null;
                Dictionary<LanguageObj, MediaResponse> mediaInfDict = GetLangAndInfo(GroupId, item.Id);
                if (mediaInfDict != null)
                {
                    retDict = new Dictionary<int, BaseCrowdsourceItem>();
                    foreach (KeyValuePair<LanguageObj, MediaResponse> mediaInfo in mediaInfDict)
                    {
                        if (mediaInfo.Value.m_lObj[0] != null)
                        {
                            OrcaItem csItem = new OrcaItem()
                            {
                                MediaId = ((MediaObj)mediaInfo.Value.m_lObj[0]).m_nID,
                                MediaName = ((MediaObj)mediaInfo.Value.m_lObj[0]).m_sName,
                                MediaImage = ((MediaObj)mediaInfo.Value.m_lObj[0]).m_lPicture.Select(pic => new BaseCrowdsourceItem.Pic()
                                {
                                    Size = pic.m_sSize,
                                    URL = pic.m_sURL
                                }).ToArray(),
                                Order = item.Order,
                                TimeStamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow)
                            };
                            retDict.Add(mediaInfo.Key.ID, csItem);
                        }

                    }
                }
                return retDict;

            }
            catch (Exception ex)
            {
                log.Error("Crowdsource - " + string.Format("Collector: {0} - Error normalizing singular item - Exception: \n {1}", CollectorType, ex.Message), ex);
                return null;
            }
        }
    }

    public class TVPApiRequest
    {
        public InitObj initObj { get; set; }
        public int mediaID { get; set; }
        public string picSize { get; set; }
        public int parentalLevel { get; set; }
        public string galleryType { get; set; }

        public class Locale
        {
            public string LocaleLanguage { get; set; }
            public string LocaleCountry { get; set; }
            public string LocaleDevice { get; set; }
            public string LocaleUserState { get; set; }
        }

        public class InitObj
        {
            public Locale Locale { get; set; }
            public string Platform { get; set; }
            public string SiteGuid { get; set; }
            public int DomainID { get; set; }
            public string UDID { get; set; }
            public string ApiUser { get; set; }
            public string ApiPass { get; set; }
        }
    }




    public class TVPApiResponse
    {
        public List<Media> Content { get; set; }

        public class Media
        {
            public string MediaID { get; set; }
        }
    }
}
