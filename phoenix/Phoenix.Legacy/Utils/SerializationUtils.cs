using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;

namespace WebAPI.Utils
{
    public class SerializationUtils
    {
        public class ConvertCommaDelimitedList<T> : CollectionModelBinder<T>
        {
            public override bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                var _queryName = HttpUtility.ParseQueryString(actionContext.Request.RequestUri.Query)[bindingContext.ModelName];
                List<string> _model = new List<string>();
                if (!String.IsNullOrEmpty(_queryName))
                    _model = _queryName.Split(',').ToList();

                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                    bindingContext.Model = _model.ConvertAll(m => (T)converter.ConvertFromString(m));
                else
                    bindingContext.Model = _model;

                return true;
            }
        }

        //TODO: Change. but keep hardcoded, as TCM config may damage this.
        private const string passPhrase = "OdedIsOded";

        public static string MaskSensitiveObject(string originalVal)
        {
            return EncryptionUtils.Encrypt(originalVal, passPhrase);
        }

        public static string UnmaskSensitiveObject(string maskedVal)
        {
            return EncryptionUtils.Decrypt(maskedVal, passPhrase);
        }
    }
}