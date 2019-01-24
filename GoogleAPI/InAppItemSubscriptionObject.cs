using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using TVinciShared;

namespace Tvinic.GoogleAPI
{
    [DataContract()]
    public class InAppItemSubscriptionObject : JWTClaim
    {
         #region "Notes"
        //Order Property of DataMember is used (only) for easier unit testing with Google's demo page. There is no "required order".
        //Otherwise, the default datacontract datamember order is alphabetical making it impossible to test using Google demo page.
        //Google testing/demo page: https://sandbox.google.com/checkout/customer/gadget/inapp/demo.html
        #endregion

        #region "Defaults"

        private string _typ = "google/payments/inapp/subscription/v1";
        private string _aud = "Google";


        /// <summary>
        /// Note the adjustment of -1 minute to compensate for differences between system times when generating iat, adjust as necessary. 
        /// If your system clock is faster than Google's, your JWT will be rejected (jwt spec: the current date/time MUST be after the issued date/time listed in the iat claim)
        /// Also note leap seconds in Windows vs nix.
        /// </summary>
        /// <remarks>Sorry #rapture fans: If the world still exists on 01/19/2038, this is an issue (Integer)</remarks>
        private long _iat; //= JWTHelpers.myServerClock() - 60;

        #endregion

        /// <summary>
        /// Required. Issuer Claim. Your Merchant/Seller ID. 
        /// On Postback from Google: This will be set by Google to "Google", and will be verified (case-sensitive). Adjust callback handler as needed.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Required()]
        [DataMember(IsRequired = true)]
        public override string iss { get; set; }

        /// <summary>
        /// Required. Audience Claim. Default is "Google". Provide value for future api version changes.
        /// On Postback from Google: This will be set by Google to your  Merchant/Seller ID and will be verified. Adjust callback handler as needed
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Required()]
        [DataMember(IsRequired = true)]
        public override string aud
        {
            get { return _aud; }
            set { _aud = value; }
        }

        /// <summary>
        /// Required. Type Claim. Default is "google/payments/inapp/item/v1". Provide value for future api version changes.
        /// On  Postback from Google: This will be set by Google and will be verified. Currently "google/payments/inapp/item/v1/postback/buy". Adjust callback handler as needed.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Required()]
        [DataMember(IsRequired = true)]
        public override string typ
        {
            get { return _typ; }
            set { _typ = value; }
        }

        /// <summary>
        /// Required. Issued At Claim. The time when the JWT was issued.
        /// Specified in number of seconds from 1970-01-01T0:0:0Z as measured in UTC until the desired date/time.
        /// Default value is current server time with a clock adjustment of -1 minute.
        /// On Postback from Google: Google will set this value and will be verified.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Required()]
        [DataMember(IsRequired = true)]
        public override long iat
        {
            get { return _iat; }
            set { _iat = value; }
        }

        /// <summary>
        /// Optional. Expiration claim. Must be greater than issued at (iat) and maximum of 1 hour after issued at (iat).
        /// Specified in number of seconds from 1970-01-01T0:0:0Z as measured in UTC until the desired date/time.
        /// On Postback from Google: Google will set this value and will be verified.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public override long? exp { get; set; }


        /// <summary>
        /// Required. Instance of InAppItemRequestObject.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Required()]
        [DataMember(IsRequired = true, Order = 1)]
        public InAppItemRequestSubscriptionObject request = null;

        /// <summary>
        /// Used only on Google Postback
        /// Contains orderID
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public InAppItemResponseObject response = null;

        /// <summary>
        /// When using this constructor, refer to property Intellisense documentation/information/hints for requirements.
        /// Refer to Google documentation.
        /// </summary>
        /// <remarks></remarks>
        public InAppItemSubscriptionObject()
        { }

        public InAppItemSubscriptionObject(InAppItemRequestSubscriptionObject request)
        {
            this.request = request;
        }


