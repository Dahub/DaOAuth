using System;
using System.Security.Cryptography;

namespace DaOAuth.Service
{
    internal static class RandomMaker
    {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        const string validInt = "0123456789";

        internal static int GenerateRandomInt(int digits)
        {
            return Int32.Parse(GenerateRandom(digits, validInt));

            //string s = "";
            //using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            //{
            //    while (s.Length != digits)
            //    {
            //        byte[] oneByte = new byte[1];
            //        provider.GetBytes(oneByte);
            //        char character = (char)oneByte[0];
            //        if (validInt.Contains(character.ToString()))
            //        {
            //            s += character;
            //        }
            //    }
            //}
            //return Int32.Parse(s);
        }

        internal static string GenerateRandomString(int stringLenght)
        {
            return GenerateRandom(stringLenght, valid);

            //string s = "";
            //using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            //{
            //    while (s.Length != stringLenght)
            //    {
            //        byte[] oneByte = new byte[1];
            //        provider.GetBytes(oneByte);
            //        char character = (char)oneByte[0];
            //        if (valid.Contains(character.ToString()))
            //        {
            //            s += character;
            //        }
            //    }
            //}
            //return s;
        }

        private static string GenerateRandom(int length, string valids)
        {
            string s = "";
            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                while (s.Length != length)
                {
                    byte[] oneByte = new byte[1];
                    provider.GetBytes(oneByte);
                    char character = (char)oneByte[0];
                    if (valids.Contains(character.ToString()))
                    {
                        s += character;
                    }
                }
            }
            return s;
        }
    }
}
