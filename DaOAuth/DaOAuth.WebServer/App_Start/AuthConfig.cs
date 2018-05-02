using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;

[assembly: OwinStartup(typeof(DaOAuth.WebServer.AuthConfig))]
namespace DaOAuth.WebServer
{
    public class AuthConfig
    {
        internal static OAuthBearerAuthenticationOptions OAuthBearerOptions { get;set;}

        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Home/Index")
            });
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions()
            {
                AuthenticationType = DefaultAuthenticationTypes.ExternalBearer,
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Passive,
            };
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);
        }
    }
}