        /// <summary>
        /// This constructor initializes a new InAppItemRequestObject.
        /// Text values exceeding maximum length requirements will be truncated. 
        /// Use named arguments for optional parameters if you don't provide all.
        /// </summary>
        /// <param name="name50">Required (name). Maximium of 50 characters (excess truncated).</param>
        /// <param name="description100">Optional (description). Pass null, Nothing or String.Empty to prevent serialization of this property. Maximum of 100 characters (excess truncated).</param>
        /// <param name="price">Required (price). Up to 2 decimal places only. Google will reject value exceeding 2 decimal places.</param>
        /// <param name="isoCurrency3">Required (isoCurrencyCode). 3 letter ISO Currency. e.g. USD (excess truncated).</param>
        /// <param name="sellerData200">Optional (sellerData). Pass null, Nothing or String.Empty to prevent serialization of this property. Maximum of 200 characters (excess truncated). </param>
        /// <param name="sellerId">Required (iss). Your Seller ID when building your JWT (you are issuer). On postback, Google is issuer and will set this value ("Google").</param>
        /// <param name="expMin60">Optional (exp). Maximium value is 60 (minutes from iat). Pass null or Nothing to prevent serialization of this property.</param>
        /// <param name="optionalAud">This is an optional parameter. Use only to override default aud value ("Google").</param>
        /// <param name="optionalTyp">This is an optional parameter. Use only to override default typ value ("google/payments/inapp/item/v1").</param>
        /// <param name="optionalIat">This is an optional parameter. Use only to override default iat value (current UTC time in number of seconds from Unix epoch with skew of -60 secs).</param>
        /// <remarks>Google Sites reference: http://sites.google.com/site/inapppaymentsapi/reference </remarks>
        public InAppItemSubscriptionObject(string name50, string description100, string Init_price, string Init_isoCurrency3, string Init_paymentType,
                       string sellerData200, string sellerId, int? expMin60, string Rec_price, string Rec_isoCurrency3, string Rec_startTime, string Rec_frequency, string Rec_numRecurrences,
                       string optionalAud = "",
                       string optionalTyp = "",
                       int optionalIat = 0)
        {

            if (!string.IsNullOrEmpty(name50) && isNumeric(Init_price, System.Globalization.NumberStyles.Currency) && !string.IsNullOrEmpty(Init_isoCurrency3) && !string.IsNullOrEmpty(sellerId) && isNumeric(Rec_price, System.Globalization.NumberStyles.Currency) && !string.IsNullOrEmpty(Rec_isoCurrency3) )
            {
                this._iat = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow) - 60;
                this.request = new InAppItemRequestSubscriptionObject();
                this.request.name = name50.Length > 50? name50.Substring(0, 50): name50;

                if(!string.IsNullOrEmpty(description100))
                {
                    this.request.description = description100.Length > 100 ? description100.Substring(0, 100): description100;
                }
                this.request.initialPayment = new InAppInitialPayment();

                this.request.initialPayment.price = Init_price;

                this.request.initialPayment.currencyCode = Init_isoCurrency3.Length > 3 ? Init_isoCurrency3.Substring(0, 3) : Init_isoCurrency3;
                if (!string.IsNullOrEmpty(Init_paymentType))
                {
                    this.request.initialPayment.paymentType = Init_paymentType;
                }

                this.request.recurrence = new InAppRecurrence();

                this.request.recurrence.price = Rec_price;

                this.request.recurrence.currencyCode = Rec_isoCurrency3.Length > 3 ? Rec_isoCurrency3.Substring(0, 3) : Rec_isoCurrency3;

                if (!string.IsNullOrEmpty(Rec_startTime))
                {
                    this.request.recurrence.startTime = Rec_startTime;
                }

                
                this.request.recurrence.frequency = Rec_frequency;
                

                if (!string.IsNullOrEmpty(Rec_numRecurrences))
                {
                    this.request.recurrence.numRecurrences = Rec_numRecurrences;
                }

                if(!string.IsNullOrEmpty(sellerData200))
                {
                    this.request.sellerData = sellerData200.Length > 200 ? sellerData200.Substring(0, 200): sellerData200;
                }

                if (expMin60.HasValue && expMin60.Value > 0)
                {
                    expMin60 = expMin60.Value <= 60 ? expMin60.Value : 60;
                    this.exp = this.iat + (expMin60.Value * 60);
                }

                this.iss = sellerId;

                if (!string.IsNullOrEmpty(optionalAud))
                {
                    this._aud = optionalAud;
                }

                if(string.IsNullOrEmpty(optionalTyp))
                {
                    this._typ = optionalTyp;
                }

                if (optionalIat > 0)
                {
                    this._iat = optionalIat;
                }
                else
                {
                    this._iat = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow) - 60;
                }
            }
            else
            {

                throw new ArgumentNullException("Invalid InAppItemObject", "You must provide required arguments.");
            }
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
            if (this.exp.HasValue)
            {
                long _issued = this.iat;
                long _expiration = this.exp.Value;
                if (_expiration <= 0 || _expiration <= _issued || _expiration > _issued + 3600)
                {
                    throw new ArgumentOutOfRangeException("InAppItemObject exp", "Expiration time claim (exp) must be greater than issued at claim (iat) and maximum of 1 hour after issued at claim (iat)");
                }
            }
            if (string.IsNullOrEmpty(this.iss) || string.IsNullOrEmpty(this.aud) || string.IsNullOrEmpty(this.typ) || this.iat <= 0 || this.request == null)
            {
                throw new NullReferenceException("Invalid InAppItemObject - make sure you provide all required properties.");
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
