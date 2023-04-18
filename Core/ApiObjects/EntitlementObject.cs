using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;

namespace ApiObjects
{
    [Serializable]
    public class EntitlementObject : IDeepCloneable<EntitlementObject>
    {
        public int ID;
        public string subscriptionCode;
        public string relPP;
        public int waiver;
        public string purchasedBySiteGuid;
        public int purchasedAsMediaFileID;
        public int ppvCode;
        public DateTime createDate;
        public DateTime? startDate;
        public DateTime? endDate;
        public int numOfUses;
        public bool isPending;

        public EntitlementObject(int p_ID, string p_subscriptionCode, string p_relPP, int p_waiver, string p_purchasedBySiteGuid, 
            int p_purchasedAsMediaFileID, int p_ppvCode, DateTime p_createDate, DateTime? p_startDate, DateTime? p_endDate, int uses)
        {
            ID = p_ID;
            subscriptionCode = p_subscriptionCode;
            relPP = p_relPP;
            waiver = p_waiver;
            purchasedBySiteGuid = p_purchasedBySiteGuid;
            purchasedAsMediaFileID = p_purchasedAsMediaFileID;
            ppvCode = p_ppvCode;
            createDate = p_createDate;
            startDate = p_startDate;
            endDate = p_endDate;
            numOfUses = uses;
        }

        public EntitlementObject() { }

        public EntitlementObject(EntitlementObject other)
        {
            ID = other.ID;
            subscriptionCode = other.subscriptionCode;
            relPP = other.relPP;
            waiver = other.waiver;
            purchasedBySiteGuid = other.purchasedBySiteGuid;
            purchasedAsMediaFileID = other.purchasedAsMediaFileID;
            ppvCode = other.ppvCode;
            createDate = other.createDate;
            startDate = other.startDate;
            endDate = other.endDate;
            numOfUses = other.numOfUses;
            isPending = other.isPending;
        }
        public EntitlementObject Clone()
        {
            return new EntitlementObject(this);
        }
    }
}