using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class RecommendationEngineBase
    {
        public int ID
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }

        public RecommendationEngineBase()
        {
        }

        public RecommendationEngineBase(RecommendationEngineBase recommendationEngineBase)
        {
            this.ID = recommendationEngineBase.ID;
            this.Name = recommendationEngineBase.Name;
        }

        public RecommendationEngineBase(int id, string name, bool isDefault)
        {
            this.ID = id;
            this.Name = name;
        }
    }
}
