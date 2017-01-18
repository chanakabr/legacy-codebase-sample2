using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.MediaMarks
{
    public class UserMediaKey : IEquatable<UserMediaKey>
    {
        /*
         * DO NOT REMOVE THE READONLY FROM THE MEMBERS.
         * 
         */ 
        public readonly int mediaID;
        public readonly int userID;
        public readonly int hashCode;

        public UserMediaKey(int userID, int mediaID)
        {
            this.mediaID = mediaID;
            this.userID = userID;
            this.hashCode = String.Concat(userID, "_###_", mediaID).GetHashCode();
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public bool Equals(UserMediaKey other)
        {
            return hashCode.Equals(other.hashCode);
        }

        public override string ToString()
        {
            return String.Concat("u", userID, "_m", mediaID);
        }
    }
}
