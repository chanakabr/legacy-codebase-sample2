using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationObj
{
    public class FollowDataTvSeries : FollowDataBase
    {
        public int AssetId;

        public override string FollowPhrase
        {
            get
            {
                if (string.IsNullOrEmpty(_followPhrase))
                    _followPhrase = GetFollowPhrase();
                return _followPhrase;
            }
            set { _followPhrase = value; }
        }        

        private string GetFollowPhrase()
        {
            return string.Format("series name='{0}'", Title);
        }
    }
}
