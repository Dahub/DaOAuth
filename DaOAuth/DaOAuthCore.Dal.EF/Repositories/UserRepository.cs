﻿using DaOAuthCore.Dal.Interface;
using DaOAuthCore.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DaOAuthCore.Dal.EF
{
    internal class UserRepository : IUserRepository
    {
        public IContext Context { get; set; }

        public void Add(User toAdd)
        {
            ((DbContext)Context).Set<User>().Add(toAdd);
        }

        public User GetByUserName(string userName)
        {
            return ((DaOAuthContext)Context).Users.
                Where(c => c.UserName.Equals(userName, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        public void Update(User toUpdate)
        {
            ((DbContext)Context).Set<User>().Attach(toUpdate);
            ((DbContext)Context).Entry(toUpdate).State = EntityState.Modified;
        }
    }
}
