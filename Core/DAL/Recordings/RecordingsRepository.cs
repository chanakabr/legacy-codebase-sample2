using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using ApiObjects.Recordings;
using DAL.MongoDB;
using MongoDB.Driver;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using OTT.Lib.MongoDB;
using Phx.Lib.Log;

namespace DAL.Recordings
{
    public interface IRecordingsRepository
    {
        TimeBasedRecording GetRecordingByKey(int partnerId, String key);
        TimeBasedRecording GetRecordingById(int partnerId, long id);
        List<TimeBasedRecording> GetRecordingsByKeys(int partnerId, List<string> Keys);
        List<TimeBasedRecording> GetRecordingsByEpgId(int partnerId, long epgId);

        List<TimeBasedRecording> GetRecordingsByEpgIdAndStatus(int partnerId, long epgId,
            string status);

        long AddRecording(int partnerId, TimeBasedRecording recording);
        bool UpdateRecording(int partnerId, TimeBasedRecording timeBasedRecording);
        bool UpdateRecordingStatus(int partnerId, long id, string recordingState);
        List<TimeBasedRecording> GetAndUpdateExpiredRecordings(int partnerId, long expiredTimeWindow);
        bool DeleteRecording(int partnerId, string recordingKey);

        Program GetProgramByEpg(int partnerId, long epgId);
        long AddProgram(int partnerId, Program program);
        bool DeleteProgram(int partnerId, long id);
        bool UpdateProgram(int partnerId, Program program);
        List<Program> GetProgramsByEpgIds(int partnerId, List<long> epgId);
        List<Program> GetProgramsByEpgIdIdsAndStartDate(int partnerId, List<long> epgId, DateTime startDate);
        List<Program> GetProgramsByProgramIds(int partnerId, List<long> programId);

        long AddHouseholdRecording(int partnerId, HouseholdRecording householdRecording);
        bool UpdateHouseholdRecording(int partnerId, HouseholdRecording householdRecording);
        bool IsHouseholdRecordingExists(int partnerId, string recordingKey, long householdId);
        HouseholdRecording GetHouseholdRecording(int partnerId, string recordingKey, long householdId);
        List<HouseholdRecording> GetHhRecordingsByKey(int partnerId, string key, string status);

        HouseholdRecording GetHouseholdRecordingById(int partnerId, long householdRecordingId, long householdId,
            string status);

        List<HouseholdRecording> GetHhRecordingsByHhIdAndRecordingStatuses(int partnerId, long householdId,
            List<string> recordingStatuses);

        List<long> GetHhRecordingsFailuresByKey(int partnerId, string key);

        List<HouseholdRecording> UpdateHouseholdRecordingsStatus(int partnerId, List<long> domainRecordingIds,
            string recordingState);

        bool UpdateHouseholdRecordingsFailure(int partnerId, List<long> domainRecordingIds, bool scheduledSaved);

        List<HouseholdRecording> ListHouseholdRecordingsByRecordingKey(int partnerId, string recordingKey,
            List<string> recordingStatuses);

        List<HouseholdRecording> GetTop2HouseholdRecordingsByKey(int partnerId, string key, string status = "");
        List<HouseholdRecording> GetHouseholdProtectedRecordings(int partnerId, long householdId, long time);

        bool DeleteHouseholdRecording(int partnerId, string recordingKey, long householdId);

        List<HouseholdRecording> UpdateHhRecordingsStatusByRecordingKey(int partnerId, string recordingKey,
            string recordingState);

        List<HouseholdRecording> GetHhRecordingsMinProtectedEpoch(int partnerId, string recordingKey,
            long utcNowEpoch);

        List<HouseholdRecording> UpdateHhRecordingsIdAndProtectDate(int partnerId, string recordingKey,
            string recordingState, long utcNowEpoch, int skip);

        bool DeleteHouseholdRecordingById(int partnerId, long domainRecordingId);

        List<HouseholdRecording> GetHhRecordingsByChannelID(int partnerId, long epgChannelId, string status,
            string type);

        List<HouseholdRecording> GetHhRecordingsByEpgId(int partnerId, long householdId, long epgId,
            List<string> recordingStatuses, string type);

        List<Program> GetAllRecordedPrograms(int partnerId, int limit, int skip);
        HouseholdRecording GetHouseholdRecording(int partnerId, long id, long householdId);

