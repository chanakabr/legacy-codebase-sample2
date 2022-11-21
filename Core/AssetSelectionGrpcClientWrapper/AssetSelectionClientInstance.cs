using System;
using System.Threading;
using ApiObjects.AssetSelection;
using GrpcClientCommon;
using OTT.Service.AssetSelection;
using Phx.Lib.Appconfig;

namespace AssetSelectionGrpcClientWrapper
{
    public class AssetSelectionClientInstance
    {
        public static IAssetSelectionClient Get() => LazyInstance.Value;
        
        private static readonly Lazy<IAssetSelectionClient> LazyInstance = new Lazy<IAssetSelectionClient>(Create, LazyThreadSafetyMode.PublicationOnly);
        private static IAssetSelectionClient Create() =>
            new AssetSelectionClient(
                new AssetSelection.AssetSelectionClient(GrpcCommon.CreateChannel(ApplicationConfiguration.Current
                    .MicroservicesClientConfiguration.AssetSelection)));
    }
}