using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.Users
{
    [DataContract(Name = "Users", Namespace = "")]
    [XmlRoot("Users")]
    public class UsersList : List<User>
    {

    }
}