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
    public class SendToFriendLoader : TVMAdapter<SendToFriend>
    {

        #region Members
        private string _myEmail = string.Empty;
        private string _note = string.Empty;
        private string _friendEmail = string.Empty;
        private string _myName = string.Empty;


        #endregion

        #region Properties
        protected string TvmPass { get; set; }
        protected string TvmUser { get; set; }

        public string MediaID
        {
            get
            {

                return Parameters.GetParameter<string>(eParameterType.Retrieve, "MediaID", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "MediaID", value);
            }
        }

        public string EmailFrom
        {
            get
            {

                return Parameters.GetParameter<string>(eParameterType.Retrieve, "EmailFrom", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "EmailFrom", value);
            }
        }

        public string AddedMessage
        {
            get
            {

                return Parameters.GetParameter<string>(eParameterType.Retrieve, "AddedMessage", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "AddedMessage", value);
            }
        }

        public string FriendEmail
        {
            get
            {

                return Parameters.GetParameter<string>(eParameterType.Retrieve, "FriendsEmail", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "FriendsEmail", value);
            }


        }

        public string SenderName
        {
            get
            {

                return Parameters.GetParameter<string>(eParameterType.Retrieve, "SenderName", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "SenderName", value);
            }


        }

        #endregion

        #region C'tors
        public SendToFriendLoader(string mediaID)
            : this(string.Empty, string.Empty, mediaID)
        {
        }

        public SendToFriendLoader(string tvmUn, string tvmPass, string mediaID)
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

            return result;

        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{3B5420B3-16A4-43e2-A873-56282B63CE82}"); }
        }

        protected override SendToFriend PreCacheHandling(object retrievedData)
        {
            SendToFriend data = retrievedData as SendToFriend;
            if (data == null)
                return new SendToFriend();

            return data;
        }
    }
}
