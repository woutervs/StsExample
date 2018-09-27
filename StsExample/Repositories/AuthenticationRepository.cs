using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using StsExample.Models;
using StsExample.Models.Database;
using StsExample.Models.View;

namespace StsExample.Repositories
{
    public class AuthenticationRepository : IDisposable
    {
        private readonly AuthenticationContext context;
        private readonly UserManager<IdentityUser> userManager;

        public AuthenticationRepository()
        {
            context = new AuthenticationContext();
            userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(context));
        }

        public async Task<IdentityResult> RegisterUserAsync(UserViewModel userViewModel)
        {
            var user = new IdentityUser
            {
                UserName = userViewModel.UserName
            };

            var result = await userManager.CreateAsync(user, userViewModel.Password);

            return result;
        }

        public async Task<IdentityUser> FindUserAsync(string userName, string password)
        {
            var user = await userManager.FindAsync(userName, password);

            return user;
        }

        public async Task<Client> FindClientAsync(string clientId)
        {
            return await context.Clients.FindAsync(clientId);
        }

        public async Task<bool> AddRefreshTokenAsync(RefreshToken token)
        {

            var existingToken = context.RefreshTokens.SingleOrDefault(r => r.Subject == token.Subject && r.ClientId == token.ClientId);

            if (existingToken != null)
            {
                await RemoveRefreshTokenAsync(existingToken);
            }

            context.RefreshTokens.Add(token);

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveRefreshTokenAsync(string refreshTokenId)
        {
            var refreshToken = await context.RefreshTokens.FindAsync(refreshTokenId);
            if (refreshToken == null) return false;
            context.RefreshTokens.Remove(refreshToken);
            return await context.SaveChangesAsync() > 0;

        }

        public async Task<bool> RemoveRefreshTokenAsync(RefreshToken refreshToken)
        {
            context.RefreshTokens.Remove(refreshToken);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<RefreshToken> FindRefreshTokenAsync(string refreshTokenId)
        {
            var refreshToken = await context.RefreshTokens.FindAsync(refreshTokenId);

            return refreshToken;
        }

        public List<RefreshToken> GetAllRefreshTokens()
        {
            return context.RefreshTokens.ToList();
        }

        public void Dispose()
        {
            context.Dispose();
            userManager.Dispose();
        }
    }
}