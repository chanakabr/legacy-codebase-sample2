using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class UserEngagement
    {
        public string DocType { get; set; }
        public int UserId { get; set; }
        public int PartnerId { get; set; }

        public int EngagementId { get; set; }
        public int Bulk { get; set; }
        public bool IsChurnSent { get; set; }

        public UserEngagement(int partnerId, int userId)
        {
            this.PartnerId = partnerId;
            this.UserId = userId;
            this.DocType = "UserEngagement";
        }
    }
}
