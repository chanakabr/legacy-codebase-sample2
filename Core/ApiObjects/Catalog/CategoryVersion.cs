using ApiObjects.Base;

namespace ApiObjects.Catalog
{
    public class CategoryVersion : ICrudHandeledObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long TreeId { get; set; }
        public CategoryVersionState State { get; set; }
        public long BaseVersionId { get; set; }
        public long CategoryItemRootId { get; set; }
        public long? DefaultDate { get; set; }
        public long UpdaterId { get; set; }
        public string Comment { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }

        public bool SetUnchangedProperties(CategoryVersion oldVersion)
        {
            bool needToUpdate = false;

            this.Id = oldVersion.Id;
            this.TreeId = oldVersion.TreeId;
            this.State = oldVersion.State;
            this.BaseVersionId = oldVersion.BaseVersionId;
            this.CategoryItemRootId = oldVersion.CategoryItemRootId;
            this.DefaultDate = oldVersion.DefaultDate;
            this.UpdaterId = oldVersion.UpdaterId;
            this.CreateDate = oldVersion.CreateDate;
            this.UpdateDate = oldVersion.UpdateDate;

            if (Name == null)
            {
                this.Name = oldVersion.Name;
            }
            else
            {
                needToUpdate = true;
            }
            
            if (this.Comment == null)
            {
                this.Comment = oldVersion.Comment;
            }
            else
            {
                needToUpdate = true;
            }

            return needToUpdate;
        }
    }

    public enum CategoryVersionState
    {
        Draft = 0, Default = 1, Released = 2
    }
}
