using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using StsExample.Models.Database;
using StsExample.Repositories;

namespace StsExample.OAuth
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            Client client;

            if (!context.TryGetBasicCredentials(out _, out var clientSecret))
            {
                context.TryGetFormCredentials(out var clientId, out clientSecret);
            }

            if (context.ClientId == null)
            {
                context.SetError("invalid_clientId", "client_id must be set.");
                return;
            }

            using (var repository = new AuthenticationRepository())
            {
                client = await repository.FindClientAsync(context.ClientId);
            }

            if (client == null)
            {
                context.SetError("invalid_clientId", $"Client with client_id: '{context.ClientId}' is not registered.");
                return;
            }

            if (client.ApplicationType == ApplicationTypes.Native)
            {
                if (string.IsNullOrWhiteSpace(clientSecret))
                {
                    context.SetError("invalid_clientId", "client_secret must be set.");
                    return;
                }

                if (client.Secret != clientSecret.Hash())
                {
                    context.SetError("invalid_clientId", "client_secret is invalid.");
                    return;
                }
            }

            if (!client.Active)
            {
                context.SetError("invalid_clientId", "Client is inactive.");
                return;
            }

            context.OwinContext.Set("as:clientAllowedOrigin", client.AllowedOrigin);
            context.OwinContext.Set("as:refreshTokenLifeTime", client.RefreshTokenLifeTime);
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            
            var allowedOrigins = context.OwinContext.Get<string>("as:clientAllowedOrigin").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (allowedOrigins.Any())
            {
                if (!allowedOrigins.Contains(context.OwinContext.Response.Headers["Access-Control-Allow-Origin"]))
                {
                    context.OwinContext.Response.Headers.Remove("Access-Control-Allow-Origin");
                }
            }

            using (var repo = new AuthenticationRepository())
            {
                bool isInvalid;
                if (string.IsNullOrWhiteSpace(context.UserName) || string.IsNullOrWhiteSpace(context.Password))
                {
                    isInvalid = true;
                }
                else
                {
                    var user = await repo.FindUserAsync(context.UserName, context.Password);
                    isInvalid = user == null;
                }

                if (isInvalid)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }
            }


            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim(ClaimTypes.Role, "User"));

            var authenticationProperties = new AuthenticationProperties(new Dictionary<string, string>
            {
                {
                    "as:client_id", context.ClientId ?? string.Empty
                },
                {
                    "userName", context.UserName
                }
            });

            var ticket = new AuthenticationTicket(identity, authenticationProperties);

            context.Validated(ticket);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (var property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }
            return Task.CompletedTask;
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var usedRefreshTokenWasExpired = context.OwinContext.Get<bool>("as:refreshTokenExpired");
            if (usedRefreshTokenWasExpired)
            {
                context.SetError("invalid_refreshtoken", "The refreshtoken has expired.");
                return Task.CompletedTask;
            }

            var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
            var currentClient = context.ClientId;
            if (originalClient != currentClient)
            {
                context.SetError("invalid_clientId", "Refresh token is issued to a different clientId");
                return Task.CompletedTask;
            }

            var newIdentity = new ClaimsIdentity(context.Ticket.Identity); //though here you can change existing claims with updated claims.
            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);
            return Task.CompletedTask;

        }

    }
}