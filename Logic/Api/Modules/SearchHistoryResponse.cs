using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Api.Modules
{
    public class SearchHistoryResponse
    {
        public Status Status;

        public List<SearchHistory> Searches
        {
            get;
            set;
        }

        public SearchHistoryResponse()            
        {
            this.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.Searches = new List<SearchHistory>();
        }
    }
}
