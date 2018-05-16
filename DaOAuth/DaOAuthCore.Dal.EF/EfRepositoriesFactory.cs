using DaOAuthCore.Dal.Interface;

namespace DaOAuthCore.Dal.EF
{
    public class EfRepositoriesFactory : IRepositoriesFactory
    {
        public IContext CreateContext(string connexion)
        {
            return new DaOAuthContext(connexion);           
        }

        public IUserRepository GetUserRepository(IContext context)
        {
            return new UserRepository()
            {
                Context = context,
            };
        }

        public IClientRepository GetClientRepository(IContext context)
        {
            return new ClientRepository()
            {
                Context = context,
            };
        }

        public ICodeRepository GetCodeRepository(IContext context)
        {
            return new CodeRepository()
            {
                Context = context
            };
        }

        public IUserClientRepository GetUserClientRepository(IContext context)
        {
            return new UserClientRepository()
            {
                Context = context
            };
        }

        public IScopeRepository GetScopeRepository(IContext context)
        {
            return new ScopeRepository()
            {
                Context = context
            };
        }
    }
}
