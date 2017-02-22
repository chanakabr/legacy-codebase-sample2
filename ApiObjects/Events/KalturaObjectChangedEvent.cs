using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class KalturaObjectChangedEvent : KalturaObjectActionEvent
    {
        public ApiObjects.CoreObject PreviousObject
        {
            get;
            set;
        }

        public List<string> ChangedFields
        {
            get;
            set;
        }

        public KalturaObjectChangedEvent(int groupId = 0, ApiObjects.CoreObject newObject = null, ApiObjects.CoreObject previousObject = null, 
            List<string> changedFields = null, string type = null)
            : base(groupId, newObject, eKalturaEventActions.Changed, eKalturaEventTime.After, type)
        {
            this.PreviousObject = previousObject;
            this.ChangedFields = changedFields;

            GetChangedFields(newObject, previousObject);
        }

        private void GetChangedFields(ApiObjects.CoreObject newObject, ApiObjects.CoreObject previousObject)
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
