using System;
using System.Collections.Generic;
using System.Configuration;

namespace DaOAuth.Service
{
    public class ConfigurationWrapper
    {
        private ConfigurationWrapper() { }

        public static ConfigurationWrapper Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly ConfigurationWrapper instance = new ConfigurationWrapper();
        }

        public string ConnexionString
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["DaOAuth"];

                if (cs == null || String.IsNullOrEmpty(cs.ConnectionString))
                    throw new DaOauthServiceException("Connexion string DaOAuth absente ou non définie");

                return cs.ConnectionString;
            }
        }

        public string PasswordSalt
        {
            get
            {
                return ExtractStringParam("passwordSalt");
            }
        }

        #region private

        private static string ExtractStringParam(string paramName)
        {
            string toReturn = ConfigurationManager.AppSettings[paramName];
            if (String.IsNullOrEmpty(toReturn))
            {
                throw new DaOauthServiceException(String.Format("Le paramètre {0} est absent ou vide", paramName));
            }
            return toReturn;
        }

        private static int ExtractIntParam(string paramName)
        {
            int toReturn;

            var value = ConfigurationManager.AppSettings[paramName];

            if (String.IsNullOrEmpty(value))
            {
                throw new DaOauthServiceException(String.Format("Le paramètre {0} est absent ou vide", paramName));
            }
            if (!Int32.TryParse(value, out toReturn))
            {
                throw new DaOauthServiceException(String.Format("Le paramètre {0} à la valeur {1} n'est pas convertible en int", paramName, value));
            }
            return toReturn;
        }

        private static bool ExtractBoolParam(string paramName)
        {
            bool toReturn;

            var value = ConfigurationManager.AppSettings[paramName];

            if (String.IsNullOrEmpty(value))
            {
                throw new DaOauthServiceException(String.Format("Le paramètre {0} est absent ou vide", paramName));
            }
            if (!Boolean.TryParse(value, out toReturn))
            {
                throw new DaOauthServiceException(String.Format("Le paramètre {0} à la valeur {1} n'est pas convertible en booléen", paramName, value));
            }
            return toReturn;
        }

        private static decimal ExtractDecimalParam(string paramName)
        {
            decimal toReturn;

            var value = ConfigurationManager.AppSettings[paramName];

            if (String.IsNullOrEmpty(value))
            {
                throw new DaOauthServiceException(String.Format("Le paramètre {0} est absent ou vide", paramName));
            }
            if (!decimal.TryParse(value, out toReturn))
            {
                throw new DaOauthServiceException(String.Format("Le paramètre {0} à la valeur {1} n'est pas convertible en double", paramName, value));
            }
            return toReturn;
        }

        private static IList<int> ExtractIntListParams(string paramName)
        {
            IList<int> toReturn = new List<int>();

            string value = ConfigurationManager.AppSettings[paramName];

            if (String.IsNullOrEmpty(value))
            {
                throw new DaOauthServiceException(String.Format("Le paramètre {0} est absent ou vide", paramName));
            }

            string[] ids = value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ids.Length; i++)
            {
                int myId;
                if (!Int32.TryParse(ids[i], out myId))
                {
                    throw new DaOauthServiceException(String.Format("La valeur du paramètre {0} doit être une liste d'entiers séparés par des ;", paramName));
                }
                toReturn.Add(myId);
            }

            return toReturn;
        }

        #endregion
    }
}
