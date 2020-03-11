using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects
{
    public class UserInterests
    {
        public string DocType { get; set; }
        public int UserId { get; set; }
        public int PartnerId { get; set; }
        public List<UserInterest> UserInterestList { get; set; }

        public UserInterests()
        {
            this.DocType = "UserInterest";
            UserInterestList = new List<UserInterest>();
        }
    }

    public class UserInterest 
    {
        public string UserInterestId { get; set; }
        public UserInterestTopic Topic { get; set; }
        
        public override bool Equals(object userInterestToCompare)
        {
            var userInterest = userInterestToCompare as UserInterest;
            
            if(userInterest == null)
               return false;

            return JsonConvert.SerializeObject(this.Topic) == JsonConvert.SerializeObject(userInterest.Topic);            
        }

        public override int GetHashCode()
        {
            return this.Topic.GetHashCode();
        }
    }

    public class UserInterestTopic 
    {
        public string MetaId { get; set; }
        public string Value { get; set; }
        public UserInterestTopic ParentTopic { get; set; }
    }   
}
