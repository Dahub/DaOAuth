using DaOAuthCore.Dal.Interface;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DaOAuthCore.Service
{
    public abstract class ServiceBase
    {
        public AppConfiguration Configuration { get; set; }
        public IRepositoriesFactory Factory { get; set; }
        public string ConnexionString { get; set; }

        protected static bool AreEqualsSha1(string toCompare, byte[] hash)
        {
            bool toReturn = false;
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hashed= sha1.ComputeHash(Encoding.UTF8.GetBytes(toCompare));
                toReturn = hashed.SequenceEqual(hash);
            }
            return toReturn;
        }

        protected static byte[] Sha1Hash(string toHash)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(Encoding.UTF8.GetBytes(toHash));
            }
        }
    }
}
