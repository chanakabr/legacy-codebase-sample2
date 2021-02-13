namespace ApiObjects.BulkUpload
{
    public interface IAffectedObject
    {
        /// <summary>
        /// The AffectedObject Id must be unique as it is used to update the list of objects
        /// </summary>
        ulong ObjectId { get; }
        string EpgExternalId { get; }
        int ChannelId { get; }
        bool IsAutoFill { get; }
    }
}