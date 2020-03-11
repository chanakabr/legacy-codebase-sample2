using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.MultiClient.Configuration;
using System.Data;
using Tvinci.Helpers;
using Tvinci.Data.DataLoader.PredefinedAdapters;

namespace TVPPro.SiteManager.Manager
{
	public class SiteManager
	{
		public static List<Client> ExtractClientsList()
		{
			List<Client> result = new List<Client>();

			if (Tvinci.MultiClient.MultiClientHelper.Instance.Data.Definitions.Mode == Mode.Prohibited)
			{
				// no need to access client list
				return result;
			}

			DataTable dt = new DatabaseDirectAdapter(delegate(ODBCWrapper.DataSetSelectQuery query)
			{
				query += "select clientToken, name  from clients where ";
				query += DatabaseHelper.AddCommonFields("status", "is_active", eExecuteLocation.Application, false);
			}).Execute();

			if (dt != null)
			{
				foreach (DataRow row in dt.Rows)
				{
					result.Add(new Client() { ClientID = (string) row["clientToken"] });
				}
			}


			return result;
		}
	}
}
