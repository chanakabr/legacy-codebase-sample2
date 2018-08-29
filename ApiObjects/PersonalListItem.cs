using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{    
    [Serializable]
    public class PersonalListItem
    {
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("Name")]
        public int Name;

        [JsonProperty("Timestamp")]
        public long Timestamp;
        
        [JsonProperty("Ksql")]
        public string Ksql;

        [JsonProperty("PartnerListType")]
        public int PartnerListType;

        public PersonalListItem()
        {
        }

        public PersonalListItem(string ksql)
        {
            this.Ksql = ksql;
        }
    }

    public class UserPersonalList
    {
        public long UserId { get; set; }

        public long CreateDateSec { get; set; }

        public List<PersonalListItem> Items { get; set; }

        public UserPersonalList(long userId)
        {
            this.UserId = userId;
            this.Items = new List<PersonalListItem>();
        }
    }
}
