using System;

namespace DaOAuthCore.Dal.Interface
{
    public interface IContext : IDisposable
    {
        void Commit();
        void CommitAsync();
    }
}