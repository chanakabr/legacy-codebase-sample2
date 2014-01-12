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
        public string Author { get; set; }
        public string Header { get; set; }
        public string AddedDate { get; set; }
        public string Content { get; set; }

        public Comment(string author, string header, string addedDate, string content)
        {
            Author = author;
            Header = header;
            AddedDate = addedDate;
            Content = content;
        }
    }
}
