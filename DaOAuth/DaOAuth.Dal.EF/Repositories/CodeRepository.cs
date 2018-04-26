using DaOAuth.Dal.Interface;
using DaOAuth.Domain;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DaOAuth.Dal.EF
{
    internal class CodeRepository : ICodeRepository
    {
        public IContext Context { get; set; }

        public void Add(Code toAdd)
        {
            ((DbContext)Context).Set<Code>().Add(toAdd);
        }

        public void Delete(Code toDelete)
        {
            ((DbContext)Context).Set<Code>().Attach(toDelete);
            ((DbContext)Context).Entry(toDelete).State = EntityState.Deleted;
        }

        public IEnumerable<Code> GetAllByClientId(string clientPublicId)
        {
            return ((DaOAuthContext)Context).Codes.
                Where(c => c.Client.PublicId.Equals(clientPublicId));
        }

        public void Update(Code toUpdate)
        {
            ((DbContext)Context).Set<Code>().Attach(toUpdate);
            ((DbContext)Context).Entry(toUpdate).State = EntityState.Modified;
        }
    }
}
