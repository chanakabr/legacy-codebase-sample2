using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.ComponentModel;

namespace Tvinci.Web.Controls
{
	public class PagePersistant : Control
	{
		public Dictionary<string, string> Items { get; private set; }

		
		public PagePersistant()
		{
			
			Items = new Dictionary<string, string>();
		}

		protected override void OnInit(EventArgs e)
		{
			Page.RegisterRequiresControlState(this);
			base.OnInit(e);
		}

		protected override void LoadControlState(object savedState)
		{
			Pair pair = (Pair)savedState;

			string[] items = ((string) pair.Second).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string item in items)
			{
				string[] itemParts = item.Split('|');

				Items.Add(itemParts[0], itemParts[1]);
			}
			
			base.LoadControlState(pair.First);
		}

		protected override object SaveControlState()
		{
			StringBuilder sb = new StringBuilder();
			foreach (KeyValuePair<string, string> item in Items)
			{
				if (sb.Length != 0)
				{
					sb.Append(";");
				}

				sb.AppendFormat("{0}|{1}", item.Key, item.Value);
			}

			return new Pair(base.SaveControlState(),sb.ToString());
		}

		/// <summary>
		/// Returns value of querystring parameter.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public string GetString(string key)
		{
			return Items[key];
		}

		/// <summary>
		/// Returns value of querystring parameter. If parameter not found returns the default value.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public string GetString(string key, string defaultValue)
		{
			string result;

			if (TryGetString(key, out result))
			{
				return result;
			}
			else
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// Returns value of querystring parameter. if value exists returns true and the value as out parameter.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetValue<TValue>(string key, out TValue value)
		{
			string tempValue;

			if (Items.TryGetValue(key,out tempValue))
			{
				value = getValue<TValue>(key);
				return true;
			}
			else
			{
				value = default(TValue);
				return false;
			}
		}

		/// <summary>
		/// Returns value of querystring parameter. if value exists returns true and the value as out parameter
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public  bool TryGetString(string key, out string value)
		{
			return (Items.TryGetValue(key,out value));			
		}

		/// <summary>
		/// Returns value of querystring parameter. throw exception if key not found.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		private  TValue getValue<TValue>(string key)
		{
			string tempValue;

			if (TryGetString(key, out tempValue))
			{
				return (TValue) TypeDescriptor.GetConverter(typeof(TValue)).ConvertTo(tempValue, typeof(TValue));
			}
			else
			{
				return default(TValue);
			}
		}

		/// <summary>
		/// Returns value of querystring parameter.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public  TValue GetValue<TValue>(string key, TValue defaultValue)
		{
			string tempValue;

			if (TryGetString(key, out tempValue))
			{
				return (TValue) TypeDescriptor.GetConverter(typeof(TValue)).ConvertTo(tempValue, typeof(TValue));
			}
			else
			{
				return defaultValue;
			}
		}

		public  object GetObject<TValue>(string key, object defaultValue)
		{
			string tempValue;

			if (TryGetString(key, out tempValue))
			{
				return (TValue) TypeDescriptor.GetConverter(typeof(TValue)).ConvertTo(tempValue, typeof(TValue));
			}
			else
			{
				return defaultValue;
			}
		}

	}
}
