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

        public static List<AssetStatsResultDTO> ConvertToDTO(List<AssetStatsResult> result)
        {
            if(result == null)
            {
                return null;
            }
            List<AssetStatsResultDTO> res = result.ConvertAll(x => new AssetStatsResultDTO()
            {
                m_buzzAverScore = new BuzzWeightedAverScoreDTO()
                {
                    NormalizedWeightedAverageScore = x.m_buzzAverScore.NormalizedWeightedAverageScore,
                    UpdateDate = x.m_buzzAverScore.UpdateDate,
                    WeightedAverageScore = x.m_buzzAverScore.WeightedAverageScore
                },
                m_dRate = x.m_dRate,
                m_nAssetID = x.m_nAssetID,
                m_nLikes = x.m_nLikes,
                m_nViews = x.m_nViews,
                m_nVotes = x.m_nVotes
            });
            return res;
        }
    }
}
