﻿using DaOAuth.Dal.Interface;

namespace DaOAuth.Dal.EF
{
    public class EfRepositoriesFactory : IRepositoriesFactory
    {
        public IContext CreateContext(string connexion)
        {
            return new DaOAuthContext(connexion);           
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
    }
}
