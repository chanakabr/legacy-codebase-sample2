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
        public string Id { get; set; }
        public string MetaId { get; set; }
        public UserInterestTopic Topic { get; set; }

        public override bool Equals(object obj)
        {
            var userInterest = obj as UserInterest;

            if (userInterest == null)
            {
                return false;
            }

            return this.MetaId == userInterest.MetaId && JsonConvert.SerializeObject(this.Topic) == JsonConvert.SerializeObject(userInterest.Topic);
        }
    }

    public class UserInterestTopic 
    {
        public string Value { get; set; }
        public UserInterestTopic ParentTopic { get; set; }
    }

    //TODO: anat remove
    //class Test
    //{
    //    UserInterest a = new UserInterest() { MetaId = "203_Media_team", Topic = new UserInterestTopic() { Value = "Maccbi Haifa", ParentTopic = new UserInterestTopic() { Value = "HAAL" } } };
        
    //}
}
