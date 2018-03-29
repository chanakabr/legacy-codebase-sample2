using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ConfigurationManager
{
    public abstract class ConfigurationValue
    {
        #region Members

        internal string Key;
        internal object ObjectValue;
        internal string Description;
        internal bool ShouldAllowEmpty;
        public object DefaultValue;
        internal ConfigurationValue Parent;
        internal List<ConfigurationValue> Children;
        internal string OriginalKey;

        #endregion

        #region Ctor

        /// <summary>
        /// Basic, straight forward TCM value (key:value)
        /// </summary>
        /// <param name="key"></param>
        public ConfigurationValue(string key)
        {
            this.ShouldAllowEmpty = false;
            this.Key = key;
            this.ObjectValue = TCMClient.Settings.Instance.GetValue<object>(this.Key);
            this.Children = new List<ConfigurationValue>();
        }


        /// <summary>
        /// A sub value; an inner part of a larger configuration object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parent"></param>
        public ConfigurationValue(string key, ConfigurationValue parent)
        {
            this.Key = key.ToLower();
            this.Parent = parent;
            this.Children = new List<ConfigurationValue>();

            if (parent != null)
            {
                // Add this value to the list of the parent's children
                parent.Children.Add(this);

                if (parent.ObjectValue != null)
                {
                    Newtonsoft.Json.Linq.JObject parentValue = null;

                    // This might not work because of differences between JSON embedded in TCM client and JSON reference
                    if (parentValue == null)
                    {
                        parentValue = parent.ObjectValue as Newtonsoft.Json.Linq.JObject;
                    }

                    // If the previous part doesn't work, we will re-parse the JSON on our own...
                    if (parentValue == null)
                    {
                        string parentValueString = Convert.ToString(parent.ObjectValue);

                        parentValue = JObject.Parse(parentValueString);
                    }

                    if (parentValue != null)
                    {
                        var childValue = parentValue[this.Key];

                        if (childValue != null)
                        {
                            this.ObjectValue = childValue.Value<object>();
                        }
                        else
                        {
                            LogError(string.Format("parent value {0} has missing child {1}", parent.Key, this.Key), ConfigurationValidationErrorLevel.Model);
                        }
                    }
                }
            }
        }

        #endregion
        
        #region Virtual Methods

        internal virtual bool Validate()
        {
            bool result = true;

            if (this.Children != null)
            {
                foreach (var child in this.Children)
                {
                    result &= child.Validate();
                }
            }

            if (!string.IsNullOrEmpty(this.OriginalKey))
            {
                ApplicationConfiguration.AddConfigurationValueWithOrigin(this);
            }

            return result;
        }

        protected virtual void LogError(string reason, ConfigurationValidationErrorLevel level)
        {
            StringBuilder builder = new StringBuilder(string.Format("!!{0}!! Key: {1}. Reason: {2}. ", 
                level.ToString().ToUpper(), 
                this.GetFullKey(), 
                reason));

            if (this.DefaultValue != null)
            {
                builder.AppendFormat("Default Value: {0}. ", this.DefaultValue);
            }

            if (this.ShouldAllowEmpty)
            {
                builder.Append("Optional value. ");
            }

            if (!string.IsNullOrEmpty(this.Description))
            {
                builder.AppendFormat("Description: {0} ", this.Description);
            }

            if (!string.IsNullOrEmpty(this.OriginalKey))
            {
                builder.AppendFormat("Original Key: {0} ", this.OriginalKey);
            }

            string log = builder.ToString();

            ApplicationConfiguration.WriteToLog(log);
        }

        #endregion

        #region Public Methods

        internal string GetFullKey()
        {
            // default - use the normal key
            string result = this.Key;

            // otherwise, string together all keys from parents, in a non recursive manner (all hail the stack)
            if (this.Parent != null)
            {
                Stack<ConfigurationValue> stack = new Stack<ConfigurationValue>();
                ConfigurationValue currentValue = this;

                stack.Push(currentValue);

                while (currentValue.Parent != null)
                {
                    currentValue = currentValue.Parent;
                    stack.Push(currentValue);
                }

                StringBuilder builder = new StringBuilder();

                while (stack.Count > 0)
                {
                    currentValue = stack.Pop();
                    builder.AppendFormat("{0}.", currentValue.Key);
                }

                // remove last .
                if (builder.Length > 0)
                {
                    builder.Remove(builder.Length - 1, 1);
                }

                result = builder.ToString();
            }

            return result;
        }

        public void LoadDefault()
        {
            if (this.ObjectValue == null)
            {
                this.ObjectValue = this.DefaultValue;
            }

            if (this.Children != null && this.Children.Count > 0)
            {
                foreach (var child in this.Children)
                {
                    child.LoadDefault();
                }
            }
        }

        #endregion

    }

    public enum ConfigurationValidationErrorLevel
    {
        Optional,
        Model,
        Failure
    }
}