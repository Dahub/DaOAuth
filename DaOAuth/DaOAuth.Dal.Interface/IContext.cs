using System;

namespace DaOAuth.Dal.Interface
{
    public interface IContext : IDisposable
    {
        void Commit();
        void CommitAsync();
    }
}
