namespace ApiObjects.SearchObjects
{
    public class LabelValue
    {
        public long Id { get; set; }

        public string Value { get; set; }

        public EntityAttribute EntityAttribute { get; set; }

        public LabelValue(long id, EntityAttribute entityAttribute, string value)
        {
            Id = id;
            EntityAttribute = entityAttribute;
            Value = value;
        }

        public override string ToString()
        {
            return $"{{{nameof(Id)}={Id}, {nameof(EntityAttribute)}={EntityAttribute}, {nameof(Value)}=\"{Value}\"}}";
        }
    }
}
