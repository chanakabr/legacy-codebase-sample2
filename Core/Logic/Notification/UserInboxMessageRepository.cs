using ApiObjects;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using OTT.Lib.MongoDB;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Notification
{
    public interface IUserInboxMessageStatusRepository
    {
        UserInboxMessageStatus GetMessageStatus(int groupId, string messageId);
        UserInboxMessageStatus UpsertStatus(int groupId, int userId, string messageId, eMessageState status, DateTime expiration);
        Dictionary<string, UserInboxMessageStatus> GetMessageStatuses(int groupId, int userId);
    }

    public class UserInboxMessageStatusRepository : IUserInboxMessageStatusRepository
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string CollectionName = "user_message_statuses";
        private const string DBName = "user_inbox";
        private IMongoDbClientFactory _service;

        public UserInboxMessageStatusRepository(string connectionString)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMongoDbClientFactory(new MongoDbConfiguration
            {
                ConnectionString = connectionString,
                CollectionProps =
                {
                    {
                        CollectionName, new MongoDbConfiguration.CollectionProperties
                        {
                            DisableLogicalDelete = false,
                            DisableAutoTimestamps = false,
                            IndexBuilder = (builder) =>
                            {
                                builder.CreateIndex(o =>
                                    o.Ascending(f => f.UserId)
                                    .Ascending(f=> f.MessageId), GetIndexOption());

                                builder.CreateIndex(o =>
                                    o.Ascending(f => f.Expiration), ExpireIndex());
                            }
                        }
                    }
                }
            }, DBName);

            var p = serviceCollection.BuildServiceProvider();
            _service = p.GetService<IMongoDbClientFactory>();
        }

        private MongoDbCreateIndexOptions<UserInboxMessageStatus> GetIndexOption()
        {
            var option = new MongoDbCreateIndexOptions<UserInboxMessageStatus>
            {
                Unique = true,
                PartialFilterExpression = b => b.Exists(a => a.UserId) & b.Type(a => a.UserId, "int")
                & b.Gt<object>(a => a.UserId, 0)
            };
            return option;
        }

        #region Methods
        public UserInboxMessageStatus UpsertStatus(int groupId, int userId, string messageId, eMessageState status, DateTime expiration)
        {
            var factory = _service.NewMongoDbClient(groupId, log);
            var current = GetMessageStatus(groupId, messageId);
            if (current != null)
            {
                current.Expiration = expiration;
                var updateResult = factory.UpdateOne<UserInboxMessageStatus>(
                    CollectionName,
                        f => f.Eq(o => o.MessageId, messageId),
                        u => u.Set(o => o.Status, status.ToString())
                    );

                updateResult = factory.UpdateOne<UserInboxMessageStatus>(
                    CollectionName,
                        f => f.Eq(o => o.MessageId, messageId),
                        u => u.Set(o => o.Expiration, expiration)//Set ttl
                    );

                current.Status = status.ToString();
                return current;
            }
            else
            {
                var item = new UserInboxMessageStatus()
                {
                    MessageId = messageId,
                    Status = status.ToString(),
                    UserId = userId,
                    UpdateDate = DateTime.UtcNow,
                    Expiration = expiration//Set ttl
                };

                factory.InsertOne(CollectionName, item);
                return item;
            }
        }

        public UserInboxMessageStatus GetMessageStatus(int groupId, string messageId)
        {
            var factory = _service.NewMongoDbClient(groupId, log);
            var messageStatus =
                factory.Find<UserInboxMessageStatus>(CollectionName, f =>
                f.Where(i => i.MessageId.Equals(messageId)))
                .FirstOrDefault();
            return messageStatus;
        }

        public List<UserInboxMessageStatus> GetGroupMessageStatuses(int groupId)
        {
            var factory = _service.NewMongoDbClient(groupId, log);
            return factory.Find<UserInboxMessageStatus>(CollectionName, f => f.Empty).ToList();
        }

        public Dictionary<string, UserInboxMessageStatus> GetMessageStatuses(int groupId, int userId)
        {
            var factory = _service.NewMongoDbClient(groupId, log);
            return factory.Find<UserInboxMessageStatus>(CollectionName, 
                f => f.Where(i => i.UserId.Equals(userId)))?
                .ToDictionary(x => x.MessageId, x => x);
        }

        private MongoDbCreateIndexOptions<UserInboxMessageStatus> ExpireIndex()
        {
            //https://github.com/kaltura/ott-service-offers/blob/master/logic/clients/mongo_client.go#L67
            return new MongoDbCreateIndexOptions<UserInboxMessageStatus>()
            {
                ExpireAfterSeconds = 0,
                Unique = false
            };
        }

        #endregion
    }
}
