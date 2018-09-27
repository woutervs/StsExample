using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using StsExample.Models.Database;

namespace StsExample.Models
{
    //change this with a model that derives from IdentityUser to add extra properties
    //See: https://odetocode.com/blogs/scott/archive/2014/01/03/asp-net-identity-with-the-entity-framework.aspx

    public class AuthenticationContext : IdentityDbContext<IdentityUser> 
    {
        public AuthenticationContext() : base("AuthenticationContext")
        {
            
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}