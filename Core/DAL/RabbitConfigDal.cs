using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using ODBCWrapper;

namespace DAL
{
    public interface IRabbitConfigDal
    {
        Dictionary<string, string> GetRabbitRoutingBindings();
    }

    public class RabbitConfigDal: IRabbitConfigDal
    {
        private static readonly Lazy<RabbitConfigDal> LazyInstance = new Lazy<RabbitConfigDal>(() => new RabbitConfigDal(), LazyThreadSafetyMode.PublicationOnly);
        
        public static IRabbitConfigDal Instance => LazyInstance.Value;

        public Dictionary<string, string> GetRabbitRoutingBindings()
        {
            Dictionary<string, string> rabbitQueueBindings = new Dictionary<string, string>();
            DataTable result = UtilsDal.Execute("Get_RabbitQueueBindings");

            if (result?.Rows.Count > 0)
            {
                foreach (DataRow row in result.Rows)
                {
                    rabbitQueueBindings.Add(Utils.GetSafeStr(row, "QUEUE_NAME"), Utils.GetSafeStr(row, "routing_key"));
                }
            }

            return rabbitQueueBindings;
        }
    }
}
