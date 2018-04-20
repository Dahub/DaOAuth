using DaOAuth.Dal.Interface;

namespace DaOAuth.Service
{
    public abstract class ServiceBase
    {
        public string ConnexionString { get; set; }
        public IRepositoriesFactory Factory { get; set; }
    }
}
