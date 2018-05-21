namespace DaOAuthCore.Domain
{
    public class ClientScope
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public int ScopeId { get; set; }
        public Scope Scope { get; set; }
    }
}
