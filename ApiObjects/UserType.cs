using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    /// <summary>
    /// Represent type of user, 
    /// ID property maps to User_Type column at users table , Description maps to  users_types table.
    /// </summary>
    [Serializable]
    public struct UserType
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
    }
}
