using System.Data;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using TVinciShared;
using DAL;
using Tvinci.Core.DAL;
using System;
using System.Text;


namespace Catalog.Request
{

    public class MediaBookmarkData
    {
        [DataMember]
        public string action;
        [DataMember]
        public int location;

        public MediaBookmarkData()
        {

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("MediaPlayRequestData obj: ");
            sb.Append(String.Concat(" Action: ", action ?? "null"));
            sb.Append(String.Concat(" Loc: ", location));

            return sb.ToString();
        }

    }
}
