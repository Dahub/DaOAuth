namespace DaOAuthCore.Service
{
    public interface IRessourceServerService
    {
        bool AreRessourceServerCredentialsValid(string basicAuthCredentials);
        string[] GetAllRessourcesServersNames();
    }
}
