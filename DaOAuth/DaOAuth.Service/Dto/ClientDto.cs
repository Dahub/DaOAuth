namespace DaOAuth.Service
{
    public class ClientDto
    {
        public string Name { get; set; }
        public bool IsValid { get; set; }
        public string PublicId { get; set; }
        public string Description { get; set; }
        public string[] Scopes { get; set; }
    }
}
