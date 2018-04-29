using DaOAuth.Dal.Interface;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DaOAuth.Service
{
    public abstract class ServiceBase
    {
        public string ConnexionString { get; set; }
        public IRepositoriesFactory Factory { get; set; }

        protected bool AreEqualsSha1(string toCompare, byte[] hash)
        {
            bool toReturn = false;
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hashed= sha1.ComputeHash(Encoding.UTF8.GetBytes(toCompare));
                toReturn = hashed.SequenceEqual(hash);
            }
            return toReturn;
        }

        protected byte[] Sha1Hash(string toHash)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(Encoding.UTF8.GetBytes(toHash));
            }
        }
    }
}
