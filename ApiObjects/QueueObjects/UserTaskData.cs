using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.QueueObjects
{
    public class UserTaskData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_user_task";
        
        #endregion

        #region Data Members

        private int GroupID { get; set; }
        private UserTaskType UserTaskType { get; set; }
        private string UserId { get; set; }
        private int DomainId { get; set; }

        #endregion

        public UserTaskData(int groupId, UserTaskType userTaskType, string userId, int domainId)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.UserTaskType = userTaskType;
            this.UserId = userId;
            this.DomainId = domainId;
            this.args = new List<object>()
            {
                groupId,
                userTaskType,
                userId, 
                domainId
            };
        }
    }
}
