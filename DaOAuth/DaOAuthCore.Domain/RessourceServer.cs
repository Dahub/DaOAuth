namespace DaOAuthCore.Domain
{
    public class RessourceServer
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public byte[] ServerSecret { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsValid { get; set; }
    }
}
