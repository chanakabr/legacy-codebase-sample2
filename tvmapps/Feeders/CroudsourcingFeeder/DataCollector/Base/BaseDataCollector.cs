using ApiObjects;
using ApiObjects.CrowdsourceItems;
using ApiObjects.CrowdsourceItems.Base;
using ConfigurationManager;
using CrowdsourcingFeeder.WS_Catalog;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using Tvinci.Core.DAL;
using TVinciShared;

namespace CrowdsourcingFeeder.DataCollector.Base
{
    public abstract class BaseDataCollector
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public bool ProccessCroudsourceTask()
        {
            using (CatalogClient = GetCatalogClient())
            {
                int[] collectedItems = Collect();
                if (collectedItems != null && collectedItems.Length > 0)
                {
                    SelectedItem = SelectSingularItem(collectedItems);
                    if (SelectedItem != null)
                    {
                        Dictionary<int, BaseCrowdsourceItem> itemsByLangDict = Normalize(SelectedItem);
                        if (UpdateDataStore(itemsByLangDict))
                        {
                            return DAL.CrowdsourceDAL.SetLastItemId(GroupId, CollectorType, AssetId, SelectedItem.Id);
                        }
                        else return false;
                    }
                    log.Debug("Crowdsource - " + string.Format("Collector: {0} - Error selecting singular item", CollectorType));
                    return false;
                }
                log.Debug("Crowdsource - " + string.Format("Collector: {0} - 0 ItemsCollected", CollectorType));
                return false;
            }
        }

        #region Members
        internal IserviceClient CatalogClient;
        internal int AssetId;
        internal int GroupId;
        internal eCrowdsourceType CollectorType;
        internal SingularItem SelectedItem;
        #endregion

        #region Ctor

        protected BaseDataCollector(int assetId, int groupId, eCrowdsourceType collectorType)
        {
            this.AssetId = assetId;
            this.GroupId = groupId;
            this.CollectorType = collectorType;
        }

        #endregion

        #region abstract methods

        protected abstract int[] Collect();
        protected abstract Dictionary<int, BaseCrowdsourceItem> Normalize(SingularItem item);

        #endregion

        #region common methods

        protected SingularItem SelectSingularItem(int[] collectedItems)
        {
            try
            {
                SingularItem retVal = null;
                if (collectedItems != null)
                {
                    int lastItem = CrowdsourceDAL.GetLastItemId(GroupId, CollectorType, AssetId);
                    int tryCount = 0;
                    while (tryCount < 5)
                    {
                        Random rand = new Random();
                        int randomLocation = rand.Next(0, collectedItems.Length);
                        int newId = collectedItems[randomLocation];
                        if (newId != lastItem)
                        {
                            retVal = new SingularItem()
                            {
                                Id = newId,
                                Order = randomLocation
                            };
                            break;
                        }
                        tryCount++;
                    }
                }
                return retVal;
            }
            catch (Exception ex)
            {
                log.Error("Crowdsource - " + string.Format("Collector: {0} - Error selecting singular item. groupId:{1} ex: \n {2}", CollectorType, GroupId, ex.Message), ex);
                return null;
            }
        }

        protected bool UpdateDataStore(Dictionary<int, BaseCrowdsourceItem> itemsByLangDict)
        {
            if (itemsByLangDict != null && itemsByLangDict.Count > 0)
            {
                foreach (KeyValuePair<int, BaseCrowdsourceItem> croudsourceItem in itemsByLangDict)
                {
                    if (croudsourceItem.Value != null)
                        CrowdsourceDAL.UpdateCsList(GroupId, croudsourceItem);
                    else
                        log.Debug("Crowdsource - " + string.Format("Collector: {0} - Media info missing:{1} MediaId={2} Language={3}", CollectorType, GroupId, SelectedItem.Id, croudsourceItem.Key));
                }
                return true;
            }
            return false;
        }

        internal Dictionary<LanguageObj, MediaResponse> GetLangAndInfo(int groupId, int mediaId)
        {
            try
            {
                Dictionary<LanguageObj, MediaResponse> retDict = null;
                List<LanguageObj> languageList = CatalogDAL.GetGroupLanguages(GroupId);
                if (languageList != null)
                {
                    retDict = new Dictionary<LanguageObj, MediaResponse>();
                    using (IserviceClient client = GetCatalogClient())
                    {
                        foreach (LanguageObj languageObj in languageList)
                        {
                            string catalogSignString = Guid.NewGuid().ToString();
                            MediaResponse mediaInfoForLanguage = client.GetMediasByIDs(new MediasProtocolRequest()
                            {
                                m_lMediasIds = new[] { mediaId },
                                m_nGroupID = GroupId,
                                m_oFilter = new Filter()
                                {
                                    m_nLanguage = languageObj.ID,
                                },
                                m_sSignString = catalogSignString,
                                m_sSignature = WS_Utils.GetCatalogSignature(catalogSignString, ApplicationConfiguration.CatalogSignatureKey.Value),
                            });
                            retDict.Add(languageObj, mediaInfoForLanguage);
                        }
                    }
                }
                return retDict;

            }
            catch (Exception ex)
            {
                log.Error("Crowdsource - " + string.Format("Collector: {0} - Error getting item info. groupId:{1}, mediaId:{2} ex: \n {3}", CollectorType, GroupId, mediaId, ex.Message), ex);
                return null;
            }
        }

        private IserviceClient GetCatalogClient()
        {
            string catalogUrl = WS_Utils.GetTcmConfigValue("catalog_ws");
            Uri serviceUri = new Uri(catalogUrl);
            EndpointAddress endpointAddress = new EndpointAddress(serviceUri);
            WSHttpBinding binding = new WSHttpBinding
            {
                Security =
                {
                    Mode = SecurityMode.None,
                    Transport =
                    {
                        ClientCredentialType = HttpClientCredentialType.None
                    }
                },
                UseDefaultWebProxy = true
            };
            IserviceClient client = new IserviceClient(binding, endpointAddress);
            return client;
        }

        #endregion

    }
}

