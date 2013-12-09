using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.SendToFriend;
using Tvinci.Data.DataLoader;
using TVPApi;


namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class APISendToFriendLoader : SendToFriendLoader
    {
        public int GroupID
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "GroupID", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "GroupID", value);
            }
        }

        public PlatformType Platform
        {
            get
            {
                return Parameters.GetParameter<PlatformType>(eParameterType.Retrieve, "Platform", PlatformType.Unknown);
            }
            set
            {
                Parameters.SetParameter<PlatformType>(eParameterType.Retrieve, "Platform", value);
            }
        }

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

        protected override void PreExecute()
        {
            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL))
                (base.GetProvider() as Tvinci.Data.TVMDataLoader.TVMProvider).TVMAltURL = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL;

            base.PreExecute();
        }


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
