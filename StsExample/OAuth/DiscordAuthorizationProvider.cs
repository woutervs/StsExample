using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Owin.Security.Providers.Discord.Provider;
using StsExample.Models.Database;
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
                //authenticationProperties.Dictionary.Add("userName", context.Identity.FindFirst(ClaimTypes.Email).Value);

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
                using (var sr = new StreamWriter(context.Response.Body))
                {
                    sr.Write(@"<!doctype html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <title>Oops! An error occured.</title>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <link rel=""stylesheet"" href=""https://use.fontawesome.com/releases/v5.3.1/css/all.css"" integrity=""sha384-mzrmE5qonljUremFsqc01SB46JvROS7bZs3IO2EmfFsd15uHvIt+Y8vEf7N7fWAU""
        crossorigin=""anonymous"">
    <link rel=""stylesheet"" href=""https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css"" integrity=""sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO""
        crossorigin=""anonymous"">
</head>
<body>
    <div class=""card w-50 mx-auto mt-5"">
        <i class=""fab fa-discord fa-10x mx-auto""></i>
        <div class=""card-body"">
            <h5 class=""card-title"">Unable to connect.</h5>
            <p class=""card-text"">Your account on Discord does not seem verified, so we can't connect to STSExample</p>
            <a href=""{0}"" class=""btn btn-primary"">Take me back!</a>
        </div>
    </div>
</body>
</html>", context.RedirectUri);
                }
            }
            await base.ReturnEndpoint(context);
        }
    }
}