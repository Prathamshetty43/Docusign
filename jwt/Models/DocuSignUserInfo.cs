public class DocuSignUserInfo
{
    public string Sub { get; set; }
    public string Name { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public DateTime Created { get; set; }
    public string Email { get; set; }
    public List<DocuSignAccount> Accounts { get; set; }

    public class DocuSignAccount
    {
        public string AccountId { get; set; }
        public bool IsDefault { get; set; }
        public string AccountName { get; set; }
        public string BaseUri { get; set; }
    }
}
