using System;

namespace ApiObjects.User
{
    public class InsertUserOutputModel
    {
        public int UserId { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}