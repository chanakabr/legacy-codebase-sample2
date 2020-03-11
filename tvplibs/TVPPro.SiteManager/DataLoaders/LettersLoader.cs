using System;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Manager;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using System.Data;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.Context;

namespace TVPPro.SiteManager.DataLoaders
{
	[Serializable]
	public class LetterLoader : CustomAdapter<DataTable>
	{
        public Enums.eAddToSide SignSide
        {
            get
            {
                return Parameters.GetParameter<Enums.eAddToSide>(eParameterType.Retrieve, "SignSide", Enums.eAddToSide.Left);
            }
            set
            {
                Parameters.SetParameter<Enums.eAddToSide>(eParameterType.Retrieve, "SignSide", value);
            }
        }

        public string AllSign
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "AllSign", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "AllSign", value);
            }
        }

		public string NumericSign
		{
			get
			{
				return Parameters.GetParameter<string>(eParameterType.Retrieve, "NumericSign", string.Empty);
			}
			set
			{
				Parameters.SetParameter<string>(eParameterType.Retrieve, "NumericSign", value);
			}
		}

		public LetterLoader()
		{

		}

		protected override DataTable CreateSourceResult()
		{
			DataTable table = new DatabaseDirectAdapter(delegate(ODBCWrapper.DataSetSelectQuery query)
			{
				query += "select sbl.letter 'Letter' from SearchByLetters sbl where ";
				query += ODBCWrapper.Parameter.NEW_PARAM("sbl.languageid", "=", TextLocalization.Instance.UserContext.ValueInDB);
				query += "order by sbl.letter";
			}).Execute();

			if (!string.IsNullOrEmpty(NumericSign))
			{
				DataRow newRow = table.NewRow();
				newRow["Letter"] = NumericSign;

				if(SignSide == Enums.eAddToSide.Right) 
					table.Rows.InsertAt(newRow, table.Rows.Count);
				else
					table.Rows.InsertAt(newRow, 0);
			}

            if (!string.IsNullOrEmpty(AllSign))
            {
                DataRow newRow = table.NewRow();
                newRow["Letter"] = AllSign;

                if (SignSide == Enums.eAddToSide.Right)
                    table.Rows.InsertAt(newRow, table.Rows.Count);
                else
                    table.Rows.InsertAt(newRow, 0);
            }

			return table;
		}

		protected override Guid UniqueIdentifier
		{
			get { return new Guid("{1B05388F-7FE1-4ce6-9B4E-C7D7AD3BBD05}"); }
		}
	}
}
