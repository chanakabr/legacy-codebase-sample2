using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class LanguageResponse
    {

        public Status Status;

        public List<LanguageObj> Languages { get; set; }

        public LanguageResponse()            
        {
            this.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.Languages = new List<LanguageObj>();
        }

    }
}
