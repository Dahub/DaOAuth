using DaOAuthCore.Dal.Interface;
using Microsoft.EntityFrameworkCore;

namespace DaOAuthCore.Dal.EF
{
    public class EfRepositoriesFactory : IRepositoriesFactory
    {
        public IContext CreateContext(string connexion)
        {
            var builder = new DbContextOptionsBuilder<DaOAuthContext>();
            builder.UseSqlServer(connexion, b => b.MigrationsAssembly("DaOAuthCore.WebServer"));
            
            return new DaOAuthContext(builder.Options);           
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

        public IRessourceServerRepository GetRessourceServerRepository(IContext context)
        {
            return new RessourceServerRepository()
            {
                Context = context
            };
        }
    }
}
