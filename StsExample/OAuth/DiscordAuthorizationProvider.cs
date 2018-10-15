using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Owin.Security.Providers.Discord.Provider;
using StsExample.Models.Database;
using StsExample.Repositories;

namespace StsExample.OAuth
{
    public class DiscordAuthorizationProvider : DiscordAuthenticationProvider
    {
        public override Task Authenticated(DiscordAuthenticatedContext context)
        {
            return base.Authenticated(context);
        }

        public override async Task ReturnEndpoint(DiscordReturnEndpointContext context)
        {
            var clientId = "angular";

            var authenticationProperties = new AuthenticationProperties(new Dictionary<string, string>
            {
                {
                    "as:client_id", clientId
                },
                {
                    "userName", context.Identity.FindFirst(ClaimTypes.Email).Value
                }
            });

            var identity = new ClaimsIdentity(Startup.OAuthAuthorizationServerOptions.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, context.Identity.FindFirst(ClaimTypes.Email).Value));
            identity.AddClaim(new Claim(ClaimTypes.Role, "User"));

            var ticket = new AuthenticationTicket(identity, authenticationProperties);

            var refreshContext = new AuthenticationTokenCreateContext(context.OwinContext, Startup.OAuthAuthorizationServerOptions.RefreshTokenFormat, ticket);
            await Startup.OAuthAuthorizationServerOptions.RefreshTokenProvider.CreateAsync(refreshContext);
            

            context.Response.Cookies.Append("refreshtoken",refreshContext.Token);
            await base.ReturnEndpoint(context);
        }
    }
}