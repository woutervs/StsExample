using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Owin.Security.Providers.Discord.Provider;
using StsExample.Helpers;
using StsExample.Helpers.EmailNotVerified;
using StsExample.Models.View;
using StsExample.Repositories;

namespace StsExample.OAuth
{
    public class DiscordAuthorizationProvider : DiscordAuthenticationProvider
    {
        public override async Task Authenticated(DiscordAuthenticatedContext context)
        {
            if (context.Verified)
            {
                var authenticationRepository = new AuthenticationRepository();
                var user = await authenticationRepository.FindUserAsync(context.Email);
                if (user == null) //if user is unknown create him in the system.
                {
                    await authenticationRepository.RegisterUserAsync(new UserViewModel
                    {
                        UserName = context.Email,
                        Password = Guid.NewGuid().ToString("N")
                    });
                }
            }
            context.Properties.Dictionary.Add("userName", context.Email);
            context.OwinContext.Set("discord:verified", context.Verified);
            await base.Authenticated(context);
        }

        public override async Task ReturnEndpoint(DiscordReturnEndpointContext context)
        {
            if (context.OwinContext.Get<bool>("discord:verified"))
            {
                var authenticationProperties = context.Properties;
                var identity = new ClaimsIdentity(Startup.OAuthAuthorizationServerOptions.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.Name, context.Identity.FindFirst(ClaimTypes.Email).Value));
                identity.AddClaim(new Claim(ClaimTypes.Role, "User"));

                var ticket = new AuthenticationTicket(identity, authenticationProperties);

                var refreshContext = new AuthenticationTokenCreateContext(context.OwinContext, Startup.OAuthAuthorizationServerOptions.RefreshTokenFormat, ticket);
                await Startup.OAuthAuthorizationServerOptions.RefreshTokenProvider.CreateAsync(refreshContext);

                context.Response.Cookies.Append("refreshtoken", refreshContext.Token);
            }
            else
            {
                context.Response.StatusCode = 403;
                context.Response.ReasonPhrase = "Account on discord is not verified so STSExample will not allow authentication this way.";

                var model = new EmailNotVerifiedModel
                {
                    ReturnUrl = context.RedirectUri,
                    FontAwesomeIconName = "fa-discord",
                    Provider = "Discord"
                };
                using (var sr = new StreamWriter(context.Response.Body))
                {
                    var transformPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Helpers/EmailNotVerified/EmailNotVerified.html");
                    sr.Write(string.IsNullOrWhiteSpace(transformPath) ? "Error: Can't render page." : TransformXsltHelper.Transform(model, transformPath));
                }
            }
            await base.ReturnEndpoint(context);
        }
    }
}