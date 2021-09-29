namespace ApiObjects.BulkUpload
{
    public class EpgProgramInfo
    {
        public string LanguageCode { get; set; }

        public string DocumentId { get; set; }

        public string EpgExternalId { get; set; }

        public bool IsAutofill { get; set; }
        
        public int GroupId { get; set; }
    }
}