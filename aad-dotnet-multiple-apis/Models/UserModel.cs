namespace aad_dotnet_multiple_apis.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }

        public string UserPrincipalName { get; set; }
        public string OnPremisesDomainName { get; set; }
        public string OnPremisesImmutableId { get; set; }
        public string OnPremisesSamAccountName { get; set; }
        public string OnPremisesUserPrincipalName { get; set; }


    }
}