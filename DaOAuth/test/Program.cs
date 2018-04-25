using DaOAuth.Dal.EF;
using DaOAuth.Service;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.DataHandler.Serializer;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
// using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            //ClientService s = new ClientService();
            //s.ConnexionString = ConfigurationWrapper.Instance.ConnexionString; // "Server=localhost\\SQLEXPRESS;Database=DaOAuth;Trusted_Connection=True;";
            //s.Factory = new EfRepositoriesFactory();
            //s.CreateNewClient("hello", "http://www.google.fr");

            //System.ServiceModel.Security.Tokens.Se
            TestToken tt = new TestToken();
            // tt.

            /*
            var ticket = new AuthenticationTicket(identity, props);
            var accessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
            */
            ClaimsIdentity identity = new ClaimsIdentity(new List<Claim>()
            {
                new Claim("test","plop")
            },
            OAuthDefaults.AuthenticationType);

            

            var tokenExpiration = TimeSpan.FromDays(900);
            var props = new AuthenticationProperties()
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration),
            };
            var ticket = new AuthenticationTicket(identity, props);

            // https://stackoverflow.com/questions/28406624/asp-net-oauth-how-is-access-token-generated?rq=1
            // https://github.com/aspnet/AspNetKatana/tree/dev/src/Microsoft.Owin.Security
            var aa = new OAuthBearerAuthenticationOptions()
            {
                AccessTokenFormat = new SecureDataFormat<AuthenticationTicket>(new TicketSerializer(), new DpapiDataProtectionProvider("DaOAuth").Create(new string[] { "OAuth" }), new Base64UrlTextEncoder())
            };

            var accessToken = aa.AccessTokenFormat.Protect(ticket);

            Console.WriteLine(accessToken);
            Console.WriteLine();

            accessToken = aa.AccessTokenFormat.Protect(ticket);

            Console.WriteLine(accessToken);
            Console.WriteLine();

            accessToken = aa.AccessTokenFormat.Protect(ticket);

            Console.WriteLine(accessToken);
            Console.WriteLine();

            var ttt = aa.AccessTokenFormat.Unprotect(accessToken);

            Console.WriteLine(ttt);

            Console.Read();
        }
    }

    public class TestToken : SecurityToken
    {
        public override string Id => throw new NotImplementedException();

        public override ReadOnlyCollection<SecurityKey> SecurityKeys => throw new NotImplementedException();

        public override DateTime ValidFrom => throw new NotImplementedException();

        public override DateTime ValidTo => throw new NotImplementedException();
    }
}
