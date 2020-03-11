using System.Collections.Generic;
using System.Runtime.Serialization;
using Core.Catalog.Response;
namespace TVPApiModule.Objects.CRM
{

    [DataContract]
    public class AssetStatsResultDTO
    {
        [DataMember]
        public int m_nAssetID;

        [DataMember]
        public int m_nLikes;

        [DataMember]
        public int m_nVotes;

        [DataMember]
        public double m_dRate;

        [DataMember]
        public int m_nViews;

        [DataMember]
        public BuzzWeightedAverScoreDTO m_buzzAverScore;


        public static AssetStatsResultDTO ConvertToDTO(AssetStatsResult statResult)
        {
            if (statResult == null)
            {
                return null;
            }
            AssetStatsResultDTO res = new AssetStatsResultDTO()
            {
                m_buzzAverScore = BuzzWeightedAverScoreDTO.ConvertToDTO(statResult.m_buzzAverScore),
                m_dRate = statResult.m_dRate,
                m_nAssetID = statResult.m_nAssetID,
                m_nLikes = statResult.m_nLikes,
                m_nViews = statResult.m_nViews,
                m_nVotes = statResult.m_nVotes
            };
            return res;
        }

        
    }
}
