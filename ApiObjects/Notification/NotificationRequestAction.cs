using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    /// <summary>
    /// Represent notifiaction action, 
    /// mapped to a record in notification_actions table at the db.
    /// </summary>
    [Serializable]
    public class NotificationRequestAction
    {
        #region public Properties
        public long ID { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
        #endregion

        #region Constructor
        public NotificationRequestAction(long id, string text, string link)
        {
            this.ID = id;
            this.Text = text;
            this.Link = link;
        }
        #endregion
    }
}
