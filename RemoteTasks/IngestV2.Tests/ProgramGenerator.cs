using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.BulkUpload;

namespace IngestV2.Tests
{
    public class ProgramGenerator
    {
        private int _groupId;
        private DateTime _startDate;
        private int _count;
        private TimeSpan _duration;
        private int _channel;
        private int _startingId;
        private bool _generateEpgIds;
        
        private ProgramGenerator()
        {
          _groupId = 0;
          _startDate = DateTime.Now;
          _count = 1;
          _duration = TimeSpan.FromHours(1);
          _channel = 1;
          _startingId = 1;
          _generateEpgIds = false;
        }

        public static ProgramGenerator Generate(int count)
        {
            var gen = new ProgramGenerator();
            gen._count = count;
            return gen;
        }

        public ProgramGenerator WithGroupId(int groupId)
        {
            _groupId = groupId;
            return this;
        }

        public ProgramGenerator FromDate(DateTime fromDate)
        {
            _startDate = fromDate;
            return this;
        }

        public ProgramGenerator WithDuration(TimeSpan duration)
        {
            _duration = duration;
            return this;
        }

        public ProgramGenerator WithEpgIds(bool withIds = true)
        {
            _generateEpgIds = withIds;
            return this;
        }

        public ProgramGenerator ForChannelId(int channelId)
        {
            _channel = channelId;
            return this;
        }

        public ProgramGenerator StartFromId(int id)
        {
            _startingId = id;
            return this;
        }
        
        
        public List<EpgProgramBulkUploadObject> BuildExistingPrograms()
        {
            var results = BuildProgramsToIngest();
            var programs = results.Select(r => r.Object as EpgProgramBulkUploadObject).ToList();
            return programs;
        }

        public BulkUpload BuildBulkUploadObj()
        {
            var bulkUploadObject = new BulkUpload();
            bulkUploadObject.GroupId = _groupId;
            bulkUploadObject.NumOfObjects = _count;
            bulkUploadObject.CreateDate = DateTime.Now;
            bulkUploadObject.Id = 0;
            bulkUploadObject.Results = BuildProgramsToIngest().Cast<BulkUploadResult>().ToList();
            return bulkUploadObject;
        }
        
        public List<BulkUploadProgramAssetResult> BuildProgramsToIngest()
        {
            var results = new List<BulkUploadProgramAssetResult>();
            for (var i = 0; i < _count; i++)
            {
                var currentId = _startingId + i;
                var result = new BulkUploadProgramAssetResult();
                result.ChannelId = _channel;
                result.ProgramExternalId = $"prog_{currentId}";
                result.LiveAssetId = _channel + 1000000;
                result.StartDate = _startDate.Add(_duration * i);
                result.EndDate = result.StartDate.Add(_duration);
                result.Index = i;
                result.Status = BulkUploadResultStatus.InProgress;

                var epgObject = new EpgProgramBulkUploadObject();
                epgObject.ChannelId = result.ChannelId;
                epgObject.ChannelExternalId = $"chan_{_channel}";
                epgObject.StartDate = result.StartDate;
                epgObject.EndDate = result.EndDate;
                epgObject.EpgExternalId = result.ProgramExternalId;
                epgObject.LinearMediaId = result.LiveAssetId;
                epgObject.GroupId = _groupId;
                epgObject.ParentGroupId = _groupId;
                if (_generateEpgIds)
                {
                    epgObject.EpgId = (ulong)currentId;
                }

                result.Object = epgObject;
                results.Add(result);
            }

            return results;
        }
    }
}