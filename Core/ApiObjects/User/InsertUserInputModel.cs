namespace ApiObjects.User
{
    public class InsertUserInputModel
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Salt { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FacebookId { get; set; }

        public string FacebookImage { get; set; }

        public string FacebookToken { get; set; }

        public bool IsFacebookImagePermitted { get; set; }

        public string Email { get; set; }

        public bool ActivateStatus { get; set; }

        public string ActivationToken { get; set; }

        public string CoGuid { get; set; }

        public string ExternalToken { get; set; }

        public int? UserTypeId { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public int CountryId { get; set; }

        public int StateId { get; set; }

        public string Zip { get; set; }

        public string Phone { get; set; }

        public string AffiliateCode { get; set; }

        public string TwitterToken { get; set; }

        public string TwitterTokenSecret { get; set; }

        public int GroupId { get; set; }

        public bool UsernameEncryptionEnabled { get; set; }
    }
}