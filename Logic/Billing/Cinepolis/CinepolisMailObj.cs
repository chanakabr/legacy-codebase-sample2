using ApiObjects.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Billing
{
    public class CinepolisMailObj
    {
        public string sUsername = string.Empty;
        public string sItemName = string.Empty;
        public string sPurchaseDate = string.Empty;
        public string sPrice = string.Empty;
        public long lGroupID = 0;
        public string sSiteGuid = string.Empty;
        public CinepolisMailType eCMT = CinepolisMailType.Purchase;

        public CinepolisMailObj(string username, string itemName, string purchaseDate, string price, long groupID, string siteGuid, CinepolisMailType cmt)
        {
            sUsername = username;
            sItemName = itemName;
            sPurchaseDate = purchaseDate;
            sPrice = price;
            lGroupID = groupID;
            sSiteGuid = siteGuid;
            eCMT = cmt;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Concat("CinepolisMailObj: "));
            sb.Append(String.Concat("Username: ", sUsername));
            sb.Append(String.Concat(" Item Name: ", sItemName));
            sb.Append(String.Concat(" Purchase Date: ", sPurchaseDate));
            sb.Append(String.Concat(" Price: ", sPrice));
            sb.Append(String.Concat(" Group ID: ", lGroupID));
            sb.Append(String.Concat(" Site Guid: ", sSiteGuid));

            return sb.ToString();
        }
    }
}
