using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using ApiObjects;
using ApiObjects.CrowdsourceItems;
using ApiObjects.CrowdsourceItems.Base;
using CrowdsourcingFeeder.WS_Catalog;
using DAL;
using Tvinci.Core.DAL;
using TVinciShared;

namespace CrowdsourcingFeeder.DataCollector.Base
{
    public abstract class BaseDataCollector
    {

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
                            DAL.CrowdsourceDAL.SetLastItemId(GroupId, CollectorType, AssetId, SelectedItem.Id);
                        }
                        return true;
                    }
                    Logger.Logger.Log("Crowdsource", string.Format("{0}: {1} - Error selecting singular item", DateTime.UtcNow, CollectorType), "Crowdsourcing.log");
                    return false;
                }
                Logger.Logger.Log("Crowdsource", string.Format("{0}: {1} - 0 ItemsCollected", DateTime.UtcNow, CollectorType), "Crowdsourcing.log");
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
                        int randomLocation = rand.Next(0, collectedItems.Length - 1);
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
                Logger.Logger.Log("Crowdsource", string.Format("{0}: {1} - Error selecting singular item. groupId:{2} ex: \n {3}",
                    DateTime.UtcNow, CollectorType, GroupId, ex.Message), "Crowdsourcing.log");
                return null;
            }
        }

        protected bool UpdateDataStore(Dictionary<int, BaseCrowdsourceItem> itemsByLangDict)
        {
            if (itemsByLangDict != null)
            {
                foreach (KeyValuePair<int, BaseCrowdsourceItem> croudsourceItem in itemsByLangDict)
                {
                    if (croudsourceItem.Value != null)
                        CrowdsourceDAL.UpdateCsList(GroupId, croudsourceItem);
                    else
                        Logger.Logger.Log("Crowdsource", string.Format("{0}: {1} - Media info missing:{2} MediaId={3} Language={4}",
                            DateTime.UtcNow, CollectorType, GroupId, SelectedItem.Id, croudsourceItem.Key), "Crowdsourcing.log");
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
                                m_lMediasIds = new[] {mediaId},
                                m_nGroupID = GroupId,
                                m_oFilter = new Filter()
                                {
                                    m_nLanguage = languageObj.ID,
                                },
                                m_sSignString = catalogSignString,
                                m_sSignature = WS_Utils.GetCatalogSignature(catalogSignString, WS_Utils.GetTcmConfigValue("signKey")),
                            });
                            retDict.Add(languageObj, mediaInfoForLanguage);
                        }
                    }
                }
                return retDict;

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Crowdsource", string.Format("{0}: {1} - Error getting item info. groupId:{2}, mediaId:{3} ex: \n {4}",
                    DateTime.UtcNow, CollectorType, GroupId, mediaId, ex.Message), "Crowdsourcing.log");
                return null;
            }
        }

        private IserviceClient GetCatalogClient()
        {
            string catalogUrl = WS_Utils.GetTcmConfigValue("WS_Catalog");
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

