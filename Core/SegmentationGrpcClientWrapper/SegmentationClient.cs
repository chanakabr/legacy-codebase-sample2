using GrpcClientCommon;
using OTT.Service.Segmentation;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using SegmentationType = ApiObjects.Segmentation.SegmentationType;

namespace SegmentationGrpcClientWrapper
{
    public class SegmentationClient
    {
        private readonly Segmentation.SegmentationClient _client;

        private static readonly Lazy<SegmentationClient> LazyInstance =
            new Lazy<SegmentationClient>(() => new SegmentationClient(), LazyThreadSafetyMode.PublicationOnly);

        public static readonly SegmentationClient Instance = LazyInstance.Value;

        private SegmentationClient()
        {
            var address = ApplicationConfiguration.Current.MicroservicesClientConfiguration.Segmentation.Address.Value;
            var certFilePath = ApplicationConfiguration.Current.MicroservicesClientConfiguration.Segmentation.CertFilePath.Value;
            var retryCount = ApplicationConfiguration.Current.MicroservicesClientConfiguration.Segmentation.RetryCount.Value;
            _client = new Segmentation.SegmentationClient(GrpcCommon.CreateChannel(address, certFilePath, retryCount));
        }

        public IEnumerable<long> ListHouseholdSegmentIds(ListHouseholdSegmentRequest request)
        {
            using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, request.PartnerId.ToString()))
            {
                var response = _client.ListHouseholdSegment(request);
                return response.Segments.SegmentIds;
            }
        }

        public IEnumerable<long> ListUserSegmentIds(ListUserSegmentRequest request)
        {
            using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, request.PartnerId.ToString()))
            {
                var response = _client.ListUserSegment(request);
                return response.Segments.SegmentIds;
            }
        }
        
        public List<SegmentationType> GetSegmentationTypesByValue(GetSegmentationTypesByValueRequest request)
        {
            using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, request.PartnerId.ToString()))
            {
                var grpcResponse = _client.GetSegmentationTypesByValue(request);
                return SegmentationMapper.MapToListResponse(grpcResponse);
            }
        }
    }
}