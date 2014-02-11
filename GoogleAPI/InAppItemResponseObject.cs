using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Tvinic.GoogleAPI
{
   
    [DataContract()]
    public class InAppItemResponseObject
    {
        /// <summary>
        /// A unique identifier for this transaction (this is set by Google on postback)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue=true, IsRequired=true)]
        public string orderId = null;
        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public string statusCode = null;

        public InAppItemResponseObject()
        {}

        [OnDeserialized()]
        public void validateDeserializedClaim(StreamingContext context)
        {
            validateClaim();
        }

        [OnSerialized()]
        public void validateSerializedClaim(StreamingContext context)
        {
            validateClaim();
        }

        private void validateClaim()
        {
            if (string.IsNullOrEmpty(this.orderId))
            {
                throw new NullReferenceException("InAppItemResponseObject orderId is required. InAppItemResponseObject is only instantiated on Google postback and it must contain an Google orderID value.");
            }
            
        }

    
    }
}
