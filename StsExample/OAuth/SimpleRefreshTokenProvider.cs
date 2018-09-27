using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;
using StsExample.Models.Database;
using StsExample.Repositories;

namespace StsExample.OAuth
{
    public class SimpleRefreshTokenProvider : IAuthenticationTokenProvider
    {
        public void Create(AuthenticationTokenCreateContext context)
        {
            CreateAsync(context).RunSynchronously();
        }

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var clientId = context.Ticket.Properties.Dictionary["as:client_id"];

            var refreshTokenId = Guid.NewGuid().ToString("N");

            using (var authenticationRepository = new AuthenticationRepository())
            {
                var refreshTokenLifeTime = context.OwinContext.Get<int?>("as:refreshTokenLifeTime") ?? TimeSpan.FromDays(365).TotalMinutes; //defaults to a year in minutes
                var token = new RefreshToken
                {
                    Id = refreshTokenId.Hash(),
                    ClientId = clientId,
                    Subject = context.Ticket.Identity.Name,
                    IssuedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(refreshTokenLifeTime)
                };

                context.Ticket.Properties.IssuedUtc = token.IssuedUtc;
                context.Ticket.Properties.ExpiresUtc = token.ExpiresUtc;

                token.ProtectedTicket = context.SerializeTicket();

                var result = await authenticationRepository.AddRefreshTokenAsync(token);
                if (result)
                {
                    context.SetToken(refreshTokenId);
                }
            }
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            ReceiveAsync(context).RunSynchronously();
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var allowedOrigins = context.OwinContext.Get<string>("as:clientAllowedOrigin").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (allowedOrigins.Any())
            {
                if (!allowedOrigins.Contains(context.OwinContext.Response.Headers["Access-Control-Allow-Origin"]))
                {
                    context.OwinContext.Response.Headers.Remove("Access-Control-Allow-Origin");
                }
            }

            using (var authenticationRepository = new AuthenticationRepository())
            {
                var refreshTokenId = context.Token.Hash();

                var refreshToken = await authenticationRepository.FindRefreshTokenAsync(refreshTokenId);
                if (refreshToken != null)
                {
                    if (refreshToken.HasExpired())
                    {
                        context.OwinContext.Set("as:refreshTokenExpired", true);
                    }

                    context.DeserializeTicket(refreshToken.ProtectedTicket);
                    await authenticationRepository.RemoveRefreshTokenAsync(refreshTokenId);
                }
            }
        }
    }
}