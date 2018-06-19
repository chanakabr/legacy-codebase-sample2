using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using KLogMonitor;

namespace DAL
{
    public static class Helpers
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly IDictionary<Type, ICollection<PropertyInfo>> _Properties = new Dictionary<Type, ICollection<PropertyInfo>>();

        /// <summary>
        /// Converts a DataTable to a list with generic objects
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="table">DataTable</param>
        /// <returns>List with generic objects</returns>
        public static List<T> ToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                var objType = typeof(T);
                var properties = GetObjectProperties<T>(objType);

                var list = new List<T>(table.Rows.Count);

                foreach (DataRow row in table.Rows)
                {
                    var obj = ConvertRowToObject<T>(properties, row);
                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return new List<T>();
            }
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> data) where T : class, new()
        {
            var objType = typeof(T);
            var properties = GetObjectProperties<T>(objType);

            var table = new DataTable();
            foreach (var prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;
        }

        private static ICollection<PropertyInfo> GetObjectProperties<T>(Type objType) where T : class, new()
        {
            ICollection<PropertyInfo> properties;
            lock (_Properties)
            {
                if (!_Properties.TryGetValue(objType, out properties))
                {
                    properties = objType.GetProperties().Where(property => property.CanWrite).ToList();
                    _Properties.Add(objType, properties);
                }
            }
            return properties;
        }

        private static T ConvertRowToObject<T>(ICollection<PropertyInfo> properties, DataRow row) where T : class, new()
        {
            var obj = new T();

            foreach (var prop in properties)
            {
                try
                {
                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    var fieldName = prop.GetCustomAttribute<DBFieldMappingAttribute>(inherit: true)?.DbFieldName;
                    fieldName = fieldName ?? prop.Name;
                    var safeValue = row[fieldName] == null ? null : Convert.ChangeType(row[fieldName], propType);

                    prop.SetValue(obj, safeValue, null);
                }
                catch (Exception e)
                {
                    _Logger.WarnFormat("Failed to convert DB field to object for object type: [{0}] prop:[{1}], ex:{2}", obj.GetType().Name, prop.Name, e);
                }
            }
            return obj;
        }


    }
}
