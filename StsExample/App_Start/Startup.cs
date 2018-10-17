using System;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Owin.Security.Providers.Discord;
using StsExample;
using StsExample.OAuth;

[assembly: OwinStartup(typeof(Startup))]
namespace StsExample
{
    public class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthAuthorizationServerOptions { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);

            ConfigureOAuth(app);

            var config = new HttpConfiguration();

            WebApiConfig.Register(config);

            app.UseWebApi(config);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            //In combination with an extra login through the bearer authentication you can also set a cookie, or using the default by supplying a login path which will owin signin.
            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    AuthenticationMode = AuthenticationMode.Active,
            //    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
            //});

            // You only need this when you want external providers to set a cookie and to be able to validate that way.
            //            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            app.UseOAuthAuthorizationServer(OAuthAuthorizationServerOptions = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60),
                Provider = new SimpleAuthorizationServerProvider(),
                RefreshTokenProvider = new SimpleRefreshTokenProvider()
            });

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());


            app.UseDiscordAuthentication(new DiscordAuthenticationOptions
            {
                ClientId = "493794776144150528",
                ClientSecret = "k_b5S2Id6lHQdfPnEg2QPmwSh7CE3kCy",
                Provider = new DiscordAuthorizationProvider(),
                CallbackPath = new PathString("/api/external/discord/"),
                SignInAsAuthenticationType = "None" //We don't want the default login behavior where a cookie is set.
            });
        }
    }
}