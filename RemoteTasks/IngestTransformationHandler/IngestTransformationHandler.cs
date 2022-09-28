using System;
using System.Threading.Tasks;
using ApiObjects;
using ApiObjects.EventBus;
using Core.Api;
using Core.GroupManagers;
using EventBus.Abstraction;

namespace IngestTransformationHandler
{
    public class IngestTransformationHandler : IServiceEventHandler<BulkUploadTransformationEvent>
    {
        private readonly IGroupSettingsManager _groupSettingsManager;
        private readonly IngestV2TransformationHandler _transformationHandlerV2;
        private readonly IngestV3TransformationHandler _transformationHandlerV3;

        public IngestTransformationHandler(
            IGroupSettingsManager groupSettingsManager,
            IngestV2TransformationHandler transformationHandlerV2,
            IngestV3TransformationHandler transformationHandlerV3)
        {
            _groupSettingsManager = groupSettingsManager;
            _transformationHandlerV2 = transformationHandlerV2;
            _transformationHandlerV3 = transformationHandlerV3;
        }

        public Task Handle(BulkUploadTransformationEvent serviceEvent)
        {
            var epgFeatureVersion = _groupSettingsManager.GetEpgFeatureVersion(serviceEvent.GroupId);
            switch (epgFeatureVersion)
            {
                case EpgFeatureVersion.V1:
                    throw new NotImplementedException("Transformation handler does not support epg v1");
                case EpgFeatureVersion.V2:
                    return _transformationHandlerV2.Handle(serviceEvent);
                case EpgFeatureVersion.V3:
                    return _transformationHandlerV3.Handle(serviceEvent);
                default:
                    throw new Exception($"Unknown epg feature version {epgFeatureVersion}");
            }
        }
    }
}