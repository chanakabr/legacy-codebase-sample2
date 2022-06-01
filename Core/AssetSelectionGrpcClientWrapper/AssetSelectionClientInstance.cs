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
        private static IAssetSelectionClient Create()
        {
            var grpcServiceConfig = ApplicationConfiguration.Current.MicroservicesClientConfiguration.AssetSelection;
            var address = grpcServiceConfig.Address.Value;
            var certFilePath = grpcServiceConfig.CertFilePath.Value;
            var retryCount = grpcServiceConfig.RetryCount.Value;

            return new AssetSelectionClient(
                new AssetSelection.AssetSelectionClient(GrpcCommon.CreateChannel(address, certFilePath, retryCount)));
        }
    }
}