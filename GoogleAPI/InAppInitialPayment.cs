using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Tvinic.GoogleAPI
{
    [DataContract()]
    public class InAppInitialPayment
    {
        #region "Defaults"
        private string _price = "0.00";
        private string _currencyCode = "USD";
        private string _paymentType = "prorated";
        
        

        #endregion
        /// <summary>
        /// Required. The purchase price of the item, with up to 2 decimal places.
        /// Google will reject value exceeding 2 decimal places.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Required()]
        [DataMember(IsRequired = true, Order = 1)]
        public string price
        {
            get { return _price; }

            set
            {
                //NOTE: Pending official clarification.
                //   Currently Google sandbox allows 0 value for itemPrice.
                //   While it would be odd to allow 0 value payments, allow 0 value to match official Google spec/current behavior.
                if (!string.IsNullOrEmpty(value) && isNumeric(value, System.Globalization.NumberStyles.Currency))
                {
                    _price = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("InAppInitialPayment price", "Item price is required.");
                }
            }
        }

        /// <summary>
        /// Required. A 3-character currency code that defines the billing currency. Currently the only supported currency code is USD.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Required()]
        [StringLength(3, ErrorMessage = "Maximum of 3 characters")]
        [DataMember(IsRequired = true, Order = 2)]
        public string currencyCode
        {
            get { return _currencyCode; }
            set
            {
                if (!string.IsNullOrEmpty(value) && maxLength(3, value.Length))
                {
                    _currencyCode = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("InAppInitialPayment currencyCode", "Required. Maximimum of 3 characters");
                }
            }
        }
    
        
        [Required()]
        [DataMember(IsRequired = true, Order = 3)]
        public string paymentType
        {
            get { return _paymentType; }

            set
            {
                //NOTE: Pending official clarification.
                //   Currently Google sandbox allows 0 value for itemPrice.
                //   While it would be odd to allow 0 value payments, allow 0 value to match official Google spec/current behavior.
                if (!string.IsNullOrEmpty(value))
                {
                    _paymentType = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("InAppInitialPayment paymentType", "Item payment iype is required.");
                }
            }
        }

        public InAppInitialPayment()
        {
        }

       

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
            if (string.IsNullOrEmpty(this.price) || string.IsNullOrEmpty(this.currencyCode) || string.IsNullOrEmpty(this.paymentType))
            {
                throw new NullReferenceException("Invalid InAppInitialPayment - make sure you provide all required properties");
            }
        }

       
        public bool isNumeric(string val, System.Globalization.NumberStyles NumberStyle)
        {
            Double result;
            return Double.TryParse(val, NumberStyle,
                System.Globalization.CultureInfo.CurrentCulture, out result);
        }
        private bool maxLength(int maxCharacterLength, int currentLength)
        {
            return currentLength <= maxCharacterLength;
        }
    }
}
