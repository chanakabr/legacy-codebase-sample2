using System.Xml.Serialization;

namespace ApiObjects.Billing
{
    public class HouseholdPaymentMethod
    {
        [XmlIgnore]
        public int PaymentGatewayId { get; set; }
        [XmlIgnore]
        public int HouseholdId { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }
}
