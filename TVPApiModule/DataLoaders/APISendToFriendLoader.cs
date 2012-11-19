using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.SendToFriend;
using Tvinci.Data.DataLoader;


namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class APISendToFriendLoader : SendToFriendLoader
    {       
        #region C'tors
        public APISendToFriendLoader(string mediaID)
            : this(string.Empty, string.Empty, mediaID)
        {
        }

        public APISendToFriendLoader(string tvmUn, string tvmPass, string mediaID)
            : base(tvmUn, tvmPass, mediaID)
        {
            this.MediaID = mediaID;
            this.TvmPass = tvmPass;
            this.TvmUser = tvmUn;
        }
        #endregion



        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            SendToFriend result = new SendToFriend();
            result.root.request.mail.from = EmailFrom;
            result.root.request.mail.sender_name = SenderName;
            result.root.request.mail.to = FriendEmail;
            result.root.request.mail.Value = AddedMessage;
            result.root.request.media.id = this.MediaID;
            result.root.flashvars.player_un = TvmUser;
            result.root.flashvars.player_pass = TvmPass;

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{3B5420B3-16A4-43e2-A873-56282B63CE83}"); }
        }

        //protected override SendToFriend PreCacheHandling(object retrievedData)
        //{
        //    SendToFriend data = retrievedData as SendToFriend;
        //    if (data == null)
        //        return new SendToFriend();

        //    return data;
        //}
    }
}