        List<long> GetAllPartnerIds();
    }

    public class RecordingsRepository : IRecordingsRepository
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly IMongoDbClientFactory _recordingsService;
        private readonly IMongoDbAdminClientFactory _adminService;

        private static readonly Lazy<IRecordingsRepository> LazyInstance = new Lazy<IRecordingsRepository>(
            () => new RecordingsRepository(
                ClientFactoryBuilder.Instance.GetClientFactory(
                    RecordingsDbProperties.RECORDINGS_DATABASE,
                    RecordingsDbProperties.CollectionProperties),
                ClientFactoryBuilder.Instance.GetAdminClientFactory(
                    RecordingsDbProperties.RECORDINGS_DATABASE,
                    RecordingsDbProperties.CollectionProperties)),
            LazyThreadSafetyMode.PublicationOnly);

        public static IRecordingsRepository Instance => LazyInstance.Value;

        public RecordingsRepository(IMongoDbClientFactory clientFactory, IMongoDbAdminClientFactory adminFactory)
        {
            _recordingsService = clientFactory;
            _adminService = adminFactory;
        }

        public List<long> GetAllPartnerIds()
        {
            var factory = _adminService.NewMongoDbAdminClient(log);
            return factory.GetPartnerDbClients().Select(x => x.PartnerId).ToList();
        }

