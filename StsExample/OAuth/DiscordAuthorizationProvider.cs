using System.Threading.Tasks;
using Owin.Security.Providers.Discord.Provider;

namespace StsExample.OAuth
{
    public class DiscordAuthorizationProvider : DiscordAuthenticationProvider
    {
        public override Task Authenticated(DiscordAuthenticatedContext context)
        {
            
            return base.Authenticated(context);
        }

        public override Task ReturnEndpoint(DiscordReturnEndpointContext context)
        {
            return base.ReturnEndpoint(context);
        }
    }
}