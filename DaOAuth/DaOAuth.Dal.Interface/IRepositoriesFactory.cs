namespace DaOAuth.Dal.Interface
{
    public interface IRepositoriesFactory
    {
        IContext CreateContext(string connexion);
        IClientRepository GetClientRepository(IContext context);
        ICodeRepository GetCodeRepository(IContext context);
    }
}
