using DaOAuth.Dal.Interface;
using DaOAuth.Domain;
using System;

namespace DaOAuth.Dal.Fake
{
    internal class ClientRepository : IClientRepository
    {
        public IContext Context { get; set; }

        public void Add(Client toAdd)
        {
            toAdd.Id = 16;
        }

        public Client GetByPublicId(string publicId)
        {
            if (String.IsNullOrEmpty(publicId) || publicId == "abc")
                return null;

            return new Client()
            {
                Id = 16,
                CreationDate = DateTime.Now.AddDays(-1),
                DefautRedirectUri = "http://www.google.fr",
                IsValid = true,
                Name = "Démo",
                PublicId = publicId
            };
        }
    }
}
