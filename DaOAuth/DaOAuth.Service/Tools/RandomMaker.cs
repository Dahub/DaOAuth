using System;
using System.Security.Cryptography;

namespace DaOAuth.Service
{
    internal static class RandomMaker
    {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        internal static string GenerateRandomString(int stringLenght)
        {
            string s = "";
            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                while (s.Length != stringLenght)
                {
                    byte[] oneByte = new byte[1];
                    provider.GetBytes(oneByte);
                    char character = (char)oneByte[0];
                    if (valid.Contains(character.ToString()))
                    {
                        s += character;
                    }
                }
            }
            return s;
        }
    }
}
