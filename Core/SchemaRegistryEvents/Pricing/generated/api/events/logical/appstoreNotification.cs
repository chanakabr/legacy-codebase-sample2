namespace Phoenix.Generated.Api.Events.Logical.appstoreNotification
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// AIndicates that we got a notification from apple/google about subscription
    ///
    /// Google or Apple subscription state (status changed)
    ///
    /// A base event for ALL EVENTS, currently does not contain anything, placeholder for the
    /// future
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class AppstoreNotification
    {
        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        /// <summary>
        /// order ID for Google (a unique identifier of the latest recurring order associated with
        /// the purchase of the subscription). originalTransactionId for Apple (the transactionId
        /// given when the subscription was purchased)
        /// </summary>
        [JsonProperty("externalTransactionId", NullValueHandling = NullValueHandling.Ignore)]
        public string ExternalTransactionId { get; set; }

        /// <summary>
        /// Dictionary<string, Value> -> map of values of different extra fields provided by
        /// Google/Apple
        /// </summary>
        [JsonProperty("extraInformation", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> ExtraInformation { get; set; }

        /// <summary>
        /// Event source type - either from Google or Apple
        /// </summary>
        [JsonProperty("notificationSource", NullValueHandling = NullValueHandling.Ignore)]
        public NotificationSource? NotificationSource { get; set; }

        /// <summary>
        /// A unified normalized state of the subscription as received from Google's notificationType
        /// parameter or from a combination of Apple's notificationType & subType parameters.
        /// </summary>
        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public State? State { get; set; }

        /// <summary>
        /// Google Subscription ID as received from Google store or Apple productId of the in-app
        /// purchase as received from Apple store
        /// </summary>
        [JsonProperty("subscriptionId", NullValueHandling = NullValueHandling.Ignore)]
        public string SubscriptionId { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }

    /// <summary>
    /// Event source type - either from Google or Apple
    /// </summary>
    public enum NotificationSource { Apple, Google };

    /// <summary>
    /// A unified normalized state of the subscription as received from Google's notificationType
    /// parameter or from a combination of Apple's notificationType & subType parameters.
    /// </summary>
    public enum State { PriceIncreased, SubscriptionCanceled, SubscriptionExpired, SubscriptionExpiredBillingRetry, SubscriptionExpiredGracePeriod, SubscriptionInGracePeriod, SubscriptionOfferRedeemed, SubscriptionPauseRequest, SubscriptionPaused, SubscriptionPurchased, SubscriptionReEnabledRenewal, SubscriptionRecovered, SubscriptionRenewalExtended, SubscriptionRenewed, SubscriptionRevoked, SubscriptionSuspended };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                NotificationSourceConverter.Singleton,
                StateConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class NotificationSourceConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(NotificationSource) || t == typeof(NotificationSource?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Apple":
                    return NotificationSource.Apple;
                case "Google":
                    return NotificationSource.Google;
            }
            throw new Exception("Cannot unmarshal type NotificationSource");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (NotificationSource)untypedValue;
            switch (value)
            {
                case NotificationSource.Apple:
                    serializer.Serialize(writer, "Apple");
                    return;
                case NotificationSource.Google:
                    serializer.Serialize(writer, "Google");
                    return;
            }
            throw new Exception("Cannot marshal type NotificationSource");
        }

        public static readonly NotificationSourceConverter Singleton = new NotificationSourceConverter();
    }

    internal class StateConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(State) || t == typeof(State?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "priceIncreased":
                    return State.PriceIncreased;
                case "subscriptionCanceled":
                    return State.SubscriptionCanceled;
                case "subscriptionExpired":
                    return State.SubscriptionExpired;
                case "subscriptionExpiredBillingRetry":
                    return State.SubscriptionExpiredBillingRetry;
                case "subscriptionExpiredGracePeriod":
                    return State.SubscriptionExpiredGracePeriod;
                case "subscriptionInGracePeriod":
                    return State.SubscriptionInGracePeriod;
                case "subscriptionOfferRedeemed":
                    return State.SubscriptionOfferRedeemed;
                case "subscriptionPauseRequest":
                    return State.SubscriptionPauseRequest;
                case "subscriptionPaused":
                    return State.SubscriptionPaused;
                case "subscriptionPurchased":
                    return State.SubscriptionPurchased;
                case "subscriptionReEnabledRenewal":
                    return State.SubscriptionReEnabledRenewal;
                case "subscriptionRecovered":
                    return State.SubscriptionRecovered;
                case "subscriptionRenewalExtended":
                    return State.SubscriptionRenewalExtended;
                case "subscriptionRenewed":
                    return State.SubscriptionRenewed;
                case "subscriptionRevoked":
                    return State.SubscriptionRevoked;
                case "subscriptionSuspended":
                    return State.SubscriptionSuspended;
            }
            throw new Exception("Cannot unmarshal type State");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (State)untypedValue;
            switch (value)
            {
                case State.PriceIncreased:
                    serializer.Serialize(writer, "priceIncreased");
                    return;
                case State.SubscriptionCanceled:
                    serializer.Serialize(writer, "subscriptionCanceled");
                    return;
                case State.SubscriptionExpired:
                    serializer.Serialize(writer, "subscriptionExpired");
                    return;
                case State.SubscriptionExpiredBillingRetry:
                    serializer.Serialize(writer, "subscriptionExpiredBillingRetry");
                    return;
                case State.SubscriptionExpiredGracePeriod:
                    serializer.Serialize(writer, "subscriptionExpiredGracePeriod");
                    return;
                case State.SubscriptionInGracePeriod:
                    serializer.Serialize(writer, "subscriptionInGracePeriod");
                    return;
                case State.SubscriptionOfferRedeemed:
                    serializer.Serialize(writer, "subscriptionOfferRedeemed");
                    return;
                case State.SubscriptionPauseRequest:
                    serializer.Serialize(writer, "subscriptionPauseRequest");
                    return;
                case State.SubscriptionPaused:
                    serializer.Serialize(writer, "subscriptionPaused");
                    return;
                case State.SubscriptionPurchased:
                    serializer.Serialize(writer, "subscriptionPurchased");
                    return;
                case State.SubscriptionReEnabledRenewal:
                    serializer.Serialize(writer, "subscriptionReEnabledRenewal");
                    return;
                case State.SubscriptionRecovered:
                    serializer.Serialize(writer, "subscriptionRecovered");
                    return;
                case State.SubscriptionRenewalExtended:
                    serializer.Serialize(writer, "subscriptionRenewalExtended");
                    return;
                case State.SubscriptionRenewed:
                    serializer.Serialize(writer, "subscriptionRenewed");
                    return;
                case State.SubscriptionRevoked:
                    serializer.Serialize(writer, "subscriptionRevoked");
                    return;
                case State.SubscriptionSuspended:
                    serializer.Serialize(writer, "subscriptionSuspended");
                    return;
            }
            throw new Exception("Cannot marshal type State");
        }

        public static readonly StateConverter Singleton = new StateConverter();
    }
}
