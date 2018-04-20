using DaOAuth.Dal.Interface;
using DaOAuth.Domain;

namespace DaOAuth.Dal.Fake
{
    internal class CodeRepository : ICodeRepository
    {
        public IContext Context { get; set; }

        public void Add(Code toAdd)
        {
            toAdd.Id = 32;
        }
    }
}
