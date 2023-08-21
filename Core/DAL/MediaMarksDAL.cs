using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiObjects;
using ApiObjects.MediaMarks;
using CouchbaseManager;
using Phx.Lib.Log;

namespace DAL
{
    public interface IMediaMarksDAL
    {
        UserMediaMarks GetUserMediaMarks(long userId);
        Task<bool> CleanUserMediaMarks(long userId);
        bool CleanUserMediaMarks(long userId, List<KeyValuePair<int, eAssetTypes>> assets);
        Task<bool> CleanMediaMarkLogsAsync(long userId, IEnumerable<KeyValuePair<int, eAssetTypes>> assets);
        bool CleanMediaMarkLogs(long userId, IEnumerable<KeyValuePair<int, eAssetTypes>> assets);
    }

    public class MediaMarksDAL : IMediaMarksDAL
    {
        private static readonly IKLogger Logger = new KLogger(nameof(MediaMarksDAL));

        private static readonly Lazy<IMediaMarksDAL> LazyInstance = new Lazy<IMediaMarksDAL>(
            () => new MediaMarksDAL(),
            LazyThreadSafetyMode.PublicationOnly);

        public static IMediaMarksDAL Instance => LazyInstance.Value;

        public UserMediaMarks GetUserMediaMarks(long userId)
        {
            var key = UtilsDal.GetUserAllAssetMarksDocKey(userId);

            return UtilsDal.GetObjectFromCB<UserMediaMarks>(eCouchbaseBucket.MEDIAMARK, key);
        }

        public Task<bool> CleanUserMediaMarks(long userId)
        {
            var key = UtilsDal.GetUserAllAssetMarksDocKey(userId);

            return UtilsDal.DeleteObjectFromCBAsync(eCouchbaseBucket.MEDIAMARK, key);
        }

        public static IDictionary<string, MediaMarkLog> GetMediaMarkLogs(
            long userId, IEnumerable<AssetAndLocation> assetsAndLocations)
        {
            var mediaMarkKeys = assetsAndLocations
                .Select(x => ConvertToMediaMarkKey(userId, x.AssetType, x.AssetId))
                .ToList();
            var mediaMarksManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);

            return mediaMarksManager.GetValues<MediaMarkLog>(mediaMarkKeys, true, true);
        }

        public bool CleanUserMediaMarks(long userId, List<KeyValuePair<int, eAssetTypes>> assets)
        {
            var userMarks = GetUserMediaMarks(userId);
            if (userMarks.mediaMarks?.Count > 0)
            {
                var mediaMarkManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
                var documentKey = UtilsDal.GetUserAllAssetMarksDocKey(userId);
                userMarks.mediaMarks.RemoveAll(m => assets.Exists(MatchAssetIdAndType(m)));

                return mediaMarkManager.Set(documentKey, userMarks, UtilsDal.UserMediaMarksTtl);
            }

            return true;
        }

        public bool CleanMediaMarkLogs(long userId, IEnumerable<KeyValuePair<int, eAssetTypes>> assets)
        {
            try
            {
                var mediaMarkManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);

                Random r = new Random();

                foreach (string documentKey in assets.Select(x => ConvertToMediaMarkKey(userId, x.Value, x.Key)))
                {
                    bool markResult = mediaMarkManager.Remove(documentKey);
                    Thread.Sleep(r.Next(50));

                    if (!markResult)
                    {
                        Logger.ErrorFormat("Failed to remove asset history key = {0}, markResult = {1}", documentKey, markResult);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to remove asset history, user: {userId}, assetTypes: {string.Join(",", assets.Select(x => (x.Key, x.Value)))}", ex);
                return false;
            }
        }

        public Task<bool> CleanMediaMarkLogsAsync(long userId, IEnumerable<KeyValuePair<int, eAssetTypes>> assets)
        {
            var keys = assets.Select(x => ConvertToMediaMarkKey(userId, x.Value, x.Key));

            return UtilsDal.BulkDeleteObjectFromCBAsync(eCouchbaseBucket.MEDIAMARK, keys);
        }

        private static Predicate<KeyValuePair<int, eAssetTypes>> MatchAssetIdAndType(AssetAndLocation m)
        {
            return kv => (kv.Value == eAssetTypes.NPVR
                    && !string.IsNullOrEmpty(m.NpvrId)
                    && kv.Key.ToString() == m.NpvrId)
                || (kv.Value == m.AssetType && kv.Key == m.AssetId);
        }

        private static string ConvertToMediaMarkKey(long userId, eAssetTypes assetType, long assetId)
        {
            switch (assetType)
            {
                case eAssetTypes.EPG:
                    return UtilsDal.GetUserEpgMarkDocKey(userId, assetId);
                case eAssetTypes.NPVR:
                    return UtilsDal.GetUserNpvrMarkDocKey(userId, assetId.ToString());
                default:
                    return UtilsDal.GetUserMediaMarkDocKey(userId.ToString(), (int)assetId);
            }
        }
    }
}