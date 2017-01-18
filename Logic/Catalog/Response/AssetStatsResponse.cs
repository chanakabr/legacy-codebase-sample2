using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using ApiObjects.Statistics;
using System.Xml.Serialization;

namespace Core.Catalog.Response
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

        internal class IndexedAssetStatsResult : IComparable<IndexedAssetStatsResult>
        {
            private AssetStatsResult assetStatsResult;
            private int index;

            public AssetStatsResult Result
            {
                get
                {
                    return assetStatsResult;
                }
                private set
                {
                    assetStatsResult = value;
                }
            }

            public int Index
            {
                get
                {
                    return index;
                }
                private set
                {
                    index = value;
                }
            }

            public IndexedAssetStatsResult(int index, AssetStatsResult result)
            {
                Index = index;
                Result = result;
            }

            public int CompareTo(IndexedAssetStatsResult other)
            {
                return Index.CompareTo(other.Index);
            }
        }

        internal class SocialPartialAssetStatsResult
        {
            public int assetId;
            public int likesCounter;
            public double rate;
            public int votes;

            public SocialPartialAssetStatsResult()
            {
                this.assetId = 0;
                this.likesCounter = 0;
                this.rate = 0d;
                this.votes = 0;
            }

            public SocialPartialAssetStatsResult(int assetId, int likesCounter, double rate, int votes)
            {
                this.assetId = assetId;
                this.likesCounter = likesCounter;
                this.rate = rate;
                this.votes = votes;
            }
        }


    }
}
