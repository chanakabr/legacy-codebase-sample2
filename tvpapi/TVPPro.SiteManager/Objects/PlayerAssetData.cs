using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.Objects
{
    public class PlayerAssetData
    {
        public string action;
        public int location;
        public int averageBitRate;
        public int totalBitRate;
        public int currentBitRate;

        public PlayerAssetData()
        {

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("PlayerAssetData obj: ");
            sb.Append(String.Concat(" Action: ", action ?? "null"));
            sb.Append(String.Concat(" Location: ", location));
            sb.Append(String.Concat(" AverageBitRate: ", averageBitRate));
            sb.Append(String.Concat(" TotalBitRate: ", totalBitRate));
            sb.Append(String.Concat(" CurrentBitRate: ", currentBitRate));

            return sb.ToString();
        }

    }
}
