using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;

namespace ApiObjects
{
    /// <summary>
    /// Represent type of user, 
    /// ID property maps to User_Type column at users table , Description maps to  users_types table.
    /// </summary>
    [Serializable]
    public struct UserType : IDeepCloneable<UserType>
    {
        public int? ID { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }

        public UserType(int? id, string desc, bool isDefault)
            : this()
        {
            ID = id;
            Description = desc;
            IsDefault = isDefault;
        }

        public UserType(UserType other)
        {
            ID = other.ID;
            Description = other.Description;
            IsDefault = other.IsDefault;
        }

        public UserType Clone()
        {
            return new UserType(this);
        }
    }
}