using System.Collections.Generic;
using System.Reflection;

namespace ApiObjects
{
    public class KalturaObjectChangedEvent : KalturaObjectActionEvent
    {
        public CoreObject PreviousObject { get; set; }
        public List<string> ChangedFields { get; set; }

        public KalturaObjectChangedEvent(int groupId = 0, CoreObject newObject = null, CoreObject previousObject = null, List<string> changedFields = null, 
                                         eKalturaEventTime time = eKalturaEventTime.After, string type = null)
            : base(groupId, newObject, eKalturaEventActions.Changed, time, type)
        {
            this.PreviousObject = previousObject;
            this.ChangedFields = changedFields;

            GetChangedFields(newObject, previousObject);
        }

        private void GetChangedFields(CoreObject newObject, CoreObject previousObject)
        {
            if (this.ChangedFields == null)
            {
                this.ChangedFields = new List<string>();

                var properties = GetProperties(newObject);

                foreach (var property in properties)
                {
                    string name = property.Name;
                    var newValue = property.GetValue(newObject, null);
                    var oldValue = property.GetValue(previousObject, null);

                    if (newValue != null)
                    {
                        if (oldValue != null)
                        {
                            if (!newValue.Equals(oldValue))
                            {
                                this.ChangedFields.Add(property.Name);
                            }
                        }
                        else
                        {
                            this.ChangedFields.Add(property.Name);
                        }
                    }
                    else if (oldValue != null)
                    {
                        this.ChangedFields.Add(property.Name);
                    }
                }
            }
        }

        private static PropertyInfo[] GetProperties(object obj)
        {
            return obj.GetType().GetProperties();
        }
    }
}