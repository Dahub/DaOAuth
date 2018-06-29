namespace DaOAuthCore.Domain
{
    public class ClientReturnUrl
    {
        public int Id { get; set; }
        public string ReturnUrl { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }
    }
}
