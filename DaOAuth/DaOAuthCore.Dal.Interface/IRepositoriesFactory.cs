namespace DaOAuthCore.Dal.Interface
{
    public interface IRepositoriesFactory
    {
        IContext CreateContext(string connexion);
        IClientRepository GetClientRepository(IContext context);
        ICodeRepository GetCodeRepository(IContext context);
        IUserRepository GetUserRepository(IContext context);
        IUserClientRepository GetUserClientRepository(IContext context);
        IScopeRepository GetScopeRepository(IContext context);
    }
}