        private long GetNextRecordingId(int partnerId)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.GetNextId(RecordingsDbProperties.RECORDINGS_COLLECTION);
        }

        private long GetNextHouseholdRecordingId(int partnerId)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.GetNextId(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION);
        }

        private long GetNextProgramId(int partnerId)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.GetNextId(RecordingsDbProperties.PROGRAMS_COLLECTION);
        }

        public long AddRecording(int partnerId, TimeBasedRecording recording)
        {
            recording.Id = GetNextRecordingId(partnerId);
            recording.CreateDate = DateTime.UtcNow;
            recording.UpdateDate = DateTime.UtcNow;

            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            factory.InsertOne(RecordingsDbProperties.RECORDINGS_COLLECTION, recording);

            return recording.Id;
        }

        public TimeBasedRecording GetRecordingByKey(int partnerId, String key)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory
                .Find<TimeBasedRecording>(RecordingsDbProperties.RECORDINGS_COLLECTION, f => f.Eq(r => r.Key, key))
                .SingleOrDefault();
        }

        public TimeBasedRecording GetRecordingById(int partnerId, long id)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory
                .Find<TimeBasedRecording>(RecordingsDbProperties.RECORDINGS_COLLECTION, f => f.Eq(r => r.Id, id))
                .SingleOrDefault();
        }

        public List<TimeBasedRecording> GetRecordingsByKeys(int partnerId, List<string> Keys)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<TimeBasedRecording>(RecordingsDbProperties.RECORDINGS_COLLECTION, f =>
                f.In(p => p.Key, Keys)).ToList();
        }

        public List<TimeBasedRecording> GetRecordingsByEpgIdAndStatus(int partnerId, long epgId, string status)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<TimeBasedRecording>(RecordingsDbProperties.RECORDINGS_COLLECTION, f =>
                f.And(Builders<TimeBasedRecording>.Filter.Eq(p => p.EpgId, epgId),
                    Builders<TimeBasedRecording>.Filter.Eq(p => p.Status, status))).ToList();
        }

        public List<TimeBasedRecording> GetAndUpdateExpiredRecordings(int partnerId, long expiredTimeWindow)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            var results = factory.Find<TimeBasedRecording>(RecordingsDbProperties.RECORDINGS_COLLECTION, f =>
                f.And(Builders<TimeBasedRecording>.Filter.Lte(p => p.ViewableUntilEpoch, expiredTimeWindow),
                    Builders<TimeBasedRecording>.Filter.Eq(p => p.LifeTimeExpiryHandled, false))).ToList();
            factory.UpdateMany<TimeBasedRecording>(
                RecordingsDbProperties.RECORDINGS_COLLECTION,
                f => f.And(Builders<TimeBasedRecording>.Filter.Lte(p => p.ViewableUntilEpoch, expiredTimeWindow),
                    Builders<TimeBasedRecording>.Filter.Eq(p => p.LifeTimeExpiryHandled, false)),
                u => u.Set(c => c.LifeTimeExpiryHandled, true).Set(c => c.UpdateDate, DateTime.UtcNow));
            return results;
        }

        public List<TimeBasedRecording> GetRecordingsByEpgId(int partnerId, long epgId)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<TimeBasedRecording>(RecordingsDbProperties.RECORDINGS_COLLECTION, f =>
                f.Eq(p => p.EpgId, epgId)).ToList();
        }

        public bool UpdateRecording(int partnerId, TimeBasedRecording timeBasedRecording)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            var updateResult = factory.UpdateOne<TimeBasedRecording>(
                RecordingsDbProperties.RECORDINGS_COLLECTION,
                f => f.Eq(o => o.Id, timeBasedRecording.Id),
                u => SetUpdateExpression(timeBasedRecording, u));
            return updateResult.ModifiedCount > 0;
        }

        public long AddHouseholdRecording(int partnerId, HouseholdRecording householdRecording)
        {
            householdRecording.Id = GetNextHouseholdRecordingId(partnerId);
            householdRecording.CreateDate = DateTime.UtcNow;
            householdRecording.UpdateDate = DateTime.UtcNow;

            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            factory.InsertOne(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, householdRecording);

            return householdRecording.Id;
        }

        public bool UpdateHouseholdRecording(int partnerId, HouseholdRecording householdRecording)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            var updateResult = factory.UpdateOne<HouseholdRecording>(
                RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION,
                f => f.Eq(o => o.Id, householdRecording.Id),
                u => SetUpdateExpression(householdRecording, u));
            return updateResult.ModifiedCount > 0;
        }

        public bool IsHouseholdRecordingExists(int partnerId, string recordingKey, long householdId)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);

            var count = factory.Count<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.RecordingKey, recordingKey),
                    Builders<HouseholdRecording>.Filter.Eq(o => o.HouseholdId, householdId)));

            return count > 0;
        }

        public HouseholdRecording GetHouseholdRecording(int partnerId, string recordingKey, long householdId)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.RecordingKey, recordingKey),
                    Builders<HouseholdRecording>.Filter.Eq(o => o.HouseholdId, householdId))).SingleOrDefault();
        }

        public HouseholdRecording GetHouseholdRecording(int partnerId, long id, long householdId)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.Id, id),
                    Builders<HouseholdRecording>.Filter.Eq(o => o.HouseholdId, householdId))).SingleOrDefault();
        }

        public bool DeleteHouseholdRecording(int partnerId, string recordingKey, long householdId)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            try
            {
                factory.DeleteOne<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                    f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.RecordingKey, recordingKey),
                        Builders<HouseholdRecording>.Filter.Eq(o => o.HouseholdId, householdId)));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool DeleteRecording(int partnerId, string recordingKey)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            try
            {
                factory.DeleteOne<TimeBasedRecording>(RecordingsDbProperties.RECORDINGS_COLLECTION, f =>
                    f.Eq(o => o.Key, recordingKey));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public HouseholdRecording GetHouseholdRecordingById(int partnerId, long householdRecordingId, long householdId,
            string status)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.Id, householdRecordingId),
                    Builders<HouseholdRecording>.Filter.Eq(o => o.HouseholdId, householdId),
                    Builders<HouseholdRecording>.Filter.Eq(o => o.Status, status))).SingleOrDefault();
        }

        public List<HouseholdRecording> GetTop2HouseholdRecordingsByKey(int partnerId, string key, string status = "")
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            var option = new MongoDbFindOptions<HouseholdRecording>()
            {
                Limit = 2,
                Sort = s => s.Ascending(v => v.Id)
            };
            if (string.IsNullOrEmpty(status))
            {
                return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                    f.Eq(r => r.RecordingKey, key), option).ToList();
            }

            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                        f.And(
                            Builders<HouseholdRecording>.Filter.Eq(p => p.RecordingKey, key),
                            Builders<HouseholdRecording>.Filter.Eq(o => o.Status, status))
                    , option)
                .ToList();
        }

        public List<HouseholdRecording> GetHhRecordingsByHhIdAndRecordingStatuses(int partnerId, long householdId,
            List<string> recordingStatuses)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.HouseholdId, householdId),
                    Builders<HouseholdRecording>.Filter.In(z => z.Status, recordingStatuses))).ToList();
        }

        public List<HouseholdRecording> GetHhRecordingsByKey(int partnerId, string key, string status)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.RecordingKey, key),
                    Builders<HouseholdRecording>.Filter.Eq(z => z.Status, status))).ToList();
        }

        public List<HouseholdRecording> GetHhRecordingsByChannelID(int partnerId, long epgChannelId, string status,
            string type)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.EpgChannelId, epgChannelId),
                    Builders<HouseholdRecording>.Filter.Eq(o => o.RecordingType, type),
                    Builders<HouseholdRecording>.Filter.Eq(z => z.Status, status))).ToList();
        }

        public List<HouseholdRecording> GetHhRecordingsByEpgId(int partnerId, long householdId, long epgId,
            List<string> recordingStatuses, string type)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(
                    Builders<HouseholdRecording>.Filter.Eq(o => o.EpgId, epgId),
                    Builders<HouseholdRecording>.Filter.Eq(o => o.HouseholdId, householdId),
                    Builders<HouseholdRecording>.Filter.Eq(o => o.RecordingType, type),
                    //Builders<HouseholdRecording>.Filter.Eq(z => z.Status, status)
                    Builders<HouseholdRecording>.Filter.In(z => z.Status, recordingStatuses)
                )).ToList();
        }

        public List<long> GetHhRecordingsFailuresByKey(int partnerId, string key)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.RecordingKey, key),
                    Builders<HouseholdRecording>.Filter.Eq(o => o.ScheduledSaved, false),
                    Builders<HouseholdRecording>.Filter.Eq(z => z.Status, "OK"))).Select(x => x.Id).ToList();
        }

        public List<HouseholdRecording> GetHouseholdProtectedRecordings(int partnerId, long householdId, long time)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.HouseholdId, householdId),
                    Builders<HouseholdRecording>.Filter.Eq(z => z.Status, "OK"),
                    Builders<HouseholdRecording>.Filter.Gt(z => z.ProtectedUntilEpoch, time))).ToList();
        }

        public List<HouseholdRecording> UpdateHouseholdRecordingsStatus(int partnerId, List<long> domainRecordingIds,
            string recordingState)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            var updateResult = factory.UpdateMany<HouseholdRecording>(
                RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION,
                f => f.In(x => x.Id, domainRecordingIds),
                u => u.Set(c => c.Status, recordingState).Set(c => c.UpdateDate, DateTime.UtcNow));
            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.In(x => x.Id, domainRecordingIds)).ToList();
        }

        public bool DeleteHouseholdRecordingById(int partnerId, long domainRecordingId)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            try
            {
                factory.DeleteOne<HouseholdRecording>(
                    RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION,
                    f => f.Eq(x => x.Id, domainRecordingId));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<HouseholdRecording> UpdateHhRecordingsStatusByRecordingKey(int partnerId, string recordingKey,
            string recordingState)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            var updateResult = factory.UpdateMany<HouseholdRecording>(
                RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION,
                f => f.Eq(x => x.RecordingKey, recordingKey),
                u => u.Set(c => c.Status, recordingState).Set(c => c.UpdateDate, DateTime.UtcNow));

            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.Eq(o => o.RecordingKey, recordingKey)).ToList();
        }

        public List<HouseholdRecording> GetHhRecordingsMinProtectedEpoch(int partnerId, string recordingKey,
            long utcNowEpoch)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.RecordingKey, recordingKey),
                    Builders<HouseholdRecording>.Filter.Gt(o => o.ProtectedUntilEpoch, utcNowEpoch),
                    Builders<HouseholdRecording>.Filter.Eq(z => z.Status, "OK"))).ToList();
        }

        public List<HouseholdRecording> UpdateHhRecordingsIdAndProtectDate(int partnerId, string recordingKey,
            string recordingState, long utcNowEpoch, int skip)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            var option = new MongoDbFindOptions<HouseholdRecording>()
            {
                Limit = 500,
                Skip = skip,
                Sort = s => s.Ascending(v => v.Id)
            };

            var hhRecordings = factory.Find(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.RecordingKey, recordingKey),
                    Builders<HouseholdRecording>.Filter.Lt(o => o.ProtectedUntilEpoch, utcNowEpoch),
                    Builders<HouseholdRecording>.Filter.Eq(z => z.Status, "OK")), option).ToList();
            var hhRecordingsIds = hhRecordings.Select(x => x.Id);
            var updateResult = factory.UpdateMany<HouseholdRecording>(
                RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION,
                f => f.In(x => x.Id, hhRecordingsIds),
                u => u.Set(c => c.Status, recordingState).Set(c => c.UpdateDate, DateTime.UtcNow));

            return hhRecordings;
        }

        public bool UpdateHouseholdRecordingsFailure(int partnerId, List<long> domainRecordingIds, bool scheduledSaved)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            var updateResult = factory.UpdateMany<HouseholdRecording>(
                RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION,
                f => f.In(x => x.Id, domainRecordingIds),
                u => u.Set(c => c.ScheduledSaved, scheduledSaved).Set(c => c.UpdateDate, DateTime.UtcNow));
            return updateResult.ModifiedCount > 0;
        }

        public List<HouseholdRecording> ListHouseholdRecordingsByRecordingKey(int partnerId, string recordingKey,
            List<string> recordingStatuses)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<HouseholdRecording>(RecordingsDbProperties.HOUSEHOLD_RECORDINGS_COLLECTION, f =>
                f.And(Builders<HouseholdRecording>.Filter.Eq(o => o.RecordingKey, recordingKey),
                    Builders<HouseholdRecording>.Filter.In(z => z.Status, recordingStatuses))).ToList();
        }

        public bool UpdateRecordingStatus(int partnerId, long id, string recordingState)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            var updateResult = factory.UpdateMany<TimeBasedRecording>(
                RecordingsDbProperties.RECORDINGS_COLLECTION,
                f => f.Eq(x => x.Id, id),
                u => u.Set(c => c.Status, recordingState).Set(c => c.UpdateDate, DateTime.UtcNow));
            return updateResult.ModifiedCount > 0;
        }

        public long AddProgram(int partnerId, Program program)
        {
            program.Id = GetNextProgramId(partnerId);

            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            factory.InsertOne(RecordingsDbProperties.PROGRAMS_COLLECTION, program);

            return program.Id;
        }

        public bool DeleteProgram(int partnerId, long id)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            try
            {
                factory.DeleteOne<TimeBasedRecording>(RecordingsDbProperties.PROGRAMS_COLLECTION, f =>
                    f.Eq(o => o.Id, id));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool UpdateProgram(int partnerId, Program program)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            var updateResult = factory.UpdateOne<Program>(
                RecordingsDbProperties.PROGRAMS_COLLECTION,
                f => f.Eq(o => o.Id, program.Id),
                u => SetUpdateExpression(program, u));
            return updateResult.ModifiedCount > 0;
        }

        public List<Program> GetProgramsByEpgIds(int partnerId, List<long> epgId)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<Program>(RecordingsDbProperties.PROGRAMS_COLLECTION, f =>
                f.In(p => p.EpgId, epgId)).ToList();
        }

        public List<Program> GetProgramsByEpgIdIdsAndStartDate(int partnerId, List<long> epgId, DateTime startDate)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<Program>(RecordingsDbProperties.PROGRAMS_COLLECTION, f =>
                f.And(Builders<Program>.Filter.In(p => p.EpgId, epgId),
                    Builders<Program>.Filter.Gte(p => p.StartDate, startDate))).ToList();
        }

        public Program GetProgramByEpg(int partnerId, long epgId)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<Program>(RecordingsDbProperties.PROGRAMS_COLLECTION, f => f.Eq(r => r.EpgId, epgId))
                .SingleOrDefault();
        }

        public List<Program> GetAllRecordedPrograms(int partnerId, int limit, int skip)
        {
            var option = new MongoDbFindOptions<Program>()
            {
                Limit = limit,
                Skip = skip,
                Sort = s => s.Ascending(v => v.Id)
            };

            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<Program>(RecordingsDbProperties.PROGRAMS_COLLECTION, f =>
                f.Gte(p => p.Id, 0), option).ToList();
        }

        public List<Program> GetProgramsByProgramIds(int partnerId, List<long> programId)
        {
            var factory = _recordingsService.NewMongoDbClient(partnerId, log);
            return factory.Find<Program>(RecordingsDbProperties.PROGRAMS_COLLECTION, f =>
                f.In(p => p.Id, programId)).ToList();
        }

        private UpdateDefinition<TimeBasedRecording> SetUpdateExpression(TimeBasedRecording timeBasedRecording,
            UpdateDefinitionBuilder<TimeBasedRecording> updateBuilder)
        {
            updateBuilder = new UpdateDefinitionBuilder<TimeBasedRecording>();
            var updates = new List<UpdateDefinition<TimeBasedRecording>>();

            UpdateIfNotNull(updateBuilder, updates, x => x.Status, timeBasedRecording.Status);
            UpdateIfNotNull(updateBuilder, updates, x => x.Key, timeBasedRecording.Key);
            UpdateIfNotNull(updateBuilder, updates, x => x.PaddingAfterMins, timeBasedRecording.PaddingAfterMins);
            UpdateIfNotNull(updateBuilder, updates, x => x.PaddingBeforeMins, timeBasedRecording.PaddingBeforeMins);
            UpdateIfNotNull(updateBuilder, updates, x => x.AbsoluteStartTime, timeBasedRecording.AbsoluteStartTime);
            UpdateIfNotNull(updateBuilder, updates, x => x.AbsoluteEndTime, timeBasedRecording.AbsoluteEndTime);
            UpdateIfNotNull(updateBuilder, updates, x => x.ExternalId, timeBasedRecording.ExternalId);
            UpdateIfNotNull(updateBuilder, updates, x => x.RetriesStatus, timeBasedRecording.RetriesStatus);
            UpdateIfNotNull(updateBuilder, updates, x => x.ViewableUntilEpoch, timeBasedRecording.ViewableUntilEpoch);
            UpdateIfNotNull(updateBuilder, updates, x => x.LifeTimeExpiryHandled,
                timeBasedRecording.LifeTimeExpiryHandled);
            UpdateIfNotNull(updateBuilder, updates, x => x.UpdateDate, DateTime.UtcNow);

            return updateBuilder.Combine(updates);
        }

        private UpdateDefinition<Program> SetUpdateExpression(Program program,
            UpdateDefinitionBuilder<Program> updateBuilder)
        {
            updateBuilder = new UpdateDefinitionBuilder<Program>();
            var updates = new List<UpdateDefinition<Program>>();

            UpdateIfNotNull(updateBuilder, updates, x => x.EpgId, program.EpgId);
            UpdateIfNotNull(updateBuilder, updates, x => x.StartDate, program.StartDate);
            UpdateIfNotNull(updateBuilder, updates, x => x.EndDate, program.EndDate);
            UpdateIfNotNull(updateBuilder, updates, x => x.__updated, DateTime.UtcNow);

            return updateBuilder.Combine(updates);
        }

        private UpdateDefinition<HouseholdRecording> SetUpdateExpression(HouseholdRecording householdRecording,
            UpdateDefinitionBuilder<HouseholdRecording> updateBuilder)
        {
            updateBuilder = new UpdateDefinitionBuilder<HouseholdRecording>();
            var updates = new List<UpdateDefinition<HouseholdRecording>>();

            UpdateIfNotNull(updateBuilder, updates, x => x.Status, householdRecording.Status);
            UpdateIfNotNull(updateBuilder, updates, x => x.RecordingKey, householdRecording.RecordingKey);
            UpdateIfNotNull(updateBuilder, updates, x => x.ProtectedUntilEpoch, householdRecording.ProtectedUntilEpoch);
            UpdateIfNotNull(updateBuilder, updates, x => x.ScheduledSaved, householdRecording.ScheduledSaved);
            UpdateIfNotNull(updateBuilder, updates, x => x.UpdateDate, DateTime.UtcNow);

            return updateBuilder.Combine(updates);
        }

        private static void UpdateIfNotNull<TDocument, TField>(
            UpdateDefinitionBuilder<TDocument> updateBuilder,
            List<UpdateDefinition<TDocument>> updates,
            Expression<Func<TDocument, TField>> field,
            TField value)
        {
            if (value != null)
            {
                var update = updateBuilder.Set(field, value);
                updates.Add(update);
            }
        }
    }
}