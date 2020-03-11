using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class DomainInfo
    {
        public eAddToDomainType AddDomainType { get; set; }
        public int GroupId { get; set; }
        public int DomainId { get; set; }
        public string DomainCoGuid { get; set; }
        public int DomainMasterId { get; set; }

        public DomainInfo(int groupId)
        {
            this.GroupId = groupId;
        }

        public enum eAddToDomainType
        {
            DontAddDomain = 0,
            CreateNewDomain = 1,
            AddToExistingDomain = 2
        }
    }
}
