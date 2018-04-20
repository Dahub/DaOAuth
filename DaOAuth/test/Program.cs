using DaOAuth.Dal.EF;
using DaOAuth.Service;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientService s = new ClientService();
            s.ConnexionString = ConfigurationWrapper.Instance.ConnexionString; // "Server=localhost\\SQLEXPRESS;Database=DaOAuth;Trusted_Connection=True;";
            s.Factory = new EfRepositoriesFactory();
            s.CreateNewClient("hello", "http://www.google.fr");
        }
    }
}
