using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Comment
/// </summary>
/// 
namespace TVPApi
{
    public class Comment
    {
        public string author { get; set; }

        public string header { get; set; }

        public string added_date { get; set; }

        public string content { get; set; }

        public Comment(string author, string header, string addedDate, string content)
        {
            this.author = author;
            this.header = header;
            this.added_date = addedDate;
            this.content = content;
        }
    }
}
