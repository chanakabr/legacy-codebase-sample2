using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Tvinic.GoogleAPI
{
    [DataContract()]
    public class InAppItemRequestSubscriptionObject
    {
        
        #region "Defaults"

        private string _sellerData = null;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private InAppInitialPayment _initialPayment = null;
        private InAppRecurrence _recurrence = null;
        #endregion

       

        /// <summary>
        /// Required. The name of the item. This name is displayed prominently in the purchase flow UI and can have no more than 50 characters.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Required()]
        [StringLength(50, ErrorMessage = "Maximum of 50 characters")]
        [DataMember(IsRequired = true, Order = 1)]
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
        [DataMember(EmitDefaultValue = false, IsRequired = false, Order = 2)]
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
        [Required()]
        [DataMember(EmitDefaultValue = false, IsRequired = true, Order = 4)]
        public InAppInitialPayment initialPayment
        {
            get { return _initialPayment; }
            set { _initialPayment = value;}
        }
        [Required()]
        [DataMember(EmitDefaultValue = false, IsRequired = true, Order = 5)]
        public InAppRecurrence recurrence
        {
            get { return _recurrence; }
            set { _recurrence = value; }
        }


        /// <summary>
        /// Refer to property Intellisense documentation/information/hints for requirements (will throw exception).
        /// </summary>
        /// <remarks></remarks>
        public InAppItemRequestSubscriptionObject()
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
            if (string.IsNullOrEmpty(this.name) || _initialPayment == null || recurrence == null)
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
