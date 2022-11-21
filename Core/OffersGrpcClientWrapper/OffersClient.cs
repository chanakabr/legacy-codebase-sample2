using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Google.Protobuf.Collections;
using GrpcClientCommon;
using OTT.Service.Offers;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using SlimAsset = OTT.Service.Offers.SlimAsset;

namespace OffersGrpcClientWrapper
{
    public class OffersClient
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly Offers.OffersClient _client;
        private static readonly Lazy<OffersClient> LazyInstance = new Lazy<OffersClient>(() => new OffersClient(), LazyThreadSafetyMode.PublicationOnly);
        public static OffersClient Instance => LazyInstance.Value;

        private OffersClient()
        {
            _client = new Offers.OffersClient(GrpcCommon.CreateChannel(ApplicationConfiguration.Current.MicroservicesClientConfiguration.Offers));
        }

        public List<(long AssetId, RepeatedField<ProductOffer> Products)> GetAssetProductOffers(int groupId, IEnumerable<SlimAsset> assetIds)
        {
            try
            {
                
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC))
                {
                    var grpcResponse = _client.GetAssetOffers(new AssetOffersRequest
                    {
                        PartnerId = groupId,
                        Filter = new AssetOffersFilter
                        {
                            AssetsIn = 
                            {
                                assetIds
                            }
                        }
                    });
                    return grpcResponse.Objects.Select(x => (x.AssetId, x.Products)).ToList();
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error while calling GetAssetProductOffersAsync Offer GRPC service", e);
                return null;
            }
        }
    }
}