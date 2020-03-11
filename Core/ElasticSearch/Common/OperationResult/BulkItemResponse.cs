using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Common.OperationResult
{
    public class BulkItemResponse
    {
        public eOperation Operation { get; set; }
        public string Index { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
        public OperateResult OperationResult { get; set; }



        public static List<BulkItemResponse> GetBulkResponse(string json)
        {
            List<BulkItemResponse> response = new List<BulkItemResponse>();

            if (!string.IsNullOrEmpty(json))
            {

            }


            return response;
        }
    }
}
