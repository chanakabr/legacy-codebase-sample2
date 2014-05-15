using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using ApiObjects.Statistics;

namespace Catalog
{
    [DataContract]
    public class AssetStatsResponse : BaseResponse 
    {

        [DataMember]
        public List<AssetStatsResult> m_lAssetStat;

        public AssetStatsResponse()
            : base()
        {
            m_lAssetStat = new List<AssetStatsResult>();
        }



    }

    [DataContract]
    public class AssetStatsResult
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
        public BuzzWeightedAverScore m_buzzAverScore;

        public AssetStatsResult()
            : base()
        {
            m_nAssetID = 0;
            m_nLikes = 0;
            m_nVotes = 0;
            m_dRate = 0;
            m_nViews = 0;
           m_buzzAverScore = new BuzzWeightedAverScore();
        }

        public AssetStatsResult(int nAssetID, int nLikes, int nVotes, double dRate, int nViews, BuzzWeightedAverScore oBuzzAverScore)
        {
            m_nAssetID = nAssetID;
            m_nLikes = nLikes;
            m_nVotes = nVotes;
            m_dRate = dRate;
            m_nViews = nViews;
            m_buzzAverScore = oBuzzAverScore;
        }


    }
}
