using DaOAuth.Dal.Interface;
using DaOAuth.Domain;
using System.Data.Entity;

namespace DaOAuth.Dal.EF
{
    internal class CodeRepository : ICodeRepository
    {
        public IContext Context { get; set; }

        public void Add(Code toAdd)
        {
            ((DbContext)Context).Set<Code>().Add(toAdd);
        }
    }
}
