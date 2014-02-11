using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
namespace Tvinic.GoogleAPI
{
    [DataContract()]
    public class InAppItemRequestObject
    {

        #region "Defaults"

        private string _currencyCode = null;
        private string _price = null;
        private string _sellerData = null;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private InAppInitialPayment _initialPayment = null;
        private InAppRecurrence _recurrence = null;
        
        #endregion

        /// <summary>
        /// Required. A 3-character currency code that defines the billing currency. Currently the only supported currency code is USD.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
       
        [StringLength(3, ErrorMessage = "Maximum of 3 characters")]
        [DataMember(EmitDefaultValue = false, IsRequired = false, Order = 1)]
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
                    throw new ArgumentOutOfRangeException("InAppItemRequestObject currencyCode", "Required. Maximimum of 3 characters");
                }
            }
        }

        /// <summary>
        /// Required. The purchase price of the item, with up to 2 decimal places.
        /// Google will reject value exceeding 2 decimal places.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>

        [DataMember(EmitDefaultValue = false, IsRequired = false, Order = 2)]
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
                    throw new ArgumentOutOfRangeException("InAppItemRequestObject price", "Item price is required.");
                }
            }
        }

        /// <summary>
        /// Optional: Data to be passed to your success and failure callbacks. The string can have no more than 200 characters.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [StringLength(200, ErrorMessage = "Maximum of 200 characters")]
        [DataMember(EmitDefaultValue = false, IsRequired = false, Order = 3)]
        public string sellerData
        {
            get { return _sellerData; }
            set
            {
                if (maxLength(200, value.Length))
                {
                    _sellerData = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("InAppItemRequestObject sellerData", "Maximimum of 200 characters");
                }

            }

        }

        /// <summary>
        /// Required. The name of the item. This name is displayed prominently in the purchase flow UI and can have no more than 50 characters.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Required()]
        [StringLength(50, ErrorMessage = "Maximum of 50 characters")]
        [DataMember(IsRequired = true, Order = 4)]
        public string name
        {
            get { return _name; }
            set
            {

                if (maxLength(50, value.Length))
                {
                    _name = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("InAppItemRequestObject name", "Maximimum of 50 characters");
                }
            }
        }

        /// <summary>
        /// Optional: Text that describes the item. This description is displayed in the purchase flow UI and can have no more than 100 characters.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [StringLength(100, ErrorMessage = "Maximum of 100 characters")]
        [DataMember(EmitDefaultValue = false, IsRequired = false, Order = 4)]
        public string description
        {
            get { return _description; }
            set
            {
                if (maxLength(100, value.Length))
                {
                    _description = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("InAppItemRequestObject description", "Maximimum of 100 characters");
                }
            }
        }

        
        [DataMember(EmitDefaultValue = false, IsRequired = false, Order = 5)]
        public InAppInitialPayment initialPayment
        {
            get { return _initialPayment; }
            set { _initialPayment = value; }
        }
       
        [DataMember(EmitDefaultValue = false, IsRequired = false, Order = 6)]
        public InAppRecurrence recurrence
        {
            get { return _recurrence; }
            set { _recurrence = value; }
        }

      

        /// <summary>
        /// Refer to property Intellisense documentation/information/hints for requirements (will throw exception).
        /// </summary>
        /// <remarks></remarks>
        public InAppItemRequestObject()
        {
        }

        private bool maxLength(int maxCharacterLength, int currentLength)
        {
            return currentLength <= maxCharacterLength;
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
            if (initialPayment != null && recurrence != null &&(string.IsNullOrEmpty(this.initialPayment.price) || string.IsNullOrEmpty(this.initialPayment.currencyCode) || string.IsNullOrEmpty(this.recurrence.price) || string.IsNullOrEmpty(this.recurrence.currencyCode)))
            {
                 throw new NullReferenceException("Invalid InAppItemRequestObject - make sure you provide all required properties");
            }
            else if ((initialPayment == null && recurrence == null) && (string.IsNullOrEmpty(this.name) || string.IsNullOrEmpty(this.price) || string.IsNullOrEmpty(this.currencyCode)))
            {
                throw new NullReferenceException("Invalid InAppItemRequestObject - make sure you provide all required properties");
            }
        }
        public bool isNumeric(string val, System.Globalization.NumberStyles NumberStyle)
        {
            Double result;
            return Double.TryParse(val, NumberStyle,
                System.Globalization.CultureInfo.CurrentCulture, out result);
        }
    }
}
