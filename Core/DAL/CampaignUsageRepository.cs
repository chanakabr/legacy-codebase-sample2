using ApiObjects.Notification;
using ApiObjects.Pricing;
using CouchbaseManager;
using DAL.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OTT.Lib.MongoDB;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DAL
{
    public interface ICampaignUsageRepository
    {
        // campaign inbox message map - for specific user
        bool SaveToCampaignInboxMessageMapCb(long campaignId, int partnerId, long userId, CampaignMessageDetails inboxMessage);
        CampaignInboxMessageMap GetCampaignInboxMessageMapCB(int partnerId, long userId);
        void CleanCampaignInboxMessageMap(int partnerId, long userId, IEnumerable<long> archiveCampaigns, long utcNow);

        DeviceTriggerCampaignsUses GetDeviceTriggerCampainsUses(int partnerId, string udid);
        bool SaveToDeviceTriggerCampaignsUses(int partnerId, string udid, long campaignId, long utcNow);

        // campaign Household Usages
        int? GetCampaignHouseholdUsages(int partnerId, long householdId, long campaignId);
        bool SetCampaignHouseholdUsage(int partnerId, long householdId, long campaignId, DateTime campaignExpiration);
    }

    public class CampaignUsageRepository : ICampaignUsageRepository
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<ICampaignUsageRepository> Lazy = new Lazy<ICampaignUsageRepository>(
            () => new CampaignUsageRepository(
                ClientFactoryBuilder.Instance.GetClientFactory(
                    DBName,
                    CampaignHouseholdUsagesCollectionProperties)), 
            LazyThreadSafetyMode.PublicationOnly);

        public static ICampaignUsageRepository Instance => Lazy.Value;

        private const string CampaignHouseholdUsagesCollectionName = "campaign_household_useges";
        private const string DBName = "campaign_useges";
        public static readonly Dictionary<string, MongoDbConfiguration.CollectionProperties> CampaignHouseholdUsagesCollectionProperties
            = new Dictionary<string, MongoDbConfiguration.CollectionProperties>
            {
                {
                    CampaignHouseholdUsagesCollectionName, new MongoDbConfiguration.CollectionProperties
                    {
                        DisableLogicalDelete = false,
                        DisableAutoTimestamps = false,
                        IndexBuilder = (builder) =>
                        {
                            builder.CreateIndex(o => o.Ascending(f => f.HouseholdId), new MongoDbCreateIndexOptions<CampaignHouseholdUsages>
                            {
                                Unique = true,
                                PartialFilterExpression = b => b.Exists(a => a.HouseholdId) & b.Type(a => a.HouseholdId, "long") & b.Gt<object>(a => a.HouseholdId, 0)
                            });

                            builder.CreateIndex(o => o.Ascending(f => f.CampaignId), new MongoDbCreateIndexOptions<CampaignHouseholdUsages>
                            {
                                Unique = true,
                                PartialFilterExpression = b => b.Exists(a => a.CampaignId) & b.Type(a => a.CampaignId, "long") & b.Gt<object>(a => a.CampaignId, 0)
                            });

                            builder.CreateIndex(o => o.Ascending(f => f.Expiration), new MongoDbCreateIndexOptions<CampaignHouseholdUsages>()
                            {
                                ExpireAfterSeconds = 0,
                                Unique = false
                            });
                        }
                    }
                }
            };

        private readonly IMongoDbClientFactory _clientFactory;
        private readonly CouchbaseManager.CouchbaseManager cbManager;

        public CampaignUsageRepository(IMongoDbClientFactory clientFactory)
        {
            cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.NOTIFICATION);
            _clientFactory = clientFactory;
        }

        // campaign inbox message map - for specific user
        private static string GetInboxMessageCampaignMappingKey(int partnerId, long userId)
        {
            return $"user_inbox_campaigns:{partnerId}:{userId}";
        }

        public bool SaveToCampaignInboxMessageMapCb(long campaignId, int partnerId, long userId, CampaignMessageDetails inboxMessage)
        {
            var key = GetInboxMessageCampaignMappingKey(partnerId, userId);
            var isSaveSuccess = UtilsDal.SaveObjectWithVersionCheckInCB<CampaignInboxMessageMap>(0,
                eCouchbaseBucket.NOTIFICATION, key, mapping =>
                {
                    if (mapping.Campaigns.ContainsKey(campaignId))
                    {
                        mapping.Campaigns[campaignId] = inboxMessage;
                    }
                    else
                    {
                        mapping.Campaigns.Add(campaignId, inboxMessage);
                    }
                }, true);

            return isSaveSuccess;
        }

        public void CleanCampaignInboxMessageMap(int partnerId, long userId, IEnumerable<long> archiveCampaigns, long utcNow)
        {
            var key = GetInboxMessageCampaignMappingKey(partnerId, userId);

            Task.Run(() => UtilsDal.SaveObjectWithVersionCheckInCB<CampaignInboxMessageMap>
                (0, eCouchbaseBucket.NOTIFICATION, key, mapping =>
                {
                    foreach (var id in archiveCampaigns)
                    {
                        mapping.Campaigns.Remove(id);
                    }

                    var idsToDelete = mapping.Campaigns.Where(x => x.Value.ExpiredAt < utcNow).Select(x => x.Key);
                    foreach (var id in idsToDelete)
                    {
                        mapping.Campaigns.Remove(id);
                    }
                }, true))
                .ConfigureAwait(false);

            // var isSaveSuccess = UtilsDal.SaveObjectWithVersionCheckInCB<CampaignInboxMessageMap>(0, eCouchbaseBucket.NOTIFICATION, key, mapping =>
            // {
            //     foreach (var id in archiveCampaigns)
            //     {
            //         mapping.Campaigns.Remove(id);
            //     }
            //     
            //     var idsToDelete = mapping.Campaigns.Where(x => x.Value.ExpiredAt < utcNow).Select(x => x.Key);
            //     foreach (var id in idsToDelete)
            //     {
            //         mapping.Campaigns.Remove(id);
            //     }
            // }, true);
            //
            // return isSaveSuccess;
        }

        public CampaignInboxMessageMap GetCampaignInboxMessageMapCB(int partnerId, long userId)
        {
            CampaignInboxMessageMap campaignInboxMessageMap =
                cbManager.Get<CampaignInboxMessageMap>(GetInboxMessageCampaignMappingKey(partnerId, userId));
            return campaignInboxMessageMap ?? new CampaignInboxMessageMap();
        }

        // campaign usages by device
        private static string GetDeviceTriggerCampainsUsesKey(int partnerId, string udid)
        {
            return $"device_campaign_uses_{partnerId}_{udid}";
        }

        public DeviceTriggerCampaignsUses GetDeviceTriggerCampainsUses(int partnerId, string udid)
        {
            string key = GetDeviceTriggerCampainsUsesKey(partnerId, udid);
            DeviceTriggerCampaignsUses deviceTriggerCampaignsUses = cbManager.Get<DeviceTriggerCampaignsUses>(key);
            return deviceTriggerCampaignsUses;
        }

        public bool SaveToDeviceTriggerCampaignsUses(int partnerId, string udid, long campaignId, long utcNow)
        {
            string key = GetDeviceTriggerCampainsUsesKey(partnerId, udid);
            var isSaveSuccess = UtilsDal.SaveObjectWithVersionCheckInCB<DeviceTriggerCampaignsUses>(60 * 24 * 365,
                eCouchbaseBucket.NOTIFICATION, key, mapping =>
                {
                    if (string.IsNullOrEmpty(mapping.Udid))
                    {
                        mapping.Udid = udid;
                    }

                    if (!mapping.Uses.ContainsKey(campaignId))
                    {
                        mapping.Uses.Add(campaignId, utcNow);
                    }
                }, true);

            return isSaveSuccess;
        }

        // campaign Household Usages
        public int? GetCampaignHouseholdUsages(int partnerId, long householdId, long campaignId)
        {
            var client = _clientFactory.NewMongoDbClient(partnerId, _logger);
            var campaignHouseholdUsages = client.Find<CampaignHouseholdUsages>(CampaignHouseholdUsagesCollectionName, f =>
                f.Where(i => i.HouseholdId.Equals(householdId) && i.CampaignId.Equals(campaignId)))
                .FirstOrDefault();
            return campaignHouseholdUsages?.UsageCount;
        }

        public bool SetCampaignHouseholdUsage(int partnerId, long householdId, long campaignId, DateTime campaignExpiration)
        {
            var client = _clientFactory.NewMongoDbClient(partnerId, _logger);
            
            var data = new CampaignHouseholdUsages()
            {
                CampaignId = campaignId,
                HouseholdId = householdId,
                UsageCount = 1,
                UpdateDate = DateTime.UtcNow,
                Expiration = campaignExpiration//Set as ttl,
            };

            var result = client.UpdateOne<CampaignHouseholdUsages>(
                CampaignHouseholdUsagesCollectionName,
                h => h.Eq(o => o.HouseholdId, householdId) & h.Eq(o => o.CampaignId, campaignId),
                u => GetUpdateDefinition(data, u),
                new MongoDbUpdateOptions { IsUpsert = true });

            if (result.MatchedCount > 1)
            {
                _logger.LogError($"There have been found {result.MatchedCount} {nameof(CampaignHouseholdUsages)}'s documents in the database: {nameof(partnerId)}={partnerId}.");
            }

            var isInserted = !string.IsNullOrEmpty(result.UpsertedId);
            var isUpdated = result.ModifiedCount == 1;
            return isInserted || isUpdated;
        }

        private static UpdateDefinition<CampaignHouseholdUsages> GetUpdateDefinition(CampaignHouseholdUsages data, UpdateDefinitionBuilder<CampaignHouseholdUsages> updateBuilder)
        {
            var updateDefinition = updateBuilder
                .Set(x => x.CampaignId, data.CampaignId)
                .Set(x => x.HouseholdId, data.HouseholdId)
                .Set(x => x.UpdateDate, data.UpdateDate)
                .Set(x => x.Expiration, data.Expiration)
                .Inc(x => x.UsageCount, 1);

            return updateDefinition;
        }
    }
}
