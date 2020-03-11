using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Data;

namespace Tvinci.Web.Controls.Gallery.Part
{
    public sealed class ObjectContentPart : ContentPart<object>
    {
        #region Override methods
        [TemplateContainer(typeof(ContentPartItem<object>))]
        public override ITemplate Template { get; set; }

        #endregion

        public override ContentPartItem CreateItem(object contentItem, ContentPartMetadata itemMetadata)
        {
            return new ContentPartItem<object>(contentItem, itemMetadata);
        }
    }

	public abstract class DataContentPart<TItem> : ContentPart<TItem> where TItem : DataRow
	{
		public override ContentPartItem CreateItem(object contentItem, ContentPartMetadata itemMetadata)
		{
			return new ContentPartItem<TItem>((TItem)((DataRowView) contentItem).Row, itemMetadata);
		}
	}


	public sealed class DataRowContentPart : ContentPart<DataRow>
	{
		#region Override methods
		[TemplateContainer(typeof(ContentPartItem<DataRow>))]
		public override ITemplate Template { get; set; }

		#endregion

		public override ContentPartItem CreateItem(object contentItem, ContentPartMetadata itemMetadata)
		{
			return new ContentPartItem<DataRow>(contentItem, itemMetadata);
		}
	}

    public sealed class DataContentPart : ContentPart<DataRowView>
    {
        #region Override methods

        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(ContentPartItem<DataRowView>))]
        public override ITemplate Template { get; set; }

        #endregion

        public override ContentPartItem CreateItem(object contentItem, ContentPartMetadata itemMetadata)
        {
            return new ContentPartItem<DataRowView>(contentItem, itemMetadata);
        }
    }

}
