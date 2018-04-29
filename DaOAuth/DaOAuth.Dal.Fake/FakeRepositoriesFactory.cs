using DaOAuth.Dal.Interface;

namespace DaOAuth.Dal.Fake
{
    public class FakeRepositoriesFactory : IRepositoriesFactory
    {
        public IContext CreateContext(string connexion)
        {
            return new FakeContext();
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
                Context = context
            };
        }

        public ICodeRepository GetCodeRepository(IContext context)
        {
            return new CodeRepository()
            {
                Context = context
            };
        }
    }
}
