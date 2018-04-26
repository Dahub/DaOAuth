using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System.Web.Http;

[assembly: OwinStartup(typeof(DaOAuth.Api.Startup))]
namespace DaOAuth.Api
{
    public class Startup
    {
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

            HttpConfiguration config = new HttpConfiguration();
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);

            WebApiConfig.Register(config);           
        }        
    }
}