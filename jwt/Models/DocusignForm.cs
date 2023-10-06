namespace jwt.Models
{
    public class DocusignForm
    {
        public string TenantName { get; set; }
        public string TenantNRIC { get; set; }
        public string TenantEmail { get; set; }
        public string TenantAgentName { get; set; }
        public string TenantAgentNRIC { get; set; }
        public string AgentEmail { get; set; }
        public float Rent { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set;}
        public string mobile { get; set; }
    }
}
